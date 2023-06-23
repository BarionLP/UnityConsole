using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AmetrinStudios.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ametrin.Console{
    public class ConsoleManager : MonoBehaviour{
        public static ConsoleManager Instance {get; private set;}
        private static VisualElement ConsoleElement;
        private static TextField InputElement;
        private readonly static Dictionary<char, IConsoleHandler> Handlers = new();
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
        }

        private static void OnInputChanged(string value){

        }

        private static void Enter(){
            var value = InputElement.value;
            InputElement.value = "";
            if(!GetHandler(value).TryGet(out var handler)){
                Debug.Log(value);
                return;
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

        public static void Hide() => Instance.gameObject.SetActive(false);
        public static void Show() => Instance.gameObject.SetActive(true);
    }

#nullable enable
    public sealed class ConsoleHandler : IConsoleHandler{
        public bool PassPrefix { get; set; }
        private readonly Func<string, string?> OnExecute;
        public ConsoleHandler(Func<string, string?> execute, bool passPrefix = false){
            OnExecute = execute;
            PassPrefix = passPrefix;
        }

        public string? Execute(string value){
            return OnExecute(value);
        }
    }

    public interface IConsoleHandler{
        public bool PassPrefix {get;}

        public string? Execute(string value); 
        public string? GetSyntax(string value) => null;
    }
}
