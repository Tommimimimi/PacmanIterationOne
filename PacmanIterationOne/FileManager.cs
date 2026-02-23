using pIterationOne;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PacmanIterationOne
{
    //defines a single leaderboard entry with name, score and date
    public class LeaderBoardEntry
    {
        public string Name { get; set; } = "unknown";
        public int Score { get; set; }
        public string Date { get; set; } = "";
    }

    internal class FileManager
    {
        //resolves the project root three levels above the build output
        public static string BaseFolderDirectory()
        {
            DirectoryInfo? directory = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent;
            if (directory is null)
                throw new DirectoryNotFoundException("directory not found");
            else
                return directory.FullName;
        }

        //full path to leaderboard json file
        private static readonly string LeaderBoard = BaseFolderDirectory() + @"\LeaderBoard.Json";

        //shared json settings
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
            //leaderboard file found
            if (File.Exists(LeaderBoard))
            { return; }

            //leaderboard file not found, creating new file with empty array
            else { File.WriteAllText(LeaderBoard, "[]"); }
        }

        public static void AppendScore(string playerName, int score)
        {
            //read existing entries or start fresh if file is empty or corrupt
            List<LeaderBoardEntry> entries = new();

            try
            {
                string json = File.ReadAllText(LeaderBoard);
                if (!string.IsNullOrWhiteSpace(json) && json.Trim() != "[]")
                {
                    entries = JsonSerializer.Deserialize<List<LeaderBoardEntry>>(json, jsonConfig)
                              ?? new List<LeaderBoardEntry>();
                }
            }
            catch
            {
                //start fresh if anything goes wrong reading
                entries = new List<LeaderBoardEntry>();
            }

            //add new entry with name and timestamp
            entries.Add(new LeaderBoardEntry
            {
                Name  = playerName,
                Score = score,
                Date  = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });

            //bubble sort highest score to top
            BubbleSortDescending(entries);

            //cap at top 10 so the file doesnt grow forever
            if (entries.Count > 10)
                entries = entries.GetRange(0, 10);

            //write updated list back to file
            File.WriteAllText(LeaderBoard, JsonSerializer.Serialize(entries, jsonConfig));
        }

        //bubble sort entries in descending order by score
        private static void BubbleSortDescending(List<LeaderBoardEntry> entries)
        {
            int n = entries.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - 1 - i; j++)
                {
                    //swap if left score is smaller than right score
                    if (entries[j].Score < entries[j + 1].Score)
                    {
                        LeaderBoardEntry temp = entries[j];
                        entries[j] = entries[j + 1];
                        entries[j + 1] = temp;
                    }
                }
            }
        }

        //returns all stored scores sorted by highest first
        public static List<LeaderBoardEntry> ReadScores()
        {
            try
            {
                string json = File.ReadAllText(LeaderBoard);
                if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
                    return new List<LeaderBoardEntry>();

                List<LeaderBoardEntry> entries = JsonSerializer.Deserialize<List<LeaderBoardEntry>>(json, jsonConfig)
                                                 ?? new List<LeaderBoardEntry>();

                //sort again on read in case file was edited externally
                BubbleSortDescending(entries);
                return entries;
            }
            catch
            {
                return new List<LeaderBoardEntry>();
            }
        }
    }
}
