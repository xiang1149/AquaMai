using System;
using AquaMai.Config.Attributes;
using HarmonyLib;
using MelonLoader;
using Net.Packet.Mai2;

using Net.Packet;
using Net.VO.Mai2;
using System.IO;
using System.Linq;
using AMDaemon;



namespace AquaMai.Mods.Utils
{
    [ConfigSection(
    en: "Log user login information and statistics.",
    zh: "记录用户登录信息和统计数据")]
    public class UserActivityLogger
    {
        // CSV 文件路径
        private static string csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "user_activity_log.csv");

        [HarmonyPatch(typeof(PacketUserLogin))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(ulong), typeof(string), typeof(bool), typeof(int), typeof(Action<UserLoginResponseVO>), typeof(Action<PacketStatus>) })]
        public static class UserLoginActivityLoggerPatch
        {
            [HarmonyPostfix]
            public static void Postfix(PacketUserLogin __instance, ulong userId, string acsessCode, bool isContinue, int genericFlag, Action<UserLoginResponseVO> onDone, Action<PacketStatus> onError)
            {
                DateTime loginTime = DateTime.Now;
                MelonLogger.Msg($"[UserActivityLogger] ID:{userId} 用户登录：accessCode {acsessCode} 于 {loginTime:yyyy-MM-dd HH:mm:ss} ");
                try
                {
                    // 标记掉线的记录
                    MarkDisconnectedUsers(userId, loginTime);
                    // 保存登录信息到 CSV 文件
                    SaveUserLoginInfo(userId, acsessCode, loginTime);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[UserActivityLogger] 发生异常: {ex}");
                }

            }

        }

        // 处理用户登出
        [HarmonyPatch(typeof(PacketUserLogout))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] {typeof(ulong),typeof(LogoutType),typeof(string),typeof(Action),typeof(Action<PacketStatus>)})]

        public static class UserLogoutActivityLoggerPatch
        {
            [HarmonyPostfix]
            public static void PostfixLogout(PacketUserLogout __instance, ulong userId, LogoutType logoutType, string acsessCode, Action onDone, Action<PacketStatus> onError)
            {
                DateTime logoutTime = DateTime.Now;
                MelonLogger.Msg($"[UserActivityLogger] 触发了UserLogoutActivityLoggerPatch");
                MelonLogger.Msg($"[UserActivityLogger] ID:{userId} 用户登出：accessCode {acsessCode} 于 {logoutTime:yyyy-MM-dd HH:mm:ss}登出");
                // 更新登出信息到 CSV 文件
                UpdateUserLogoutInfo(userId, logoutTime);
            }
        }


        // 保存用户登录信息到 CSV 文件
        private static void SaveUserLoginInfo(ulong userId, string accessCode, DateTime loginTime)
        {
            // 如果文件不存在，创建文件并写入表头
            if (!File.Exists(csvFilePath))
            {
                File.WriteAllText(csvFilePath, "UserId,AccessCode,LoginTime,LogoutTime,HasLoggedOut\n");
            }

            // 写入登录记录
            MelonLogger.Msg($"写入登录记录");
            string logEntry = $"{userId},{accessCode},{loginTime:yyyy-MM-dd HH:mm:ss},,1\n";
            File.AppendAllText(csvFilePath, logEntry);
        }

        // 更新用户登出信息到 CSV 文件
        private static void UpdateUserLogoutInfo(ulong userId, DateTime logoutTime)
        {
            if (!File.Exists(csvFilePath))
            {
                MelonLogger.Warning($"[UserActivityLogger] CSV 文件不存在，无法更新登出信息。");
                return;
            }

            // 读取所有行
            var lines = File.ReadAllLines(csvFilePath).ToList();

            // 查找最近的未登出记录
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                var columns = lines[i].Split(',');
                if (columns.Length >= 5 && ulong.TryParse(columns[0], out ulong recordUserId) && recordUserId == userId && columns[4] == "1")
                {
                    // 更新登出时间和状态
                    columns[3] = logoutTime.ToString("yyyy-MM-dd HH:mm:ss");
                    columns[4] = "2"; // 标记为已登出
                    lines[i] = string.Join(",", columns);

                    // 写回文件
                    File.WriteAllLines(csvFilePath, lines);

                    MelonLogger.Msg($"[UserActivityLogger] 用户 {userId} 的最近登录记录已更新为已登出，登出时间: {logoutTime:yyyy-MM-dd HH:mm:ss}");
                    return;
                }
            }

            MelonLogger.Warning($"[UserActivityLogger] 用户 {userId} 没有找到未登出的记录，可能已掉线"); 
        }


        // **查找并标记掉线用户**
        private static void MarkDisconnectedUsers(ulong userId, DateTime timeOut)
        {
            if (!File.Exists(csvFilePath))
            {
                return;
            }

            var lines = File.ReadAllLines(csvFilePath).ToList();
            bool updated = false;

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                var columns = lines[i].Split(',');
                if (columns.Length >= 5 && ulong.TryParse(columns[0], out ulong recordUserId) && recordUserId == userId && columns[4] == "1")
                {
                    columns[3] = timeOut.ToString("yyyy-MM-dd HH:mm:ss");
                    columns[4] = "3"; // 标记为掉线
                    lines[i] = string.Join(",", columns);
                    updated = true;
                }
            }

            if (updated)
            {
                File.WriteAllLines(csvFilePath, lines);
                MelonLogger.Msg($"[UserActivityLogger] 用户 {userId} 掉线记录已标记为 3（掉线）。");
            }
        }

    }
}

