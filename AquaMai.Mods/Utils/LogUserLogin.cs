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



namespace AquaMai.Mods.Utils
{
    [ConfigSection(
        en: "Log user login information and statistics.",
        zh: "记录用户登录信息和统计数据")]
    public class LogUserLogin
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PacketGetUserPreview), MethodType.Constructor, typeof(ulong), typeof(string), typeof(Action<ulong, UserLoginResponseVO>), typeof(Action<PacketStatus>))]
        public static void Postfix(object __instance, ulong userId, string authKey, Action<ulong, UserPreviewResponseVO> onDone, Action<PacketStatus> onError)
        {

            MelonLogger.Msg($"[LogUserLogin] UserLogin: {userId} with AuthKey: {authKey}");
        }

    }
}
