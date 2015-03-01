using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Ninject;
using Ninject.Parameters;

namespace battleships
{
	public class Program
	{
		private static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: {0} <ai.exe>", Process.GetCurrentProcess().ProcessName);
				return;
			}

		    var container = new StandardKernel(new ProgramModule());
			var aiPath = args[0];

			if (File.Exists(aiPath))
				container.Get<AiTester>().TestSingleFile(aiPath);
			else
				Console.WriteLine("No AI exe-file " + aiPath);
		}
	}
}