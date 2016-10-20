using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//This class will provide three Dictinary for s->a, s->s, s->a->s 
namespace MDP_model
{
    internal class Transition_Model
    {
        string path = MyGlobals.path;

        public Dictionary<Tuple<string, string>, int> count_transitions; //s->s
        public Dictionary<Tuple<string, int, string>, int> count_transitions_action; //s->a->s
        public Dictionary<Tuple<string, int>, int> count_action; //s->a
        public Dictionary<string, Tuple<Double, int>> state_rankscore; //s->(R,count)
        public Dictionary<Tuple<string, int, string>, Double> trans_prob;  //s->a->s->(Probability)


        Dictionary<string, List<int>> RGUID_action;
        public Transition_Model(Dictionary<string, List<int>> actions)
        {
            count_transitions = new Dictionary<Tuple<string, string>, int>();
            count_transitions_action = new Dictionary<Tuple<string, int, string>, int>();
            count_action = new Dictionary<Tuple<string, int>, int>();
            state_rankscore = new Dictionary<string, Tuple<Double, int>>();
            trans_prob = new Dictionary<Tuple<string, int, string>, Double>();
            RGUID_action = actions;
        }

        //fn to add a link tuple in dictionary and increase its count if its already present else create new
        void add_link(Tuple<string, string> trans)
        {
            if (count_transitions.ContainsKey(trans))
                count_transitions[trans] += 1;
            else
                count_transitions.Add(trans, 1);

        }

        void add_link_action(Tuple<string, int, string> trans_action)
        {
            if (count_transitions_action.ContainsKey(trans_action))
                count_transitions_action[trans_action] += 1;
            else
                count_transitions_action.Add(trans_action, 1);

        }

        void add_action(Tuple<string, int> trans_action)
        {
            if (count_action.ContainsKey(trans_action))
                count_action[trans_action] += 1;
            else
                count_action.Add(trans_action, 1);

        }

        void add_rankscore(string state_no,Double rankscore)
        {
            if (state_rankscore.ContainsKey(state_no))
            {

                Double temp_rankscore = rankscore + state_rankscore[state_no].Item1;
                int temp_count = 1 + state_rankscore[state_no].Item2;
                var temp_tuple = new Tuple<Double, int>(temp_rankscore, temp_count);
                state_rankscore[state_no] = temp_tuple;
            }
            else
            {
                var temp_key = new Tuple<Double, int>(rankscore, 1);

                state_rankscore.Add(state_no, temp_key);
            }

        }

        void calculate_transition_probability()
        {
            foreach (var entry in count_transitions_action)
            {
                var item1 = entry.Key.Item1;
                var item2 = entry.Key.Item2;
                var item3 = entry.Key.Item3;
                var numerator = entry.Value;

                var denominator = count_action[new Tuple<string, int>(item1, item2)];

                var prob = (float)(numerator) / denominator;
                //Console.WriteLine(prob);
                trans_prob.Add(new Tuple<string, int, string>(item1, item2, item3), prob);
            }
        }

