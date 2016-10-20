using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MDP_model
{   
    struct State
    {

        public int state_no;

        public Double reward;

        public int best_action;  //this will be used for policy pi->a

        public Double state_value; //this will be used for storing the value of a state

        public State(int state_no ,Double reward) : this()
        {
            this.state_no = state_no;
            this.reward = reward;
        }

        
        public void set_best_action(int best)
        {
            best_action = best;
        }

        public void set_state_value(double value_)
        {
            state_value = value_;
        }

        public double get_state_value()
        {
            return state_value;
        }

        public int get_best_action()
        {
            return best_action;
        }

    };//end of struct feature 


    
    class AdjacencyList
    {
        public Dictionary<int,List<int>> adjacencyList;


        // Constructor - creates an empty Adjacency List
        public AdjacencyList()
        {
            adjacencyList = new Dictionary<int, List<int>>();
      
        }
        

        // Appends a new Edge to the linked list
        public void addEdge(int start_node, int end_node)
        {
            if(adjacencyList.ContainsKey(start_node))
                    adjacencyList[start_node].Add(end_node);
            else
            {
                adjacencyList.Add(start_node, new List<int>() { end_node });
            }

        }

      
        
        // Returns a copy of the Linked List of outward edges from a vertex
        public List<int> get_linked_nodes(int index)
        {
            List<int> edgeList = adjacencyList[index];
            return edgeList;
        }

        // Prints the Adjacency List
        public void printAdjacencyList()
        {
            StreamWriter file = new StreamWriter(MyGlobals.path+"graph.txt");
            
            foreach (var entry in adjacencyList)
            {
                string line = "";
                List<int> list = entry.Value;
                int main_node = entry.Key;
                
                
                line+="adjacencyList[" + main_node + "] -> "+list.Count+": ";

                foreach (int edge in list)
                {
                    line+=edge+" ";
                }

                file.WriteLine(line);

            }
            file.Close();
        }


    }

    class MDP
    {

        public AdjacencyList Ad_Graph;
        public List<State> Global_nodes;
        public List<State> Global_nodes_temp;
        //MDP Framework 
        int no_states;
        Double gamma = 0.9;
        int no_action = 4;
        private Dictionary<Tuple<string, string>, int> count_transition;
        private Dictionary<Tuple<string, int, string>, Double> trans_prob;
        private Dictionary<string, Tuple<Double, int>> state_reward;

        private Dictionary<int, int> state_map; // this id to map various state_no to index of list.

        //constructor      

        public MDP(Dictionary<Tuple<string, string>, int> count_transition, Dictionary<Tuple<string, int, string>, Double> trans_prob, Dictionary<string, Tuple<Double, int>> state_reward)
        {
            this.count_transition = count_transition;
            this.trans_prob = trans_prob;
            this.state_reward = state_reward;

            Global_nodes = new List<State>();
            Ad_Graph = new AdjacencyList();

            state_map = new Dictionary<int, int>();

            initialize_states();
           
            Global_nodes_temp = new List<State>(Global_nodes);
           
            initialize_graph();

            no_states = Global_nodes.Count;
            Console.WriteLine("Total no of nodes in graph : {0}",no_states);
        }
        //constructor for testing
        public MDP(Dictionary<Tuple<string, string>, int> count_transition)
        {
            this.count_transition = count_transition;
           
            Ad_Graph = new AdjacencyList();

            initialize_graph();
            
        }

        public void initialize_states()
        {
            foreach (var entry in state_reward)//s->s
            {
                var state_no = int.Parse(entry.Key);
                var reward = entry.Value.Item1 / entry.Value.Item2; 
                State s = new State(state_no, reward);

               
                if (!Global_nodes.Contains(s))
                {
                    Global_nodes.Add(s);
                    state_map.Add(state_no, Global_nodes.Count-1);
                }
                
            }

            Global_nodes.Add(new State(1, 0)); // adding start state
            state_map.Add(1, state_map.Count);  //adding mapping for start state

            Global_nodes.Add(new State(0, 0)); // adding null state 
            state_map.Add(0, state_map.Count); //adding mapping for null state

        }
        public void initialize_graph()
        {

            foreach (var entry in count_transition)//s->s
            {
                int node1 = int.Parse(entry.Key.Item1);
                int node2 = int.Parse(entry.Key.Item2);

                Ad_Graph.addEdge(node1, node2);
                
            }

            Ad_Graph.adjacencyList.Add(0, new List<int>()); // adding null list for 0th state(key) because 0th state is not connected to any other state.
        }

        public int policy_per_state(State main_node)
        {
            int main_node_index = main_node.state_no;
            //get all nodes which are connected to main index_state node
            List<int> edgeList = Ad_Graph.get_linked_nodes(main_node_index);

            double[] pi = new double[no_action];

            foreach (int connected_node_index in edgeList)
            {
                //State connected_node= Global_nodes.Find(x => x.state_no == connected_node_index); //use a map instead fo linear search for find
                State connected_node = Global_nodes[state_map[connected_node_index]];
                double connected_node_value = connected_node.state_value;

                for (int j = 0; j < no_action; j++)
                {
                    var trans = new Tuple<string, int, string>(main_node_index.ToString(), j+1, connected_node_index.ToString()); //here so many keys wont be found , i can eithermake others transision as zero
                   
                    if (trans_prob.ContainsKey(trans))
                        pi[j] += trans_prob[trans] * connected_node_value;
                   

                }

            }


            double max = pi[0];
            int pos = 0;
            for (int i = 0; i < pi.Length; i++)
            {
                if (pi[i] > max) { max = pi[i]; pos = i; }
            }
            //Console.WriteLine("pi: {0} {1} {2}", pi[0],pi[1],pi[2]);
            
            return pos+1;
            
        }

        public void policy()
        {
            //calculating best action for all states.
            for (int i = 0; i < no_states; i++)
            {
                State node = Global_nodes[i];
                
                int best_action = policy_per_state(node);
               
                node.set_best_action(best_action);
                Global_nodes[i] = node;
            }

        }


        public void value_iteration()
        {
            double value_ =0;
            for (int i = 0; i < no_states; i++)
            {
                
                State main_node = Global_nodes[i];                                         //get main_node 
                int main_node_index = main_node.state_no;
                
                Double main_node_value = main_node.state_value;
                
                double main_node_reward = main_node.reward;
                int best_action = policy_per_state(main_node);
                
                List<int> edgeList = Ad_Graph.get_linked_nodes(main_node_index);          //get the linked nodes to main_node  
                value_ = 0;

               
                foreach (int connected_node_index in edgeList)                           //calculate utility for each node
                {
                    
                    State connected_node= Global_nodes[state_map[connected_node_index]];
                    // State connected_node = Global_nodes.Find(x => x.state_no == connected_node_index); //use a map instead fo linear search for find
                    
                    double connected_node_value = connected_node.state_value;

                    var trans = new Tuple<string, int, string>(main_node_index.ToString(), best_action, connected_node_index.ToString()); //here so many kets wont be found , i can eithermake others transision as zero

                    if (trans_prob.ContainsKey(trans))
                    {
                        var p = trans_prob[trans];
                        var temp_val= p * connected_node_value;
                        value_ += temp_val;
                        
                    }
                } //inner for loop ends

                value_ = gamma * value_ + main_node_reward;

                main_node.set_state_value(value_);     // here we  put utility in state node  
                Global_nodes[i] = main_node;

               
            }// outer for loop ends

        }


        public void Bellmen_convergence()
        {
            Console.WriteLine("Value Iteration begings..");
            //Write_learned_values(0);
            value_iteration();

            StreamWriter writer = new StreamWriter(new FileStream(MyGlobals.path + "convergence_plot.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
            writer.WriteLine(string.Format("{0},{1},{2}","Iteration no","State_value_diff","policy_diff"));
            Tuple<Double,int> delta_diff ;
            for (int i = 1; i < 10000; i++)
            {

                policy();
                value_iteration();
                Console.WriteLine(i + " iteration ");
                /*for (int j = 0; j < 2; j++)
                {
                 value_iteration();
                 Console.WriteLine(i + " " + j+" ");
                }*/
                
                //Write_learned_values(i);

                delta_diff = delta_difference(Global_nodes, Global_nodes_temp);
                string row = string.Format("{0},{1},{2}",i,delta_diff.Item1,delta_diff.Item2);
                writer.WriteLine(row);
                writer.Flush();

                if (delta_diff.Item1 <= 0.001 && delta_diff.Item2<1)
                    break;

            }
            write_policy_value();
            writer.Close();

            Console.WriteLine("MDP Model Converged");
        }

        public void Write_learned_values(int epooch_no)
        {
            using (StreamWriter writer = File.AppendText(MyGlobals.path + "Value_iteration_result.txt"))

            {
                
                string line = "";
                for (int i = 0; i < no_states; i++)
                {
                    line = String.Format("\nepoch: {0} ,State_no: {1},State value: {2}, best action: {3} ", epooch_no, Global_nodes[i].state_no, Global_nodes[i].get_state_value(), Global_nodes[i].get_best_action());
                    writer.WriteLine(line);
                }
                
                writer.Close();

            }
        
        }

        public Tuple<Double,int> delta_difference(List<State> Global_nodes ,List<State>Global_nodes_temp)
        {
            Double diff = 0;
            int policy_count_diff = 0;
            for (int i =0;i<Global_nodes.Count;i++)
            {
                diff += Math.Abs(Global_nodes[i].state_value - Global_nodes_temp[i].state_value);

                if (Global_nodes[i].best_action != Global_nodes_temp[i].best_action)
                    policy_count_diff += 1;
            }


            //copy the globalnode list to globalnodetemp 
            for (int j = 0; j < no_states; j++)
            {
                Global_nodes_temp[j] = Global_nodes[j];
            }


            return new Tuple<double, int>(diff,policy_count_diff);
        }

        public void write_policy_value()
        {
            using (StreamWriter writer = File.AppendText(MyGlobals.path + "Value_iteration_convergered_result.txt"))

            {

                string line = "";
                for (int i = 0; i < no_states; i++)
                {
                    line = String.Format("{0},{1},{2}", Global_nodes[i].state_no, Global_nodes[i].get_state_value(), Global_nodes[i].get_best_action());
                    writer.WriteLine(line);
                }

                writer.Close();

            }

        }


    }

}