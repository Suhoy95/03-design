using System;
using System.Diagnostics;
using System.Linq;

namespace PerfLogger
{
	class Program
	{
		static void Main(string[] args)
		{
			var sum = 0.0;
			using (new PerfLogger.Measure((TimeSpan t) => Console.WriteLine("100M for iterations, time"+t.ToString())))
				for (var i = 0; i < 100000000; i++) sum += i;
			using (new PerfLogger.Measure((TimeSpan t) => Console.WriteLine("100M LINQ iterations, time"+t.ToString())))
				sum -= Enumerable.Range(0, 100000000).Sum(i => (double)i);
			Console.WriteLine(sum);
       
            Console.ReadKey();
        }
	}

   class Measure : IDisposable
   {
       private Stopwatch Timer;
       readonly Action<TimeSpan> EndAction;
       public Measure(Action<TimeSpan> endAction)
       {
           EndAction = endAction;
           Timer = Stopwatch.StartNew();
       }

       public void Dispose()
       {
           Timer.Stop();
           EndAction(Timer.Elapsed);
       }
    }
}
