using System;
using System.Collections.Generic;
using AmetrinStudios.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ametrin.Console{
    public sealed class ConsoleManager : MonoBehaviour{
        public static ConsoleManager Instance {get; private set;}
        private static VisualElement ConsoleElement;
        private static TextField InputElement;
        private static Label MessageDisplayElement;
        private static IConsoleHandler DefaultHandler = new ConsoleMessageHandler(AddMessage, true);
        private readonly static Dictionary<char, IConsoleHandler> Handlers = new();
        private readonly static List<string> Messages = new();
        private void Awake(){
            if(Instance != null && Instance != this){
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            ConsoleElement = GetComponent<UIDocument>().rootVisualElement;
            InputElement = ConsoleElement.Query<TextField>();
            InputElement.RegisterValueChangedCallback((value) => OnInputChanged(value.newValue));
            InputElement.RegisterCallback<KeyUpEvent>((key) => {if(key.keyCode is KeyCode.Return or KeyCode.KeypadEnter) Enter();});
            MessageDisplayElement = ConsoleElement.Query<Label>();
            Messages.Clear();
        }

        private static void OnInputChanged(string value){

        }

        private static void Enter(){
            var value = InputElement.value;
            InputElement.value = "";
            if(!GetHandler(value).TryGet(out var handler)){
                handler = DefaultHandler;
            }

            if(!handler.PassPrefix){
                value = value.Remove(0, 1);
            }

            handler.Execute(value);
        }

        private static Result<IConsoleHandler> GetHandler(string input){            
            foreach(var handler in Handlers){
                if(input.StartsWith(handler.Key)){
                    return Result<IConsoleHandler>.Succeeded(handler.Value);
                }
            }

            return ResultStatus.ValueDoesNotExist;
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

        public static void Hide() => Instance.gameObject.SetActive(false);
        public static void Show() => Instance.gameObject.SetActive(true);

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
