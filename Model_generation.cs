using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MDP_model
{
    internal class Model_generation
    {
        string path = MyGlobals.path;

        Dictionary<Tuple<string, string>, int> count_transitions; //s->s
        Dictionary<Tuple<string, int, string>, int> count_transitions_action ; //s->a->s
        Dictionary<Tuple<string, int>, int> count_action ; //s->a
        Dictionary<string, Tuple<Double, int>> state_rankscore;
        Dictionary<Tuple<string, int, string>, Double> trans_prob;

        public Model_generation(Transition_Model model)
        {
            
            count_transitions = model.count_transitions; //s->s
            count_transitions_action = model.count_transitions_action; //s->a->s
            count_action = model.count_action; //s->a
            state_rankscore = model.state_rankscore;
            trans_prob = model.trans_prob;


        }

        public void check_dictionary(Dictionary<Tuple<string, string>, int> count_transitions, Dictionary<Tuple<string, int, string>, int> count_transitions_action)
        {
            var node1 = "";
            var node2 = "";
            var Total_count = 0;
            var count = 0;
            foreach (var entry in count_transitions)
            {
                node1 = entry.Key.Item1;
                node2 = entry.Key.Item2;
                Total_count = entry.Value;

                count = 0;
                for (int i = 1; i <= 4; i++)
                {

                    Tuple<string, int, string> node = new Tuple<string, int, string>(node1, i, node2);
                    if (count_transitions_action.ContainsKey(node))
                    {
                        count += count_transitions_action[node];
                    }
                }

                if (Total_count != count)
                    Console.WriteLine("There is some logical error in dictionary generation");
            }
        }

        //fn to write all the dictionary in txt files
        public void save_model()
        {
            
            using (StreamWriter file = new StreamWriter(path+"count_transitions.txt"))
                foreach (var entry in count_transitions)
                    file.WriteLine("{0},{1},{2}", entry.Key.Item1, entry.Key.Item2, entry.Value);

            using (StreamWriter file = new StreamWriter(path+"count_transitions_action.txt"))
                foreach (var entry in count_transitions_action)
                    file.WriteLine("{0},{1},{2},{3}", entry.Key.Item1, entry.Key.Item2, entry.Key.Item3, entry.Value);

            using (StreamWriter file = new StreamWriter(path + "count_action.txt"))
                foreach (var entry in count_action)
                    file.WriteLine("{0},{1},{2}", entry.Key.Item1, entry.Key.Item2, entry.Value);

            using (StreamWriter file = new StreamWriter(path + "trans_prob.txt"))
                foreach (var entry in trans_prob)
                    file.WriteLine("{0},{1},{2},{3}", entry.Key.Item1, entry.Key.Item2, entry.Key.Item3, entry.Value);

            using (StreamWriter file = new StreamWriter(path + "state_rankscore.txt"))
                foreach (var entry in state_rankscore)
                    file.WriteLine("{0},{1},{2}", entry.Key, entry.Value.Item1, entry.Value.Item2);

            Console.Write("Total no of connections  : {0}  \n model generation done \n ", count_transitions.Count);
            check_dictionary(count_transitions, count_transitions_action);
            

            
        }



    }
}