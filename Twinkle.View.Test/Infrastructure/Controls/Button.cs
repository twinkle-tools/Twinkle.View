using Twinkle.View.Attributes;

namespace Twinkle.View.Test.Infrastructure.Controls
{
    public class Button:Control
    {
        public Button(string alias, string xpath, string css, TwinkleView twinkleView) : base(alias, xpath, css, twinkleView)
        {
        }

        [Operation(Alias = "Click")]
        public void Click()
        {
            
        }
    }
}