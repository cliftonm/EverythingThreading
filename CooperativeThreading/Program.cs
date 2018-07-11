using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

// A demo of cooperative threading in C#, aka fibers.

namespace CooperativeThreading
{
    public class CooperativeManager
    {
        protected Queue<Action> fibers = new Queue<Action>();

        public void Add(Action work)
        {
            fibers.Enqueue(work);
        }

        public void Continue(Action next)
        {
            fibers.Enqueue(next);
            var nextFiber = fibers.Dequeue();
            nextFiber();
        }

        public void Run()
        {
            while (fibers.Count > 0)
            {
                var fiber = fibers.Dequeue();
                fiber();
            }
        }
    }

    public abstract class CMBase
    {
        public CooperativeManager CooperativeManager { get; protected set; }

        public CMBase(CooperativeManager cm)
        {
            CooperativeManager = cm;
        }
    }

    public class CoopTasks : CMBase
    {
        public CoopTasks(CooperativeManager cm) : base(cm) { }

        public void DoWork1()
        {
            Console.WriteLine("1 - 0");
            CooperativeManager.Continue(() => Console.WriteLine("1 - 1"));
            CooperativeManager.Continue(() => Console.WriteLine("1 - 2"));
            CooperativeManager.Continue(() => Console.WriteLine("1 - 3"));
            CooperativeManager.Continue(() => Console.WriteLine("1 - 4"));
        }

        public void DoWork2()
        {
            Console.WriteLine("2 - 0");
            CooperativeManager.Continue(() => Console.WriteLine("2 - 1"));
            CooperativeManager.Continue(() => Console.WriteLine("2 - 2"));
            CooperativeManager.Continue(() => Console.WriteLine("2 - 3"));
            CooperativeManager.Continue(() => Console.WriteLine("2 - 4"));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CooperativeManager cm = new CooperativeManager();
            CoopTasks tasks = new CoopTasks(cm);
            cm.Add(tasks.DoWork1);
            cm.Add(tasks.DoWork2);
            cm.Run();
        }
    }
}
