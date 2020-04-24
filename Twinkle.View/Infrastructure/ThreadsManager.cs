using System.Collections.Concurrent;
using System.Threading;

namespace Twinkle.View.Infrastructure
{
    public class ThreadsManager
    {
        private static ConcurrentDictionary<int,TwinkleView> Correspondence { get; set; }
        internal static TwinkleView GetTargetFramework()
        {
            if(Correspondence == null)
                Correspondence = new ConcurrentDictionary<int, TwinkleView>();

            var threadId = Thread.CurrentThread.ManagedThreadId;

            if (Correspondence.ContainsKey(threadId))
            {
                return Correspondence.GetOrAdd(threadId, (key) => { return null; });
            }

            return Correspondence.AddOrUpdate(threadId, new TwinkleView(),(key, oldValue)=> { return null; });
        }
    }
}