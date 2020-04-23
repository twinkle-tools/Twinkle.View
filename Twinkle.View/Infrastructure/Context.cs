using Microsoft.Extensions.Configuration;

namespace Twinkle.View.Infrastructure
{
    public class Context
    {
        internal DI DI { get; set; }
        internal IConfiguration Config { get; set; }
        internal GlobalConfiguraionFramework GlobalConfiguraionFramework { get; set; }
        public Views Views { get; internal set; }

    }
}