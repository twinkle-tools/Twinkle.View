using System;

namespace Twinkle.View.Attributes
{
    /// <summary>
    /// Marks property as view control
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ControlBaseAttribute:TwinkleBaseAttribute
    {
        public ControlBaseAttribute(string alias, string xPath, string css)
        {
            Alias = alias;
            XPath = xPath;
            Css = css;
        }

        public ControlBaseAttribute() { }

        /// <summary>
        /// Provides possible control call names
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Show the entry point to the control in XPath format
        /// </summary>
        public string XPath { get; set; }

        /// <summary>
        /// Show the entry point to the control in Css format
        /// </summary>
        public string Css { get; set; }
    }
}