        public void prepare_model()
        {
            StreamReader reader = new StreamReader(path + "ads_to_states.csv");

            StreamWriter writer = new StreamWriter(new FileStream(path + "tp.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
            StreamWriter writer_action = new StreamWriter(new FileStream(path + "tp_action.csv", FileMode.Create, FileAccess.Write, FileShare.Read));

           
            string line;
            line = reader.ReadLine();
            var row = line.Split(',');
            var RGUID_old = row[0];
            var state_no_old = row[6];
            var state_no_new = "";
            var RGUID_new = "";
            var rankscore = float.Parse(row[7]);

            var state_no_old_int = int.Parse(state_no_old);
            var state_no_new_int = -1;


            var actions = RGUID_action[RGUID_old];
            int k = 0;

            add_link(new Tuple<string, string>("1", state_no_old));
            add_link_action(new Tuple<string, int, string>("1", actions[0], state_no_old));
            add_action(new Tuple<string, int>("1", actions[0]));
            
            var newLine = string.Format("{0}, {1}, {2} ,{3} ,{4}", RGUID_old, "1", state_no_old, count_transitions[new Tuple<string, string>("1", state_no_old)], 1);
            writer.WriteLine(newLine);

            var newLine_action = string.Format("{0}, {1}, {2} ,{3} ,{4} ,{5}", RGUID_old, "1", actions[0], state_no_old, count_transitions_action[new Tuple<string, int, string>("1", actions[0], state_no_old)], 1);
            writer_action.WriteLine(newLine_action);

            while ((line = reader.ReadLine()) != null)
            {
                row = line.Split(',');

                RGUID_new = row[0];
                state_no_new = row[6];
                state_no_new_int = int.Parse(state_no_new);
                rankscore= float.Parse(row[7]); 

                Tuple<string, string> trans = new Tuple<string, string>(state_no_old, state_no_new);
                Tuple<string, string> trans_end = new Tuple<string, string>(state_no_old, "0");
                Tuple<string, string> trans_start = new Tuple<string, string>("1", state_no_new);


                k++;
                Tuple<string, int, string> trans_action = new Tuple<string, int, string>(state_no_old, actions[k], state_no_new);
                Tuple<string, int, string> trans_action_end = new Tuple<string, int, string>(state_no_old, actions[k], "0");
                Tuple<string, int, string> trans_action_start = new Tuple<string, int, string>("1", actions[k], state_no_new);

                Tuple<string, int> state_action = new Tuple<string, int>(state_no_old, actions[k]);
                Tuple<string, int> state_action_end = new Tuple<string, int>(state_no_old, actions[k]);
                Tuple<string, int> state_action_start = new Tuple<string, int>("1", actions[k]);

                add_rankscore(state_no_new, rankscore);

                if (RGUID_new == RGUID_old)
                {
                    add_link(trans);
                    add_link_action(trans_action);
                    add_action(state_action);

                    newLine = string.Format("{0}, {1}, {2} ,{3}", RGUID_new, state_no_old, state_no_new, count_transitions[trans]);
                    writer.WriteLine(newLine);

                    newLine_action = string.Format("{0}, {1}, {2} ,{3} ,{4}", RGUID_new, state_no_old, actions[k], state_no_new, count_transitions_action[trans_action]);
                    writer_action.WriteLine(newLine_action);


                }
                else
                {

                    add_link(trans_end);
                    add_link(trans_start);

                    add_link_action(trans_action_end);

                    add_action(state_action_end);

                    newLine_action = string.Format("{0}, {1}, {2} ,{3} ,{4},{5}", RGUID_old, state_no_old, actions[k], 0, count_transitions_action[trans_action_end], -1);
                    writer_action.WriteLine(newLine_action);

                    actions = RGUID_action[RGUID_new];
                    k = 0;

                    add_link_action(trans_action_start);

                    add_action(state_action_start);

                    newLine_action = string.Format("{0}, {1}, {2} ,{3} ,{4},{5}", RGUID_new, 1, actions[k], state_no_new, count_transitions_action[trans_action_start], 1);
                    writer_action.WriteLine(newLine_action);


                    //writing the last ad of previous RGUID :state_no_old->0
                    newLine = string.Format("{0}, {1}, {2},{3} ,{4}", RGUID_old, state_no_old, 0, count_transitions[trans_end], -1);
                    writer.WriteLine(newLine);
                    //writing the first ad of new RGUID :1->state_no_new
                    newLine = string.Format("{0}, {1}, {2},{3} ,{4}", RGUID_new, 1, state_no_new, count_transitions[trans_start], 1);
                    writer.WriteLine(newLine);




                }

                state_no_old_int = state_no_new_int;
                state_no_old = state_no_new;
                RGUID_old = RGUID_new;

            }

            add_link(new Tuple<string, string>(state_no_old, "0"));
            add_link_action(new Tuple<string, int, string>(state_no_old, actions[k], "0"));

            add_action(new Tuple<string, int>(state_no_old, actions[k]));
            
            //to write last line of csv file to another csv
            writer.WriteLine(string.Format("{0}, {1}, {2},{3} ,{4}", RGUID_old, state_no_old, 0, count_transitions[new Tuple<string, string>(state_no_old, "0")], -1));
            writer_action.WriteLine(string.Format("{0}, {1}, {2} ,{3} ,{4},{5}", RGUID_old, state_no_old, actions[k], 0, count_transitions_action[new Tuple<string, int, string>(state_no_old, actions[k], "0")], -1));
            reader.Close();
            writer.Close();
            writer_action.Close();


            calculate_transition_probability(); // this will use above formed dictionary to make transition_prob dictionary
        }
    }
}