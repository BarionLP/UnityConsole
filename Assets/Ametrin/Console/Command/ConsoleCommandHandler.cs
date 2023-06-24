using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Ametrin.Console.Command{
    public sealed class ConsoleCommandHandler : MonoBehaviour, IConsoleHandler {
        private static readonly Dictionary<string, CommandMethodInfo> commands = new();
        private static readonly Dictionary<Type, Func<string, object>> argumentParsers = new();
        public bool PassPrefix => false;

        private void Awake(){
            ConsoleManager.RegisterHandler('/', this);
        }

        public static void RegisterArgumentParser<T>(Func<string, T> argumentParser){
            argumentParsers[typeof(T)] = value => argumentParser(value);
        }

        public static void RegisterCommands<T>(){
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            
            foreach (var method in methods){
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute is null) continue;
                
                var commandName = attribute.Name ?? method.Name.ToLower();
                var isAsync = method.ReturnType == typeof(Task);
                commands[commandName] = new CommandMethodInfo(method, isAsync);
            }
        }

        public void Execute(string input){
            var inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (inputParts.Length == 0) return;

            var commandName = inputParts[0];
            if (!commands.TryGetValue(commandName, out var commandMethod)){
                ConsoleManager.AddErrorMessage("Command not found: " + commandName);
                return;
            }

            var method = commandMethod.Method;
            var isAsync = commandMethod.IsAsync;
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];

            if(parameters.Length < inputParts.Length-1) ConsoleManager.AddErrorMessage($"Too many arguments: expected {parameters.Length} got {inputParts.Length-1}");

            for (var i = 0; i < parameters.Length; i++){
                var parameter = parameters[i];

                object arg = null;
                
                if(i + 1 < inputParts.Length){
                    arg = ConvertArgument(inputParts[i + 1], parameter.ParameterType);
                }
                
                if(arg is null){
                    if(!parameter.HasDefaultValue){
                        ConsoleManager.AddErrorMessage($"Missing or invalid argument: {parameter.Name}");
                        return;
                    }
                    arg = parameter.DefaultValue;
                }

                args[i] = arg;
            }

            // if (isAsync){
            //     _ = (Task) method.Invoke(instance, args);
            // }
            // else{
            //     method.Invoke(instance, args);
            // }

            method.Invoke(null, args);
        }

        private static object ConvertArgument(string argValue, Type targetType){
            if(argumentParsers.TryGetValue(targetType, out var argumentParser)){
                return argumentParser(argValue);
            }
            try{
                return Convert.ChangeType(argValue, targetType);
            }catch{
                return null;
            }
        }

        private sealed class CommandMethodInfo{
            public MethodInfo Method { get; }
            public bool IsAsync { get; }

            public CommandMethodInfo(MethodInfo method, bool isAsync){
                Method = method;
                IsAsync = isAsync;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute{
        public string Name { get; set; }

        public CommandAttribute(){}

        public CommandAttribute(string name){
            Name = name;
        }
    }
}