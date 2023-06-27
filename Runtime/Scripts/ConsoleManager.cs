using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Ametrin.Console{
    public sealed class ConsoleManager : MonoBehaviour{
        public static ConsoleManager Instance {get; private set;}
        private static UIDocument Document;
        private static VisualElement ConsoleElement;
        private static TextField InputElement;
        private static Label SyntaxHintLabel;
        private static Label MessageDisplayElement;
        private static IConsoleHandler DefaultHandler = new ConsoleMessageHandler(AddMessage, true);
        private readonly static Dictionary<char, IConsoleHandler> Handlers = new();
        private readonly static List<string> Messages = new();
        private static KeyValuePair<char, IConsoleHandler> CachedHandler;
        
        [SerializeField] private InputAction ShowInput;
        [SerializeField] private InputAction HideInput;
        
        private void Awake(){
            if(Instance != null && Instance != this){
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;

            Document = GetComponent<UIDocument>();
            ConsoleElement = Document.rootVisualElement;
            
            InputElement = ConsoleElement.Query<TextField>();
            InputElement.RegisterValueChangedCallback((value) => OnInputChanged(value.newValue));
            InputElement.RegisterCallback<KeyUpEvent>((key) => {if(key.keyCode is KeyCode.Return or KeyCode.KeypadEnter) Enter();});
            
            MessageDisplayElement = ConsoleElement.Query<Label>("output");
            SyntaxHintLabel = ConsoleElement.Query<Label>("syntax");
            SyntaxHintLabel.style.display = DisplayStyle.None;

            Messages.Clear();
            ShowInput.performed += (_)=> Show();
            HideInput.performed += (_)=> Hide();
            Hide();
        }

        private static void OnInputChanged(string input){
            var handler = GetHandler(input);
            if (!handler.PassPrefix){
                input = input.Remove(0, 1);
            }
            var syntax = handler.GetSyntax(input);

            if(syntax is null){
                SyntaxHintLabel.style.display = DisplayStyle.None;
                return;
            }
            SyntaxHintLabel.text = syntax;
            SyntaxHintLabel.style.display = DisplayStyle.Flex;
        }

        private static void Enter(){
            var input = InputElement.value;
            InputElement.value = "";
            var handler = GetHandler(input);

            if(!handler.PassPrefix){
                input = input.Remove(0, 1);
            }

            handler.Execute(input);
            Debug.Log(input);
        }

        private static IConsoleHandler GetHandler(string input){
            if (CachedHandler.Value != null && input.StartsWith(CachedHandler.Key)){
                return CachedHandler.Value;
            }

            foreach(var handler in Handlers){
                if(input.StartsWith(handler.Key)){
                    CachedHandler = handler;
                    return handler.Value;
                }
            }
            
            CachedHandler = default;
            return DefaultHandler;
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

        public static void AddMessage(string message){
            Messages.Add(message);
            UpdateView();
        }
        
        public static void AddErrorMessage(string message){
            AddMessage($"<color=red>{message}</color>");
        }

        public static void Hide(){
            Instance.HideInput.Disable();
            ConsoleElement.style.visibility = Visibility.Hidden;
            Instance.ShowInput.Enable();
        }
        public static void Show(){
            Instance.ShowInput.Disable();
            ConsoleElement.style.visibility = Visibility.Visible;
            Instance.HideInput.Enable();

        }

        private static void UpdateView(){
            MessageDisplayElement.text = string.Join("\n", Messages);
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

        public void Execute(string value){
            OnExecute(value);
        }
    }

    public interface IConsoleHandler{
        public bool PassPrefix {get;}
        public void Execute(string value); 
        public string? GetSyntax(string value) => null;
    }
}
