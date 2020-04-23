using System.Collections.Generic;

namespace Twinkle.View
{
    public abstract class Control
    {
        public string Alias { get; set; }
        public string XPath { get; set; }
        public string Css { get; set; }
        public List<Hook> Hooks { get; set; }
        public TwinkleView TwinkleView { get; set; }

        public Control(string alias, string xpath, string css, TwinkleView twinkleView)
        {
            Alias = alias;
            XPath = xpath;
            Css = css;
            TwinkleView = twinkleView;
        }

    }
}