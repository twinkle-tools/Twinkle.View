using System;
using Twinkle.View.Attributes;
using Twinkle.View.Enums;
using Twinkle.View.Test.Infrastructure.Controls;

namespace Twinkle.View.Test.Infrastructure.Views
{
    public class MainGooglePage:View
    {
        public MainGooglePage()
        {
            AddViewDefinitionCriteria(() => true);
            AddHook(TypeHook.PRE, "Search Input", "SetValue", () => ExampleThrowException("before set value for search input"));
            AddHook(TypeHook.POST, "Search Input", "GetValue", () => ExampleThrowException("after get value for search input"));
        }
        
        [Control(alias: "Search Input", xPath: ".//input[@name='q']", css: "")]
        public TextBox SearchInput { get; set; }
        
        [Control(alias: "Search In Google", xPath: ".//input[@name='btnK']", css: "")]
        public Button SearchInGoogle { get; set; }
        
        [Control(alias: "I`ll Be Lucky", xPath: ".//input[@name='btnI']", css: "")]
        public Button WillBeLucky { get; set; }

        public void ExampleThrowException(string message = "")
        {
            throw new Exception(message);
        }
    }
}