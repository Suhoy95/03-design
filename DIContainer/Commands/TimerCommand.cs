using System;
using System.IO;
using System.Threading;

namespace DIContainer.Commands
{
    public class TimerCommand : BaseCommand
    {
        private readonly CommandLineArgs arguments;

        public TimerCommand(CommandLineArgs arguments, TextWriter newWriter)
        {
            this.ioWriter = newWriter;
            this.arguments = arguments;
        }

        public override void Execute()
        {
            var timeout = TimeSpan.FromMilliseconds(arguments.GetInt(0));
            ioWriter.WriteLine("Waiting for " + timeout);
            Thread.Sleep(timeout);
            ioWriter.WriteLine("Done!");
        }
    }
}