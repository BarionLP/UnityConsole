using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Ametrin.Utils.Unity;

namespace Ametrin.Console{
    public sealed class ConsoleManager : MonoBehaviourSingleton<ConsoleManager>{
        public static event Action OnShow;
        public static event Action OnHide;
        public static bool IsVisible => ConsoleElement.style.visibility.value == Visibility.Visible;

        private static UIDocument Document;
        private static VisualElement ConsoleElement;
        private static TextField InputElement;
        private static Label SyntaxHintLabel;
        private static Label MessageDisplayElement;
        private static IConsoleHandler DefaultHandler = new ConsoleMessageHandler(AddMessage, false);
        private readonly static Dictionary<char, IConsoleHandler> Handlers = new();
        private readonly static List<string> Messages = new();
        
        protected override void Awake(){
            base.Awake();

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
            UpdateSyntaxHint(input, handler);
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
            }
            if(upEvent.keyCode is KeyCode.Tab){
                AutoComplete(input, handler, prefix);
                upEvent.PreventDefault();
            }
        }


        private static void UpdateSyntaxHint(ReadOnlySpan<char> input, IConsoleHandler handler){
            var syntax = handler.GetSyntax(input);

            SyntaxHintLabel.style.display = syntax.IsEmpty ? DisplayStyle.None : DisplayStyle.Flex;
            SyntaxHintLabel.text = syntax.ToString();
        }

        private static void HandleInput(ReadOnlySpan<char> input, IConsoleHandler handler){
            InputElement.value = string.Empty;
            handler.Execute(input);
            FocusInput(0);
        }

        private static void AutoComplete(ReadOnlySpan<char> input, IConsoleHandler handler, char prefix){
            var completion = handler.GetAutoCompleted(input);
            if(string.IsNullOrWhiteSpace(completion)) return;
            InputElement.value = prefix == '\0' ? completion : prefix + completion;
            FocusInput();
        }

        private static IConsoleHandler GetHandler(ReadOnlySpan<char> input){
            if(!input.IsEmpty && Handlers.TryGetValue(input[0], out var handler)) return handler;
            
            return DefaultHandler;
        }

        public static void Hide(InputAction.CallbackContext context = default){
            ConsoleElement.style.visibility = Visibility.Hidden;
            OnHide?.Invoke();
        }
        public static void Show(InputAction.CallbackContext context = default){
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

        public void Execute(ReadOnlySpan<char> input){
            OnExecute(input.ToString());
        }
    }

    public interface IConsoleHandler{
        public bool PassPrefix {get;}
        public void Execute(ReadOnlySpan<char> input); 
        public ReadOnlySpan<char> GetSyntax(ReadOnlySpan<char> input) => ReadOnlySpan<char>.Empty;
        public string GetAutoCompleted(ReadOnlySpan<char> input) => string.Empty;
    }
}
