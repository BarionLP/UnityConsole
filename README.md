# UnityConsole
A Unity Runtime Console using UI Toolkit

## Usage
- requires Unity UI package (UI Toolkit)
- new input system recommended
- install Unity Console package https://github.com/BarionLP/UnityConsole.git 
- add the ConsolePrefab to your scene

### Add Messages
```csharp
ConsoleManager.AddMessage("message");
ConsoleManager.AddMessage("<color=yellow>message</color>"); //rich text
ConsoleManager.AddWarningMessage("warning"); //yellow
ConsoleManager.AddErrorMessage("error"); //red
```
### Hide and Show
`ConsoleManager.Hide()`/`ConsoleManager.Show()`<br>
The `ConsoleToggle` component handles hiding and showing the console automatically<br>
You can listen to `ConsoleManager.OnShow` and `ConsoleManager.OnHide`  

### Input Handlers
by default the console just prints input messages
```csharp
// override the default handler
ConsoleManager.OverrideDefaultHandler(new ConsoleMessageHandler(input => {}));
//registering handlers
char prefix = '/'; //inputs staring with this prefix are handed to this handler
ConsoleManager.RegisterHandler(prefix, new ConsoleMessageHandler(input => {}));
```

#### Custom Handlers
```csharp
//implement IConsoleHandler
public sealed class CustomHandler : IConsoleHandler {
    //whether the prefix should be removed before calling
    public bool PassPrefix => false;
    
    public void Handle(ReadOnlySpan<char> input){
        //whatever you want
    }

    //optional
    public string GetHint(ReadOnlySpan<char> input){
        // displays a hint above the text input, can be empty
        // runs whenever the input changes, so be performace aware 
    }

    //optional
    public string GetAutoCompleted(ReadOnlySpan<char> input){
        // called when tab is pressed
        // return the entire completed string or empty 
    }
}
```

## Tipps
If you want to run commands from this console consider using https://github.com/BarionLP/CommandSystem with https://github.com/BarionLP/UnityConsoleCommandIntegration
