# Twinkle.View
###### Modern PageObject implementation for UI testing

## Introduction
Twinkle.View is a project that implements the well-known PageObject patern, but we add additional features to the project that extend the capabilities of 
of the standard automation approach and make Qa automation's life a little better. The project is not tied to a particular technology/protocol, 
so it can be used to test web, mobile, desktop ui. The applied approach helps to reduce costs at the beginning of automation of new projects, 
and support existing ones.

## Installation
- install the nuget package
- copy the FodyWeavers.xml file to the project and install 'Copy to output directory: Copy always/Copy if newer'
- copy the TwinkleViewSettings.json file to the project and install 'Copy to output directory: Copy always/Copy if newer'
- configure TwinkleViewSettings.json
- enjoy

## Basic use

###### View class
```
public class MainGooglePage:View
{
    public MainGooglePage()
    {
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
```

###### Controls class
```
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
```
```
public class Button:Control
{
    [Operation(Alias = "Click")]
    public void Click()
    {
            
    }
}
```

###### Test class
```
[TestFixture]
public class Hooks
{ 
    [Test]
    public void ExecuteOperationWithPreHook()
    {
        TwinkleView.GetView<MainGooglePage>().SearchInput.SetValue("value");
        TwinkleView.GetView<MainGooglePage>().SearchInGoogle.Click();
    }        
}
```

## License
Twinkle.View is open-source project, and distributed under the MIT license