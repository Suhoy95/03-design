using System;
using Ninject;

namespace battleships
{
    class ProgramModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<Settings>().ToSelf().WithConstructorArgument("settings.txt");
        }
    }
}
