using System;
using Twinkle.View.Attributes;
using Twinkle.View.Enums;
using Twinkle.View.Test.Infrastructure.Controls;

namespace Twinkle.View.Test.Infrastructure.Views
{
    [ViewBase(alias: "Main Google page")]
    public class MainGooglePage:View.Infrastructure.View
    {
        public MainGooglePage()
        {
            AddViewDefinitionCriteria(() => true);
            AddHook(TypeHook.PRE, "Search Input", "SetValue", () => ExampleThrowException("before set value for search input"));
            AddHook(TypeHook.POST, "Search Input", "GetValue", () => ExampleThrowException("after get value for search input"));
        }
        
        [ControlBase(alias: "Search Input", xPath: ".//input[@name='q']", css: "")]
        public TextBox SearchInput { get; set; }
        
        [ControlBase(alias: "Search In Google", xPath: ".//input[@name='btnK']", css: "")]
        public Button SearchInGoogle { get; set; }
        
        [ControlBase(alias: "I`ll Be Lucky", xPath: ".//input[@name='btnI']", css: "")]
        public Button WillBeLucky { get; set; }

        public void ExampleThrowException(string message = "")
        {
            throw new Exception(message);
        }
    }
}