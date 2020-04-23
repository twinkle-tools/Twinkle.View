using System;
using SimpleInjector;

namespace Twinkle.View.Infrastructure
{
    internal class DI
    {
        internal Container Container { get; set; }
        internal void Configure()
        {
            Container = new Container();
            Container.Register<Context>(Lifestyle.Singleton);
        }

        internal void RunTimeRegister(Type type, Lifestyle lifestyle)
        {
            Container.Register(type, type, lifestyle);
        }

        internal void RunTimeRegister<T>(Func<T> func) where T:class
        {
            Container.Register<T>(func, Lifestyle.Singleton);
        }
    }
}