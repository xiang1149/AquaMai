using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;


using AquaMai.Config.Attributes;
using HarmonyLib;
using MelonLoader;
using Net.Packet.Mai2;

using Net.Packet;
using Net.VO.Mai2;
using AMDaemon;
using Manager.UserDatas;
using Mono.Posix;


namespace AquaMai.Mods.Utils
{
    [ConfigSection(
    en: "Log user login information and statistics.",
    zh: "记录用户登录信息和统计数据")]
    public class UserActivityLogger
    {
        //// 存储用户登录信息的字典，键为userId，值为用户名和登录时间的元组
        //private static readonly Dictionary<ulong, (string UserName, DateTime LoginTime)> userLogins = new();

        //// 存储玩家ID和PC记录的列表
        //private static readonly Dictionary<ulong, (string UserName, DateTime LoginTime)> userActivity = new();

        //// 统计当日总PC数
        //private static int totalPcToday = 0;

        //// 定时器，用于每15分钟统计一次游玩人数
        //private static readonly Timer statisticsTimer = new(15 * 60 * 1000);

        //static UserActivityLogger()
        //{
        //    // 设置定时器的回调函数
        //    statisticsTimer.Elapsed += (sender, e) => LogStatistics();
        //    statisticsTimer.AutoReset= true;
        //    statisticsTimer.Enabled= true;
        //}

        // 拦截UserLoginApi的响应，记录用户登录信息
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PacketUserLogin), MethodType.Constructor, typeof(ulong) , typeof(string) , typeof(bool) , typeof(bool) , typeof(Action<UserLoginResponseVO>) ,typeof(Action<PacketStatus>))]
        public static void OnUserLogin(ulong userId,string accessCode)
        {

            DateTime loginTime = DateTime.Now;

            MelonLogger.Msg($"[UserActivityLogger] ID:{userId} 用户登录：accessCode {accessCode} 于 {loginTime:yyyy-MM-dd HH:mm:ss}");
        }




        private static void LogStatistics()
        {

        }
    }
}
