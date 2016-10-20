using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MDP_model
{
    internal class State_bin
    {
        string path = MyGlobals.path;

        Dictionary<string, int> sourceType_dict = new Dictionary<string, int>()
            {
             {"1", 1 },{"2", 2 },{"3", 3 },{"4", 4 },{"7", 5 },{"11", 6 },{"12", 7 },{"14", 8 },{"15", 9 },{"17", 10 },{"18", 11 },{"19", 12 },{"21", 13 },{"24", 14 },{"25", 15 },{"28", 16 },{"blank",17}
            };

        Dictionary<string, List<int>> RGUID_action = new Dictionary<string, List<int>>();

        public int get_bin_pclick(string str)
        {
            var num = float.Parse(str);

            if (0 <= num && num <= 0.0263)
                return 1;
            else if (0.0264 <= num && num <= 0.0602)
                return 2;
            else if (0.0603 <= num && num <= 0.1095)
                return 3;
            else if (0.1096 <= num && num <= 0.1855)
                return 4;
            else //if (0.1856 <= num && num <= 1)
                return 5;
        }

        public int get_bin_relevance(string str)
        {
            var num = float.Parse(str);

            if (0 <= num && num <= 0.5723)
                return 1;
            else if (0.5724 <= num && num <= 0.7297)
                return 2;
            else if (0.7298 <= num && num <= 0.8057)
                return 3;
            else if (0.8058 <= num && num <= 0.9059)
                return 4;
            else //if (0.9060 <= num && num <= 1)
                return 5;
        }

        public int get_bin_matchType(string matchType)
        {
            var matchType_int = -1;

            if (matchType == "ExactMatch")
                matchType_int = 1;
            else if (matchType == "SmartMatch")
                matchType_int = 2;
            else if (matchType == "BroadMatch")
                matchType_int = 3;
            else if (matchType == "PhraseMatch")
                matchType_int = 4;

            return matchType_int;
        }

        public int get_bin_sourceType(string sourceType)
        {
            if (sourceType == "")
                return sourceType_dict["blank"];
            else
                return sourceType_dict[sourceType];
        }

        public int get_bin_Rank(string Rank)
        {
            return int.Parse(Rank) + 1;
        }

        public List<int> get_action(List<string> clicks)
        {
            List<int> actions = new List<int>();
            int pos = clicks.IndexOf("1");
            int len = clicks.Count;
            if (pos == -1)
            {
                for (int i = 0; i < len; i++)
                    actions.Add(3);
            }
            else
            {
                for (int i = 1; i < pos; i++)
                    actions.Add(4);

                for (int i = pos; i < len; i++)
                {
                    if (clicks[i] == "1")
                        actions.Add(1);
                    else if (clicks[i - 1] == "1" && clicks[i] == "0")
                        actions.Add(2);
                    else if (clicks[i - 1] == "0" && clicks[i] == "0")
                        actions.Add(3);
                }
                actions.Add(3);//last transition is always abandon
            }

            //to print the click
            /* Console.WriteLine(); 
             for (int i = 0; i < clicks.Count; i++)
                 Console.Write(clicks[i]);
             Console.WriteLine();
             */
            return actions;
        }

        public Dictionary<string, List<int>> read_csv()
        {
            using (StreamReader reader = new StreamReader(path+MyGlobals.csv_file))
            {
                StreamWriter writer = new StreamWriter(new FileStream(path + "ads_to_states.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
                StreamWriter writer_action = new StreamWriter(new FileStream(path + "ads_to_action.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
                reader.ReadLine();

                List<String> clicks = new List<string>();
                var RGUID_old = "";
                //273730

                for (int i = 0; i < 273730; i++)
                // while ((line = reader.ReadLine()) != null)
                {

                    var line = reader.ReadLine();
                    var row = line.Split(',');
                    var RGUID = row[0];
                    var sourceType = row[3];
                    var Rank = row[6];
                    var pClick = row[7];
                    var matchType = row[9];
                    var relevance = row[16];
                    var rankScore = row[15];
                

                    var state_no = get_bin_sourceType(sourceType) + get_bin_Rank(Rank).ToString() + get_bin_matchType(matchType).ToString() + get_bin_pclick(pClick).ToString() + get_bin_relevance(relevance).ToString();
                    var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", RGUID, get_bin_sourceType(sourceType), Rank, get_bin_matchType(matchType), pClick, relevance, state_no,rankScore);

                    var isClicked = row[1];
                    var isPreviousClicked = row[2];

                    clicks.Add(isClicked);

                    if (isPreviousClicked == "5")
                    {
                        clicks.RemoveAt(clicks.Count - 1);
                        var actions = get_action(clicks);
                        RGUID_action.Add(RGUID_old, actions);

                        writer_action.WriteLine(RGUID_old + "," + string.Join(",", actions.ToArray()));

                        //for (var p = 0; p < actions.Count; p++)
                        //        Console.Write(actions[p]);

                        clicks.Clear();
                        clicks.Add(isPreviousClicked);
                        clicks.Add(isClicked);

                    }
                    

                    if (relevance != (-1).ToString())
                    {
                        writer.WriteLine(newLine);
                    }

                    RGUID_old = RGUID;

                }

                RGUID_action.Add(RGUID_old, get_action(clicks));
                writer_action.WriteLine(RGUID_old + "," + string.Join(",", get_action(clicks).ToArray()));
                reader.Close();
                writer.Close();
                writer_action.Close();

            }

            return RGUID_action;
        }
    }
}