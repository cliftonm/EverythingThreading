using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Primes
{
    class Program
    {
        // Should find 41,538 primes for MAX 500,000
        static int MAX = 500000;
        static int SQRT_MAX = (int)Math.Sqrt(MAX);
        static int totalNumPrimes = 0;
        static int nextNumber = 1;
        static bool[] notPrimes;
        static object locker = new object();

        static void Main(string[] args)
        {
            // DurationOf(BruteForce, "Brute force:");
            Console.WriteLine();

            DurationOf(ThreadedBruteForce, "Threaded brute force:");
            Console.WriteLine();

            //DurationOf(ThreadedGetNextWorkItemBruteForce, "Threaded get next work item brute force:");
            Console.WriteLine();

            // DurationOf(AsParallelGetNextWorkItemBruteForce, "As parallel get next work item brute force:");
            Console.WriteLine();

            // DurationOf(ThreadedGetNextWorkItemSieve, "Threaded get next work item sieve:");
            Console.WriteLine();

            // DurationOf(ThreadedGetNextWorkItemSieveShowPrimes, "Threaded get next work item sieve:");
            // notPrimes.Select((p, idx) => new { p, idx }).Where((item) => !item.p).ToList().ForEach(item => Console.WriteLine(item.idx));
            Console.WriteLine();

            Console.ReadLine();
        }

        static void DurationOf(Func<int> action, string section)
        {
            var start = DateTime.Now;
            int numPrimes = action();
            var stop = DateTime.Now;

            lock (locker)
            {
                Console.WriteLine(section);
                Console.WriteLine("Number of primes is : " + numPrimes);
                Console.WriteLine("Total seconds = " + (stop - start).TotalSeconds);
            }
        }

        static int BruteForce()
        {
            int numPrimes = NumPrimes(2, MAX);

            return numPrimes;
        }

        static int NumPrimes(int start, int end)
        {
            int numPrimes = 0;
            for (int i = start; i < end; numPrimes += IsPrime(i++) ? 1 : 0);
            return numPrimes;
        }

        static int ThreadedBruteForce()
        {
            List<(Thread thread, int threadNum, int start, int end)> threads = new List<(Thread thread, int threadNum, int start, int end)>();

            int numProcs = Environment.ProcessorCount;

            for (int i = 0; i < numProcs; i++)
            {
                int j = i;
                int start = Math.Max(1, i * (MAX / numProcs)) + 1;
                int end = (i + 1) * (MAX / numProcs);
                // var thread = new Thread(new ParameterizedThreadStart(BruteForceThread));
                // var thread = new Thread(new ThreadStart(() => BruteForceThread(i, start, end)));
                var thread = new Thread(new ThreadStart(() => BruteForceThread(j, start, end)));
                thread.IsBackground = true;
                threads.Add((thread, i, start, end));
            }

            totalNumPrimes = 0;
            // threads.ForEach(t => t.thread.Start((t.threadNum, t.start, t.end)));
            threads.ForEach(t => t.thread.Start());
            threads.ForEach(t => t.thread.Join());

            return totalNumPrimes;
        }

        static int ThreadedGetNextWorkItemBruteForce()
        {
            List<(Thread thread, int threadNum)> threads = new List<(Thread thread, int threadNum)>();
            int numProcs = Environment.ProcessorCount;

            for (int i = 0; i < numProcs; i++)
            {
                var thread = new Thread(new ParameterizedThreadStart(NextWorkItemBruteForceThread));
                thread.IsBackground = true;
                threads.Add((thread, i));
            }

            totalNumPrimes = 0;
            nextNumber = 1;
            threads.ForEach(t => t.thread.Start(t.threadNum));
            threads.ForEach(t => t.thread.Join());

            return totalNumPrimes;
        }

        static int AsParallelGetNextWorkItemBruteForce()
        {
            int totalNumPrimes = 0;

            var nums = Enumerable.Range(2, MAX);
            nums.AsParallel().ForAll(n =>
            {
                if (IsPrime(n))
                {
                    Interlocked.Increment(ref totalNumPrimes);
                }
            });

            return totalNumPrimes;
        }

        static int ThreadedGetNextWorkItemSieve()
        {
            List<(Thread thread, int threadNum)> threads = new List<(Thread thread, int threadNum)>();
            int numProcs = Environment.ProcessorCount;

            for (int i = 0; i < numProcs; i++)
            {
                var thread = new Thread(new ParameterizedThreadStart(NextWorkItemSieveThread));
                thread.IsBackground = true;
                threads.Add((thread, i));
            }

            totalNumPrimes = 0;
            nextNumber = 1;
            notPrimes = new bool[MAX];
            notPrimes[0] = true;
            notPrimes[1] = true;
            threads.ForEach(t => t.thread.Start(t.threadNum));
            threads.ForEach(t => t.thread.Join());

            totalNumPrimes = notPrimes.Count(p => !p);

            return totalNumPrimes;
        }

        static int ThreadedGetNextWorkItemSieveShowPrimes()
        {
            List<(Thread thread, int threadNum)> threads = new List<(Thread thread, int threadNum)>();
            int numProcs = Environment.ProcessorCount;

            for (int i = 0; i < numProcs; i++)
            {
                var thread = new Thread(new ParameterizedThreadStart(NextWorkItemSieveThread));
                thread.IsBackground = true;
                threads.Add((thread, i));
            }

            totalNumPrimes = 0;
            nextNumber = 1;
            notPrimes = new bool[MAX];
            notPrimes[0] = true;
            notPrimes[1] = true;
            threads.ForEach(t => t.thread.Start(t.threadNum));
            threads.ForEach(t => t.thread.Join());

            totalNumPrimes = notPrimes.Count(p => !p);

            return totalNumPrimes;
        }


        static void BruteForceThread(object parms)
        {
            (int threadNum, int start, int end) parm = (ValueTuple<int, int, int>)parms;
            DurationOf(() =>
            {
                int numPrimes = NumPrimes(parm.start, parm.end);
                Interlocked.Add(ref totalNumPrimes, numPrimes);
                return numPrimes;
            }, $"Thread {parm.threadNum} processing {parm.start} to {parm.end}");
        }

        static void BruteForceThread(int threadNum, int start, int end)
        {
            DurationOf(() =>
            {
                int numPrimes = NumPrimes(start, end);
                Interlocked.Add(ref totalNumPrimes, numPrimes);
                return numPrimes;
            }, $"Thread {threadNum} processing {start} to {end}");
        }

        static void NextWorkItemBruteForceThread(object parms)
        {
            int threadNum = (int)parms;
            DurationOf(() =>
            {
                int numPrimes = 0;
                int n;
                while ((n = Interlocked.Increment(ref nextNumber)) < MAX)
                {
                    if (IsPrime(n))
                    {
                        ++numPrimes;
                    }
                }
                Interlocked.Add(ref totalNumPrimes, numPrimes);
                return numPrimes;
            }, $"Thread: {threadNum}");
        }

        static void NextWorkItemSieveThread(object parms)
        {
            int threadNum = (int)parms;
            DurationOf(() =>
            {
                int n;
                while ((n = Interlocked.Increment(ref nextNumber)) < SQRT_MAX)
                {
                    int mx = n * n;

                    if (!notPrimes[n])     // potential prime candidate
                    {
                        while (mx < MAX)
                        {
                            notPrimes[mx] = true;       // This is an atomic operation.
                            mx += n;
                        }
                    }
                }
                return 0;       // we don't know the primes until all threads run.
            }, $"Thread: {threadNum}");
        }

        static bool IsPrime(int n)
        {
            bool ret = true;
            for (int i = 2; i <= n / 2 && ret; ret = n % i++ != 0);
            return ret;
        }
    }
}
