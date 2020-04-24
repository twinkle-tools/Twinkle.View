using System;

namespace Twinkle.View.Attributes
{
    /// <summary>
    /// Marks methods that execute before methods, that defines view
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PreViewDefinitionCriteriaAttribute : TwinkleBaseAttribute
    {

    }
}