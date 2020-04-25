using Twinkle.View.Attributes;

namespace Twinkle.View.Test.Infrastructure.Controls
{
    public class TextBox:Control
    {
        [Operation(Alias = "SetValue")]
        public void SetValue(string value)
        {
            
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