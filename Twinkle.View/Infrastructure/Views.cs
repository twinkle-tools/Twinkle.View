using System;
using System.Collections.Generic;

namespace Twinkle.View.Infrastructure
{
    public class Views
    {
        public Dictionary<Type, object> AllViews { get; internal set; }
        public Dictionary<Type, object> ActiveViews { get; internal set; }
    }
}