using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Twinkle.View.Infrastructure
{
    public class ThreadsManager
    {
        private static ConcurrentDictionary<int,InternalTwinkleView> Correspondence { get; set; }
        internal static InternalTwinkleView GetTargetFramework()
        {
            if(Correspondence == null)
                Correspondence = new ConcurrentDictionary<int, InternalTwinkleView>();

            var threadId = Thread.CurrentThread.ManagedThreadId;

            if (Correspondence.ContainsKey(threadId))
            {
                 var resultGet = Correspondence.TryGetValue(threadId, out var _twinkleView);
                 if (!resultGet)
                 {
                     throw new Exception();
                 }
                 return _twinkleView;
            }

            var newTwinkleView = new InternalTwinkleView();
            var resultAdd = Correspondence.TryAdd(threadId, newTwinkleView);
            if (!resultAdd)
            {
                throw new Exception();
            }

            return newTwinkleView;
        }
    }
}