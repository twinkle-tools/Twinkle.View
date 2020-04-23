using System;

namespace Twinkle.View.Attributes
{
    /// <summary>
    /// Marks property as view control
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ControlAttribute:TwinkleBaseAttribute
    {
        public ControlAttribute(string alias, string xPath, string css)
        {
            Alias = alias;
            XPath = xPath;
            Css = css;
        }

        public ControlAttribute() { }

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