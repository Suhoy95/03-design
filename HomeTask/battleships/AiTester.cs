using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Ninject;
using NLog;

namespace battleships
{
	public class AiTester
	{
		private readonly Settings settings;
	    private readonly IMapGenerator gen;
	    private readonly IGameVisualizer vis;
	    private readonly IProcessMonitor monitor;
	    private DataOfStatistics data;
	    private Func<string, Ai> aiFactory;
	    private Func<Map, Ai, Game> gameFactory; 

        private static readonly Logger resultsLog = LogManager.GetLogger("results");

	    public AiTester(Settings settings, IMapGenerator generator, 
                        IGameVisualizer visualizer, IProcessMonitor monitor,
                        DataOfStatistics data,
                        Func<string, Ai> aiFactory, Func<Map, Ai, Game> gameFactory)
	    {
            this.settings = settings;
            gen = generator;
            vis = visualizer;
            this.monitor = monitor;
	        this.data = data;
	        this.aiFactory = aiFactory;
	        this.gameFactory = gameFactory;
	    }

		public void TestSingleFile(string exe)
		{
            var ai = aiFactory(exe);
		    data.name = ai.Name;
		    ai.InitNewProcess += monitor.CatchProcess;

            for (var gameIndex = 0; gameIndex < settings.GamesCount; gameIndex++)
			{
			    var game = gameFactory(gen.GenerateMap(), ai);
				RunGameToEnd(game);
				data.gamesPlayed++;
				data.badShots += game.BadShots;
				if (game.AiCrashed)
				{
					data.crashes++;
					if (data.crashes > settings.CrashLimit) break;
                    ai = aiFactory(exe);
                    ai.InitNewProcess += monitor.CatchProcess;
				}
				else
					data.shots.Add(game.TurnsCount);
			
                if (settings.Verbose)
				{
					Console.WriteLine(
						"Game #{3,4}: Turns {0,4}, BadShots {1}{2}",
						game.TurnsCount, game.BadShots, game.AiCrashed ? ", Crashed" : "", gameIndex);
				}
			}

			ai.Dispose();
			WriteTotal(SolveStatistics(data, settings));
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

	    private static Statistics SolveStatistics(DataOfStatistics d, Settings settings)
	    {
	        if (d.shots.Count == 0) d.shots.Add(1000*1000);
	        d.shots.Sort();

	        var result = new Statistics();
	        result.name = d.name;
	        result.crashes = d.crashes;
	        result.gamesPlayed = d.gamesPlayed;
	        result.median = d.shots.Count%2 == 1 ? d.shots[d.shots.Count/2]
	                                             : (d.shots[d.shots.Count/2] + d.shots[(d.shots.Count + 1)/2])/2;
	        result.mean = d.shots.Average();
	        result.sigma = Math.Sqrt(d.shots.Average(s => (s - result.mean)*(s - result.mean)));
	        result.badFraction = (100.0*d.badShots)/d.shots.Sum();
	        result.crashPenalty = 100.0*d.crashes/settings.CrashLimit;
	        result.efficiencyScore = 100.0*(settings.Width*settings.Height - result.mean)/(settings.Width*settings.Height);
	        result.score = result.efficiencyScore - result.crashPenalty - result.badFraction;

	        return result;
	    }
        
        private static void WriteTotal(Statistics s)
		{
			var headers = FormatTableRow(new object[] { "AiName", "Mean", "Sigma", "Median", "Crashes", "Bad%", "Games", "Score" });
			var message = FormatTableRow(new object[] { s.name, s.mean, s.sigma, s.median, s.crashes, s.badFraction, s.gamesPlayed, s.score });
			resultsLog.Info(message);
			Console.WriteLine();
			Console.WriteLine("Score statistics");
			Console.WriteLine("================");
			Console.WriteLine(headers);
			Console.WriteLine(message);
		}

		private static string FormatTableRow(object[] values)
		{
			return FormatValue(values[0], 15) 
				+ string.Join(" ", values.Skip(1).Select(v => FormatValue(v, 7)));
		}

		private static string FormatValue(object v, int width)
		{
			return v.ToString().Replace("\t", " ").PadRight(width).Substring(0, width);
		}
	}

    public class DataOfStatistics
    {
        public string name = "BaseDataStatistic name";
        public long badShots = 0;
        public long crashes = 0;
        public long gamesPlayed = 0;
        public List<int> shots = new List<int>();
    }

    public class Statistics
    {
        public string name;
        public long crashes;
        public long gamesPlayed;
        public int median;
        public double mean;
        public double sigma;
        public double badFraction;
        public double crashPenalty;
        public double efficiencyScore;
        public double score;
    }
}