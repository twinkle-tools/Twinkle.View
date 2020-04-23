using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Twinkle.View.Infrastructure
{
    internal class GlobalConfiguraionFramework
    {
        internal bool AutomaticViewDetection { get; set; }
        internal List<string> DllContainingViews { get; set; }
        internal List<string> DllContainingControls { get; set; }
        internal int SearchControlNumberOfRetries { get; set; }
        internal int SearchControlTimeBetweenRetries { get; set; }
        internal bool LoggingEnable { get; set; }

        public GlobalConfiguraionFramework(IConfiguration config)
        {
            AutomaticViewDetection = Boolean.Parse(config["view:automatic_view_detection"]);
            DllContainingViews = config["view:dll_containing_views"].Split(',').ToList();

            DllContainingControls = config["control:dll_containing_controls"].Split(',').ToList();
            SearchControlNumberOfRetries = Convert.ToInt32(config["control:search_control_in_active_views:number_of_retries"]);
            SearchControlTimeBetweenRetries = Convert.ToInt32(config["control:search_control_in_active_views:time_between_retries_ms"]);
            
            LoggingEnable = Boolean.Parse(config["common:logging_enable"]);
        }
    }
}
