using System;
using System.IO;

namespace DIContainer.Commands
{
    public class PrintTimeCommand : BaseCommand
    {
        public PrintTimeCommand(TextWriter newWriter)
        {
            ioWriter = newWriter;
        }
        public override void Execute()
        {
            ioWriter.WriteLine(DateTime.Now);
        }
    }
}