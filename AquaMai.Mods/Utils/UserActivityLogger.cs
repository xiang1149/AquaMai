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
        [HarmonyPatch(typeof(PacketUserLogin))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(ulong), typeof(string), typeof(bool), typeof(int), typeof(Action<UserLoginResponseVO>), typeof(Action<PacketStatus>) })]
        public static class UserActivityLoggerPatch
        {
            [HarmonyPostfix]
            public static void Postfix(PacketUserLogin __instance, ulong userId, string acsessCode, bool isContinue, int genericFlag, Action<UserLoginResponseVO> onDone, Action<PacketStatus> onError)
            {
                DateTime loginTime = DateTime.Now;
                MelonLogger.Msg($"[UserActivityLogger] ID:{userId} 用户登录：accessCode {acsessCode} 于 {loginTime:yyyy-MM-dd HH:mm:ss}");
            }
        }





        private static void LogStatistics()
        {

        }
    }
}
