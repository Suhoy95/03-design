using System;
using Ninject;
using Ninject.Parameters;

namespace battleships
{
    class ProgramModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            var settings = new Settings("settings.txt");
             
            Bind<IMapGenerator>().To<MapGenerator>().WithConstructorArgument(settings)
                                                    .WithConstructorArgument(new Random(settings.RandomSeed));
	        Bind<IGameVisualizer>().To<GameVisualizer>();
            Bind<IProcessMonitor>().To<ProcessMonitor>()
                 .WithConstructorArgument(TimeSpan.FromSeconds(settings.TimeLimitSeconds * settings.GamesCount))
                 .WithConstructorArgument((long)settings.MemoryLimit);

            Bind<AiTester>().ToSelf().WithConstructorArgument(settings);
        }
    }
}
