using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FluentTask
{
    internal class Program
    {
        private static void Main()
        {
            var behaviour = new Behavior()
                .Say("Привет мир!")
                .UntilKeyPressed(b => b
                    .Say("Ля-ля-ля!")
                    .Say("Тру-лю-лю"))
                .Jump(JumpHeight.High)
                .UntilKeyPressed(b => b
                    .Say("Aa-a-a-a-aaaaaa!!!")
                    .Say("[набирает воздух в легкие]"))
                .Say("Ой!")
                .Delay(TimeSpan.FromSeconds(1))
                .Say("Кто здесь?!")
                .Delay(TimeSpan.FromMilliseconds(2000));

            behaviour.Execute();
        }
    }

    class Behavior
    {

        private List<Action> actions = new List<Action>();

        public Behavior Say(string text)
        {
            actions.Add(() =>
            {
                foreach (char t in text)
                {
                    Console.Write(t);
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }
                Console.WriteLine();
            });
            return this;
        }

        public Behavior UntilKeyPressed(Func<Behavior, Behavior> b)
        {
            actions.Add(()=>
            {
                var subB = b(new Behavior());
                while(!Console.KeyAvailable)
                    subB.Execute();
                Console.ReadKey();
            });
            return this;
        }

        public Behavior Jump(JumpHeight height)
        {
            string jump = "Hero Jump " + (height == JumpHeight.High ? "Hight" : "Low");
            actions.Add(() => Console.WriteLine(jump));
            return this;
        }

        public Behavior Delay(TimeSpan time)
        {
            actions.Add(() => Thread.Sleep(time));
            return this;
        }

        public Behavior Execute()
        {
            actions.ForEach(a => a());
            return this;
        }
    }
}