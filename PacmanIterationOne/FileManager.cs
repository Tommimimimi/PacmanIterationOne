using pIterationOne;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PacmanIterationOne
{
    internal class FileManager
    {
        public static string BaseFolderDirectory()
        {
        DirectoryInfo? directory = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent;
            if (directory is null) 
                throw new DirectoryNotFoundException("directory not found");
            else
                return directory.FullName;
        }


        private static readonly string LeaderBoard = BaseFolderDirectory() + "LeaderBoard.Json";

        private static JsonSerializerOptions jsonConfig = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        public static void CreateDir()
        {
            if (!File.Exists(LeaderBoard))
            {
                MessageBox.Show("leaderboard file found");
                return; }
            else { MessageBox.Show("Leaderboard file not found, creating new file...");
                File.Create(LeaderBoard); }
        }
    }
}
