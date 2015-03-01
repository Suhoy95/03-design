using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;

namespace battleships
{
	public class AiTester
	{
        private static IKernel container = new StandardKernel();
		private static readonly Logger resultsLog = LogManager.GetLogger("results");
		private readonly Settings settings;
	    private readonly IMapGenerator gen;
	    private readonly IGameVisualizer vis;
	    private readonly IProcessMonitor monitor;
        private int badShots = 0;
        private int crashes = 0;
        private int gamesPlayed = 0;
        private List<int> shots = new List<int>();
        private Ai ai;

	    public AiTester(Settings settings)
	    {
            this.settings = settings;
            InitAiTester();

            gen = container.Get<IMapGenerator>();
            vis = container.Get<IGameVisualizer>();
            monitor = container.Get<ProcessMonitor>(); 
	    }

	    private void InitAiTester()
	    {
	        container.Bind<IMapGenerator>().To<MapGenerator>().WithConstructorArgument(settings)
                                                              .WithConstructorArgument(new Random(settings.RandomSeed));
	        container.Bind<IGameVisualizer>().To<GameVisualizer>();
            container.Bind<IProcessMonitor>().To<ProcessMonitor>()
                 .WithConstructorArgument(TimeSpan.FromSeconds(settings.TimeLimitSeconds * settings.GamesCount))
                 .WithConstructorArgument(settings.MemoryLimit);
            
	        container.Bind<Map>().ToMethod(context => gen.GenerateMap());
	    }

		public void TestSingleFile(string exe)
		{
            ai = container.Get<Ai>();

            container.Bind<Ai>().ToSelf().WithConstructorArgument(exe)
                                         .WithConstructorArgument(monitor);
		    container.Bind<Game>().ToSelf().WithConstructorArgument(ai);

            for (var gameIndex = 0; gameIndex < settings.GamesCount; gameIndex++)
			{
			    var game = container.Get<Game>();
				RunGameToEnd(game);
				gamesPlayed++;
				badShots += game.BadShots;
				if (game.AiCrashed)
				{
					crashes++;
					if (crashes > settings.CrashLimit) break;
					ai = container.Get<Ai>();
				}
				else
					shots.Add(game.TurnsCount);
			
                if (settings.Verbose)
				{
					Console.WriteLine(
						"Game #{3,4}: Turns {0,4}, BadShots {1}{2}",
						game.TurnsCount, game.BadShots, game.AiCrashed ? ", Crashed" : "", gameIndex);
				}
			}

			ai.Dispose();
			WriteTotal(ai, shots, crashes, badShots, gamesPlayed);
		}

		private void RunGameToEnd(Game game)
		{
			while (!game.IsOver())
			{
				game.MakeStep();
				if (settings.Interactive)
				{
					vis.Visualize(game);
					if (game.AiCrashed)
						Console.WriteLine(game.LastError.Message);
					Console.ReadKey();
				}
			}
		}

		private void WriteTotal(Ai ai, List<int> shots, int crashes, int badShots, int gamesPlayed)
		{
			if (shots.Count == 0) shots.Add(1000 * 1000);
			shots.Sort();
			var median = shots.Count % 2 == 1 ? shots[shots.Count / 2] : (shots[shots.Count / 2] + shots[(shots.Count + 1) / 2]) / 2;
			var mean = shots.Average();
			var sigma = Math.Sqrt(shots.Average(s => (s - mean) * (s - mean)));
			var badFraction = (100.0 * badShots) / shots.Sum();
			var crashPenalty = 100.0 * crashes / settings.CrashLimit;
			var efficiencyScore = 100.0 * (settings.Width * settings.Height - mean) / (settings.Width * settings.Height);
			var score = efficiencyScore - crashPenalty - badFraction;
			var headers = FormatTableRow(new object[] { "AiName", "Mean", "Sigma", "Median", "Crashes", "Bad%", "Games", "Score" });
			var message = FormatTableRow(new object[] { ai.Name, mean, sigma, median, crashes, badFraction, gamesPlayed, score });
			resultsLog.Info(message);
			Console.WriteLine();
			Console.WriteLine("Score statistics");
			Console.WriteLine("================");
			Console.WriteLine(headers);
			Console.WriteLine(message);
		}

		private string FormatTableRow(object[] values)
		{
			return FormatValue(values[0], 15) 
				+ string.Join(" ", values.Skip(1).Select(v => FormatValue(v, 7)));
		}

		private static string FormatValue(object v, int width)
		{
			return v.ToString().Replace("\t", " ").PadRight(width).Substring(0, width);
		}
	}
}