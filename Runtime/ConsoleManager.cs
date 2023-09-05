using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ametrin.Console{
    public sealed class ConsoleManager : MonoBehaviour{
        public static event Action OnShow;
        public static event Action OnHide;
        public static bool IsVisible => ConsoleElement.style.visibility.value == Visibility.Visible;
        private static ConsoleManager _Instance;
        public static ConsoleManager Instance{
            get{
                if (_Instance != null){
                    if(FindAnyObjectByType<ConsoleManager>(FindObjectsInactive.Exclude) is not ConsoleManager instance) throw new Exception("No active Console found!");
                    _Instance = instance;
                }
                
                return _Instance;
            }
        }

        private static UIDocument Document;
        private static VisualElement ConsoleElement;
        private static TextField InputElement;
        private static Label SyntaxHintLabel;
        private static Label MessageDisplayElement;
        private static IConsoleHandler DefaultHandler = new ConsoleMessageHandler(AddMessage, false);
        private readonly static Dictionary<char, IConsoleHandler> Handlers = new();
        private readonly static List<string> Messages = new();
        
        private void Awake(){
            if (_Instance != null && _Instance != this){
                Debug.LogError("Created duplicate console manager");
                DestroyImmediate(gameObject);
                return;
            }
            Document = GetComponent<UIDocument>();
            ConsoleElement = Document.rootVisualElement;
            
            InputElement = ConsoleElement.Query<TextField>();
            InputElement.RegisterValueChangedCallback(OnInputChanged);
            InputElement.RegisterCallback<KeyDownEvent>(HandleKeys);
            
            MessageDisplayElement = ConsoleElement.Query<Label>("output");
            SyntaxHintLabel = ConsoleElement.Query<Label>("syntax");
            SyntaxHintLabel.style.display = DisplayStyle.None;
            Hide();

            Messages.Clear();
        }

        private void OnInputChanged(ChangeEvent<string> changeEvent){
            var input = ReadInput();
            if (input.IsEmpty){
                SyntaxHintLabel.style.display = DisplayStyle.None;
                return;
            }
            var handler = GetHandler(input);
            if(!handler.PassPrefix && handler != DefaultHandler){
                input = input[1..];
            }
            UpdateHint(input, handler);
        }
        private static void HandleKeys(KeyDownEvent upEvent){
            var input = ReadInput();
            if (input.IsEmpty) return;
            var handler = GetHandler(input);
            char prefix;
            if (handler.PassPrefix || handler == DefaultHandler){
                prefix = '\0';
            } else{
                prefix = input[0];
                input = input[1..];
            }

            if (upEvent.keyCode is KeyCode.Return or KeyCode.KeypadEnter){
                HandleInput(input, handler);
                upEvent.PreventDefault();
                upEvent.StopPropagation();
            }
            if(upEvent.keyCode is KeyCode.Tab){
                AutoComplete(input, handler, prefix);
                upEvent.PreventDefault();
                upEvent.StopPropagation();
            }
        }

        private static void UpdateHint(ReadOnlySpan<char> input, IConsoleHandler handler){
            var hint = handler.GetHint(input);

            SyntaxHintLabel.style.display = string.IsNullOrWhiteSpace(hint) ? DisplayStyle.None : DisplayStyle.Flex;
            SyntaxHintLabel.text = hint;
        }

        private static void HandleInput(ReadOnlySpan<char> input, IConsoleHandler handler){
            InputElement.value = string.Empty;
            handler.Handle(input);
            FocusInput(0);
        }

        private static void AutoComplete(ReadOnlySpan<char> input, IConsoleHandler handler, char prefix){
            var completion = handler.GetAutoCompleted(input);
            if(string.IsNullOrWhiteSpace(completion)) return;
            InputElement.value = prefix == '\0' || handler.PassPrefix ? completion : prefix + completion;
            FocusInput();
        }

        private static IConsoleHandler GetHandler(ReadOnlySpan<char> input){
            if(!input.IsEmpty && Handlers.TryGetValue(input[0], out var handler)) return handler;
            
            return DefaultHandler;
        }

        public static void Hide(){
            ConsoleElement.style.visibility = Visibility.Hidden;
            OnHide?.Invoke();
        }
        public static void Show(){
            ConsoleElement.style.visibility = Visibility.Visible;
            FocusInput();
            UpdateView();
            OnShow?.Invoke();
        }

        private static ReadOnlySpan<char> ReadInput(){
            if(string.IsNullOrWhiteSpace(InputElement.value)) return ReadOnlySpan<char>.Empty;
            return InputElement.value.AsSpan();
        }
        private static void UpdateView(){
            MessageDisplayElement.text = string.Join("\n", Messages);
        }
        private static void FocusInput(int idx = -1){
            // InputElement.Focus();
            if(idx == -1) idx = InputElement.value.Length;
            InputElement.SelectRange(idx, idx);
        }

        public static void AddMessage(string message){
            Messages.Add(message);
            if(IsVisible) UpdateView();
        }
        public static void AddWarningMessage(string message){
            AddMessage($"<color=yellow>{message}</color>");
        }
        public static void AddErrorMessage(string message){
            AddMessage($"<color=red>{message}</color>");
        }
        public static void AddExceptionMessage(string message){
            AddMessage($"<color=#cc0000ff>{message}</color>");
        }

        public static void RegisterHandler<T>(char prefix) where T : IConsoleHandler, new() => RegisterHandler(prefix, new T());
        public static void RegisterHandler(char prefix, IConsoleHandler handler){
            if(Handlers.ContainsKey(prefix)){
                throw new ArgumentException($"console handler prefix '{prefix}' already exists", nameof(prefix));
            }
            Handlers.Add(prefix, handler);
        }

        public static void OverrideDefaultHandler(IConsoleHandler handler){
            DefaultHandler = handler;
        }
    }

    #nullable enable
    public sealed class ConsoleMessageHandler : IConsoleHandler{
        public bool PassPrefix { get; set; }
        private readonly Action<string> OnExecute;
        public ConsoleMessageHandler(Action<string> execute, bool passPrefix = false){
            OnExecute = execute;
            PassPrefix = passPrefix;
        }

        public void Handle(ReadOnlySpan<char> input) => OnExecute(input.ToString());
    }

    public interface IConsoleHandler{
        public bool PassPrefix {get;}
        public void Handle(ReadOnlySpan<char> input); 
        public string GetHint(ReadOnlySpan<char> input) => string.Empty;
        public string GetAutoCompleted(ReadOnlySpan<char> input) => string.Empty;
    }
}
