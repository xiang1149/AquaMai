using System;
using AquaMai.Config.Attributes;
using HarmonyLib;
using MelonLoader;
using Net.Packet.Mai2;

using Net.Packet;
using Net.VO.Mai2;



namespace AquaMai.Mods.Utils
{
    [ConfigSection(
    en: "Log user login information and statistics.",
    zh: "记录用户登录信息和统计数据")]
    public class UserActivityLogger
    {
        // 数据库连接字符串
        private static string connectionString = "Server=localhost;Database=aquamai;User=root;Password=114514;";

        [HarmonyPatch(typeof(PacketUserLogin))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(ulong), typeof(string), typeof(bool), typeof(int), typeof(Action<UserLoginResponseVO>), typeof(Action<PacketStatus>) })]
        public static class UserLoginActivityLoggerPatch
        {
            [HarmonyPostfix]
            public static void Postfix(PacketUserLogin __instance, ulong userId, string acsessCode, bool isContinue, int genericFlag, Action<UserLoginResponseVO> onDone, Action<PacketStatus> onError)
            {
                DateTime loginTime = DateTime.Now;
                string sessionId = Guid.NewGuid().ToString();
                MelonLogger.Msg($"Test执行前");
                Test.TestMySqlConnection();
                MelonLogger.Msg($"Test执行后");
                MelonLogger.Msg($"[UserActivityLogger] ID:{userId} 用户登录：accessCode {acsessCode} 于 {loginTime:yyyy-MM-dd HH:mm:ss} 会话ID: {sessionId}");
                // 保存登录信息到数据库
                //SaveUserLoginInfo(userId, acsessCode, loginTime);
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
                MelonLogger.Msg($"[UserActivityLogger] ID:{userId} 用户登录：accessCode {acsessCode} 于 {logoutTime:yyyy-MM-dd HH:mm:ss}登出");
                //UpdateUserLogoutInfo(userId, logoutTime);
            }
        }


        // 记录用户登录信息
        //private static void SaveUserLoginInfo(ulong userId, string accessCode, DateTime loginTime)
        //{
        //    using (var connection = new MySqlConnection(connectionString))
        //    {
        //        connection.Open();

        //        // 关闭所有未登出的旧记录（hasLoggedOut = 1 → 3，表示掉线）
        //        //string closeOldQuery = "UPDATE user_activity SET hasLoggedOut = 3 WHERE userId = @userId AND hasLoggedOut = 1";
        //        //using (var cmd = new MySqlCommand(closeOldQuery, connection))
        //        //{
        //        //    cmd.Parameters.AddWithValue("@userId", userId);
        //        //    int rowsAffected = cmd.ExecuteNonQuery();
        //        //    MelonLogger.Msg($"[UserActivityLogger] 标记 {rowsAffected} 条旧的未登出记录为掉线 (hasLoggedOut = 3) (userId: {userId})");
        //        //}

        //        // 插入新的登录记录
        //        string insertQuery = "INSERT INTO User_Activity (userId, accessCode, loginTime, hasLoggedOut) VALUES (@userId, @accessCode, @loginTime, 1)";
        //        using (var cmd = new MySqlCommand(insertQuery, connection))
        //        {
        //            cmd.Parameters.AddWithValue("@userId", userId);
        //            cmd.Parameters.AddWithValue("@accessCode", accessCode);
        //            cmd.Parameters.AddWithValue("@loginTime", loginTime);
        //            int rowsInserted = cmd.ExecuteNonQuery();
        //            MelonLogger.Msg($"[UserActivityLogger] 插入 {rowsInserted} 条新的登录记录 (userId: {userId}, accessCode: {accessCode}, loginTime: {loginTime:yyyy-MM-dd HH:mm:ss})");
        //        }
        //    }
        //}

        //// 记录用户登出信息
        //private static void UpdateUserLogoutInfo(ulong userId, DateTime logoutTime)
        //{
        //    using (var connection = new MySqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        // 只更新最近的未登出记录（hasLoggedOut = 1）
        //        string updateQuery = "UPDATE user_activity SET logoutTime = @logoutTime, hasLoggedOut = 2 WHERE userId = @userId AND hasLoggedOut = 1 ORDER BY loginTime DESC LIMIT 1";
        //        using (var cmd = new MySqlCommand(updateQuery, connection))
        //        {
        //            cmd.Parameters.AddWithValue("@userId", userId);
        //            cmd.Parameters.AddWithValue("@logoutTime", logoutTime);
        //            int rowsAffected = cmd.ExecuteNonQuery();

        //            if (rowsAffected > 0)
        //            {
        //                MelonLogger.Msg($"[UserActivityLogger] 用户 {userId} 的最近登录记录已更新为已登出 (hasLoggedOut = 2), 登出时间: {logoutTime:yyyy-MM-dd HH:mm:ss}");
        //            }
        //            else
        //            {
        //                MelonLogger.Warning($"[UserActivityLogger] 用户 {userId} 没有找到未登出的记录，可能已掉线");
        //            }
        //        }
        //    }
        //}
    }
}

