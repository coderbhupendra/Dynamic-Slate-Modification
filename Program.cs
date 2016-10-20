using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MDP_model
{
    public static class MyGlobals
    {
        public const string path = @"C:\Users\t-bhsin\Downloads\data\";
        public const string csv_file = "NenaExact6.ss (2).csv";
        
    }
    class Program
    {
        public static void Main()
        {
            Test test = new Test();
        }
       /* public static void Main()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();


            State_bin bin = new State_bin();      
            var RGUID_actions = bin.read_csv();

            Transition_Model model = new Transition_Model(RGUID_actions);
            model.prepare_model();


            //Model Generation:if u want to save the model dictinaries in txt file
            //Model_generation mg = new Model_generation(model);
            //mg.save_model();

            Dictionary<Tuple<string, int, string>, Double> trans_prob = model.trans_prob; //s->a->s
            Dictionary<Tuple<string, string>, int> count_transition= model.count_transitions; //s->s
            Dictionary<string, Tuple<Double, int>> state_reward = model.state_rankscore;
           
            //Graph Generation and MDP learning
            MDP m = new MDP(count_transition,trans_prob,state_reward);
            Console.WriteLine("\nGraph Generated\n");

            m.Ad_Graph.printAdjacencyList();
            m.Bellmen_convergence();

            Console.WriteLine("\npress any key to exit");
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Time consumed: "+elapsedMs/1000 +"sec");
            Console.ReadLine(); //Pause  

        }*/
    }
}