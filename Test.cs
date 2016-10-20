using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MDP_model
{
    class Test
    {
        Dictionary<Tuple<string, int, string>, Double> trans_prob = new Dictionary<Tuple<string, int, string>, Double>();
        Dictionary<string, Tuple<Double, int>> mdp_model = new Dictionary<string, Tuple<Double, int>>();
        Dictionary<Tuple<string, string>, int> count_transition = new Dictionary<Tuple<string, string>, int>();

        AdjacencyList Ad_Graph= new AdjacencyList();

        StreamWriter writer = new StreamWriter(new FileStream(MyGlobals.path + "mdp_result.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
        public Test()
        {
            read_dictionary();
            MDP model = new MDP(count_transition);
            Ad_Graph = model.Ad_Graph;

            
            var nodes= Ad_Graph.adjacencyList.Keys.ToList(); ;

            for (int i = 0; i < nodes.Count; i++)
            {
                writer.WriteLine();
                var row = string.Format("{0},{1}", "start node", nodes[i]);
                writer.WriteLine(row);
                //Console.WriteLine("start node : "+ nodes[i]);
                state_flow(nodes[i]); //1 61155 71221 21254 92434
            }

            writer.Close();
            Console.ReadLine();
        }

        public void state_flow(int start_state)
        {

            if (start_state == 0)
            {
                writer.WriteLine(string.Format("{0}", "0"));
                return;
            }

            List<int> edgeList = Ad_Graph.get_linked_nodes(start_state);
            int best_action = mdp_model[start_state.ToString()].Item2;

            Double prob=0;
            Double max_prob=0;
            int state_max_prob = 0;

            //Console.WriteLine(start_state + " " + best_action);
            var row = string.Format("{0},{1}", start_state, best_action);
            writer.WriteLine(row);

            foreach (int connected_node_index in edgeList)                           //calculate utility for each node
            {

                
                var trans = new Tuple<string, int, string>(start_state.ToString(), best_action, connected_node_index.ToString()); //here so many kets wont be found , i can eithermake others transision as zero

                if (trans_prob.ContainsKey(trans))
                {
                    prob = trans_prob[trans];
                    
                    if(prob>max_prob)
                    {
                        max_prob = prob;
                        state_max_prob = connected_node_index;
                    }
                }
            }

            
            state_flow(state_max_prob);

        }
        public void read_dictionary()
        {
            //how to read the dict and make a dict object 
            
            using (StreamReader reader = new StreamReader(MyGlobals.path+ "trans_prob.txt"))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var items = line.Split(',');
                    trans_prob.Add(new Tuple<string, int, string>(items[0], int.Parse(items[1]), items[2]), Double.Parse(items[3]));
                }

                reader.Close();
            }

            
            using (StreamReader reader = new StreamReader(MyGlobals.path + "Value_iteration_convergered_result.txt"))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var items = line.Split(',');
                    mdp_model.Add(items[0],new Tuple<double, int>(Double.Parse(items[1]), int.Parse(items[2]) ));
                }

                reader.Close();
            }

            
            using (StreamReader reader = new StreamReader(MyGlobals.path + "count_transitions.txt"))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var items = line.Split(',');
                    count_transition.Add(new Tuple<string, string>(items[0], items[1]), int.Parse(items[2]));
                }

                reader.Close();
            }

        }
    }
}
