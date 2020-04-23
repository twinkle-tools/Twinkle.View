using System;
using Twinkle.View.Attributes;

namespace Twinkle.View.Test.Infrastructure.Controls
{
    public class TextBox:Control
    {
        public TextBox(string alias, string xpath, string css, TwinkleView twinkleView) : base(alias, xpath, css, twinkleView)
        {
        }

        [Operation(Alias = "SetValue")]
        public void SetValue(string value)
        {
            Console.WriteLine("");
        }
        
        [Operation(Alias = "GetValue")]
        public string GetValue()
        {
            return "";
        }
        
        [Operation(Alias = "ClearValue")]
        public void ClearValue()
        {
            
        }
        
        [Operation(Alias = "AddValue")]
        public void AddValue(string value)
        {
            
        }
    }
}