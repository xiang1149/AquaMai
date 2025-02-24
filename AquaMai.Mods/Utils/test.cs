using System;
using System.Data.SQLite;
using MelonLoader;  // MelonLoader 日志输出

namespace AquaMai.Mods.Utils
{
    internal class Test
    {
        private static string connectionString = "Server=localhost;Database=aquamai;User=root;Password=114514;";

        public static void TestMySqlConnection()
        {
            string connectionString = "Data Source=test.db;Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                MelonLogger.Error($"[MySQL] 数据库连接失败");
            }              
        }
    }

}
