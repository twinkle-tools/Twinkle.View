using System;

namespace Twinkle.View.Attributes
{
    /// <summary>
    /// Marks clases as view description
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public class ViewAttribute:TwinkleBaseAttribute
    {
        /// <summary>
        /// Provides possible view call names
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Indicates which prefix will be added to each control aliases
        /// </summary>
        public string Prefix { get; set; }

        public ViewAttribute(string alias)
        {
            Alias = alias;
        }

        public ViewAttribute(string alias, string prefix)
        {
            Alias = alias;
            Prefix = prefix;
        }
    }
}