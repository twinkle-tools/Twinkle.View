using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Twinkle.View.Infrastructure
{
    public class GlobalConfiguraionFramework
    {
        public bool AutomaticViewDetection { get; internal set; }
        public List<string> DllContainingViews { get; internal set; }
        public List<string> DllContainingControls { get; internal set; }
        public int SearchControlNumberOfRetries { get; internal set; }
        public int SearchControlTimeBetweenRetries { get; internal set; }
        public bool LoggingEnable { get; internal set; }

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
