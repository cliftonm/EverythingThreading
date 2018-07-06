using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TWinForm
{
    public static class ExtensionMethods
    {
        public static void AppendLine(this TextBox tb, string text)
        {
            // tb.Invoke(() => tb.AppendText(text + "\r\n"));
            tb.AppendText(text + "\r\n");
        }

        public static void Invoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                // We want a synchronous call here!!!!
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public static void BeginInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                // We want an asynchronous call here!!!!
                control.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }

    public partial class Form1 : Form
    {
        int MAX = 500000;
        int nextNumber = 1;
        object locker = new object();

        public Form1()
        {
            InitializeComponent();
            Shown += OnFormShown;
        }

        private async void OnFormShown(object sender, EventArgs e)
        {
            List<Task<int>> tasks = TaskAwaitGetNextWorkItemBruteForceWithReturnAndContinuation();
            await Task.WhenAll(tasks);
            int numPrimes = tasks.Sum(t => t.Result);
            tbOutput.AppendLine("Number of primes is : " + numPrimes);
        }

        protected bool IsPrime(int n)
        {
            bool ret = true;
            for (int i = 2; i <= n / 2 && ret; ret = n % i++ != 0) ;
            return ret;
        }

        protected async void DurationOf(Func<int> action, string section)
        {
            var start = DateTime.Now;
            int numPrimes = await Task.Run(() => action());
            var stop = DateTime.Now;

            lock (locker)
            {
                //tbOutput.BeginInvoke(() =>
                //{
                tbOutput.AppendLine(section);
                tbOutput.AppendLine("Number of primes is : " + numPrimes);
                tbOutput.AppendLine("Total seconds = " + (stop - start).TotalSeconds);
                //});
            }
        }

        protected int BruteForceAlgorithm(object parms)
        {
            int threadNum = (int)parms;
            int numPrimes = 0;

            int n;

            while ((n = Interlocked.Increment(ref nextNumber)) < MAX)
            {
                if (IsPrime(n))
                {
                    ++numPrimes;
                }
            }

            return numPrimes;
        }

        protected List<Task<int>> TaskAwaitGetNextWorkItemBruteForceWithReturnAndContinuation()
        {
            int numProcs = Environment.ProcessorCount;
            nextNumber = 1;
            List<Task<int>> tasks = new List<Task<int>>();
            DateTime start = DateTime.Now;

            for (int i = 0; i < numProcs; i++)
            {
                tbOutput.AppendLine("Starting thread " + i + " at " + (DateTime.Now - start).TotalMilliseconds + " ms");
                var task = DoWorkWithReturnAsyncAndContinuation(i);
                tasks.Add(task);
            }

            return tasks;
        }

        protected async Task<int> DoWorkWithReturnAsyncAndContinuation(int threadNum)
        {
            DateTime start = DateTime.Now;
            var t = await Task.Run(() => BruteForceAlgorithm(threadNum)); //.ConfigureAwait(true);

            DateTime stop = DateTime.Now;

            lock (locker)
            {
                tbOutput.AppendLine("Continuation: Thread number " + threadNum + " finished.");
                tbOutput.AppendLine("Total seconds = " + (stop - start).TotalSeconds);
                tbOutput.AppendLine("Continuation: Count = " + t);
            }

            return t;
        }
    }
}
