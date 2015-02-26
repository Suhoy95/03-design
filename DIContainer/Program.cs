using System.Collections.Generic;
using System.IO;
using DIContainer.Commands;
using FakeItEasy.ExtensionSyntax;
using Ninject;
using System;
using System.Linq;
using NUnit.Framework;

namespace DIContainer
{
    public class Program
    {
        private readonly CommandLineArgs arguments;
        private readonly ICommand[] commands;
        private readonly TextWriter ioWriter;

        public Program(CommandLineArgs arguments, TextWriter newWriter, params ICommand[] commands)
        {
            this.arguments = arguments;
            this.commands = commands;
            this.ioWriter = newWriter;
        }

        static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            var arguments = new Lazy<CommandLineArgs>(() => kernel.Get<CommandLineArgs>());

            kernel.Bind<TextWriter>().To<Writer>();
            kernel.Bind<CommandLineArgs>().ToSelf().WithConstructorArgument(args);
            kernel.Bind<ICommand>().To<PrintTimeCommand>();
            kernel.Bind<ICommand>().To<TimerCommand>();
            kernel.Bind<ICommand>().To<HelpCommand>();
            kernel.Bind<HelpCommand>().ToSelf().WithConstructorArgument(
                new Lazy<ICommand[]>(() => kernel.GetAll<ICommand>().ToArray()));
            kernel.Bind<TimerCommand>().ToSelf().WithConstructorArgument(arguments.Value);
            var commands = kernel.GetAll<ICommand>();
            var writer = kernel.Get<TextWriter>();
            new Program(arguments.Value,  writer, commands.ToArray()).Run();
        }

        public void Run()
        {
            if (arguments.Command == null)
            {
                ioWriter.WriteLine("Please specify <command> as the first command line argument");
                return;
            }
            var command = commands.FirstOrDefault(c => c.Name.Equals(arguments.Command, StringComparison.InvariantCultureIgnoreCase));
            if (command == null)
                ioWriter.WriteLine("Sorry. Unknown command {0}", arguments.Command);
            else
                command.Execute();
        }
    }

    class HelpCommand : BaseCommand
    {
        private readonly Lazy<ICommand[]> commands;

        public HelpCommand(Lazy<ICommand[]> newCommands, TextWriter newWriter)
        {
            ioWriter = newWriter;
            commands = newCommands;
        }

        public override void Execute()
        {
            ioWriter.WriteLine("Available commands:");
            foreach (var command in commands.Value)
            {
                ioWriter.WriteLine(command.Name);
            }
        }
    }

    class Writer : TextWriter
    {
        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        public override void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }

}
