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

        public void Continue(Action next)
        {
        }
    }

    public class CoopTask1
    {
        public void DoWork(CooperativeManager cm)
        {
            Thread.Sleep(1000);
            cm.Continue(() => Thread.Sleep(1000));
            cm.Continue(() => Thread.Sleep(1000));
            cm.Continue(() => Thread.Sleep(1000));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

        }
    }
}
