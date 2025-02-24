using System;
using AquaMai.Config.Attributes;
using HarmonyLib;
using Net.Packet.Mai2;
using Net.Packet;
using Net.VO.Mai2;
using MelonLoader;

namespace AquaMai.Mods.Utils
{
    [ConfigSection(
        en: "Log user login information and statistics.",
        zh: "记录用户登录信息和统计数据")]
    public class LogUserLogin
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PacketGetUserPreview), MethodType.Constructor,
            typeof(ulong), typeof(string), typeof(Action<ulong, UserPreviewResponseVO>), typeof(Action<PacketStatus>))]
        public static void Postfix(object __instance, ulong userId, string authKey,
            ref Action<ulong, UserPreviewResponseVO> onDone, Action<PacketStatus> onError)
        {
            MelonLogger.Msg("[LogUserLogin] HarmonyPostfix triggered."); // 调试日志

            // 记录用户ID和AuthKey
            MelonLogger.Msg($"[LogUserLogin] UserLogin: {userId} with AuthKey: {authKey}");

            // 包装原始 onDone，确保 response 有值
            Action<ulong, UserPreviewResponseVO> originalOnDone = onDone;

            onDone = (id, response) =>
            {
                // 检查 response 是否为 null
                if (response == null)
                {
                    MelonLogger.Msg($"[LogUserLogin] Error: UserPreviewResponseVO is null for user {userId}.");
                    return;
                }

                // 检查 userName 是否为 null 或空
                string userName = response.userName ?? "Unknown User"; // 默认值
                MelonLogger.Msg($"[LogUserLogin] User {userId} name: {userName}");

                // 调用原始 onDone
                if (originalOnDone != null)
                {
                    try
                    {
                        originalOnDone(id, response);
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Msg($"[LogUserLogin] Error invoking original onDone: {ex.Message}");
                    }
                }
                else
                {
                    MelonLogger.Msg("[LogUserLogin] originalOnDone is null.");
                }
            };
        }
    }
}