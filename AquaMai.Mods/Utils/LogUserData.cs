using System;
using System.Collections.Generic;
using System.IO;

using AquaMai.Config.Attributes;
using MelonLoader;

using System.Text.RegularExpressions;

namespace AquaMai.Mods.Utils
{
    [ConfigSection(
    en: "Log user Data on login.",
    zh: "登录时将记录 UserData ")]
    public class LogUserData
    {
            // CSV 文件路径
        private static string csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "user_data_list.csv");

        // 预定义 CSV 表头（字段顺序固定）
        private static readonly string[] CsvHeaders =
        {
            "userId", "userName", "isLogin", "lastGameId", "lastDataVersion", "lastRomVersion",
            "lastLoginDate", "lastPlayDate", "playerRating", "nameplateId", "iconId", "trophyId",
            "partnerId", "frameId", "totalAwake", "isNetMember", "dailyBonusDate", "headPhoneVolume",
            "dispRate", "isInherit", "banState"
        };

        public static void parseData(string message)
        {
            MelonLogger.Msg($"[LogUserData] Response: {message}");

            try
            {

                Dictionary<string, string> parsedData = ParseJsonString(message);

                // 保存到 CSV 文件
                SaveToCsv(parsedData);


                // 输出解析结果
                foreach (var kvp in parsedData)
                {
                    MelonLogger.Warning($"{kvp.Key}: {kvp.Value}");
                }

            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LogUserData] 解析 JSON 失败: {ex}");
            }
        }

        public static Dictionary<string, string> ParseJsonString(string jsonMessage)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            try
            {
                // 正则表达式提取 "key":"value" 结构
                string pattern = @"""(.*?)"":(.*?)(,|})";
                MatchCollection matches = Regex.Matches(jsonMessage, pattern);

                foreach (Match match in matches)
                {
                    string key = match.Groups[1].Value;  // 键名
                    string value = match.Groups[2].Value.Trim(); // 值，去掉空格

                    // 去掉字符串值的引号
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    // 处理布尔值（true/false）
                    if (value == "true" || value == "false")
                    {
                        value = value.ToLower();
                    }

                    // 处理 null 值
                    if (value == "null")
                    {
                        value = "";
                    }

                    result[key] = value;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[JsonParser] 解析 JSON 失败: {ex.Message}");
            }
            return result;
        }


        /// <summary>
        /// 读取现有的 CSV 数据，检查 userId 是否已存在
        /// </summary>
        private static HashSet<string> LoadExistingUserIds()
        {
            HashSet<string> existingUserIds = new HashSet<string>();

            if (!File.Exists(csvFilePath))
            {
                return existingUserIds; // 文件不存在，返回空集合
            }

            try
            {
                using (StreamReader reader = new StreamReader(csvFilePath))
                {
                    string? line;
                    bool isFirstLine = true; // 第一行为表头，跳过

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue;
                        }

                        string[] columns = line.Split(',');
                        if (columns.Length > 0)
                        {
                            existingUserIds.Add(columns[0]); // 第一列是 userId
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LogUserData] 读取 CSV 失败: {ex}");
            }

            return existingUserIds;
        }


        /// <summary>
        /// 将解析后的数据保存到 CSV 文件
        /// </summary>
        private static void SaveToCsv(Dictionary<string, string> parsedData)
        {
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(csvFilePath));


                bool fileExists = File.Exists(csvFilePath);

                // 读取已有的 userId
                HashSet<string> existingUserIds = LoadExistingUserIds();
                // 获取当前 userId
                if (!parsedData.TryGetValue("userId", out string? userId) || string.IsNullOrWhiteSpace(userId))
                {
                    MelonLogger.Warning("[LogUserData] 无法获取 userId，跳过存储");
                    return;
                }

                // 检查 userId 是否已存在
                if (existingUserIds.Contains(userId))
                {
                    MelonLogger.Msg($"[LogUserData] userId {userId} 已存在，跳过存储");
                    return;
                }


                using (StreamWriter writer = new StreamWriter(csvFilePath, true))
                {
                    // 如果文件不存在，先写入表头
                    if (!fileExists)
                    {
                        writer.WriteLine(string.Join(",", CsvHeaders));
                    }

                    // 按照固定表头顺序写入数据
                    List<string> values = new List<string>();
                    foreach (string header in CsvHeaders)
                    {
                        // 如果解析数据中包含该字段，则写入值，否则填充为空
                        values.Add(parsedData.ContainsKey(header) ? parsedData[header] : "");
                    }

                    writer.WriteLine(string.Join(",", values));
                }

                MelonLogger.Msg($"[LogUserData] 数据成功写入 CSV 文件: {csvFilePath}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LogUserData] 写入 CSV 失败: {ex}");
            }
        }

    }
}

