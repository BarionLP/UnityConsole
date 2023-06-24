using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ametrin.Console.Command{
    public static class CommandManager{
        private static readonly Dictionary<string, (MethodInfo info, string syntax)> Commands = new();
        private static readonly Dictionary<Type, ICommandArgumentParser> ArgumentParsers = new();

        public static void RegisterArgumentParser<T>(ICommandArgumentParser argumentParser){
            ArgumentParsers[typeof(T)] = argumentParser;
        }

        public static void RegisterCommands<T>(){
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var method in methods){
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute is null) continue;

                var commandName = attribute.Name ?? method.Name.ToLower();
                Commands[commandName] = (method, GenerateCommandSytanx(commandName, method));
            }
        }

        public static void Execute(string input){
            var inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(inputParts.Length == 0) return;

            var commandName = inputParts[0];
            if (!Commands.TryGetValue(commandName, out var command)){
                ConsoleManager.AddErrorMessage("Command not found: " + commandName);
                return;
            }

            var parameters = command.info.GetParameters();
            var args = new object[parameters.Length];

            if(parameters.Length < inputParts.Length - 1) ConsoleManager.AddErrorMessage($"Too many arguments: expected {parameters.Length} got {inputParts.Length - 1}");

            for(var i = 0; i < parameters.Length; i++){
                var parameter = parameters[i];

                object arg = null;

                if(i + 1 < inputParts.Length){
                    arg = ConvertArgument(inputParts[i + 1], parameter.ParameterType);
                }

                if(arg is null){
                    if (!parameter.HasDefaultValue){
                        ConsoleManager.AddErrorMessage($"Missing or invalid argument: {parameter.Name}");
                        return;
                    }
                    arg = parameter.DefaultValue;
                }

                args[i] = arg;
            }

            command.info.Invoke(null, args);
        }

        private static object ConvertArgument(string argValue, Type targetType){
            if (ArgumentParsers.TryGetValue(targetType, out var argumentParser)){
                return argumentParser.Parse(argValue);
            }

            try{
                return Convert.ChangeType(argValue, targetType);
            }
            catch{
                return null;
            }
        }

        public static string GetSyntax(string commandKey){
            foreach(var command in Commands){
                if(command.Key.StartsWith(commandKey)) return command.Value.syntax;
            }
            
            return null;
        }

        private static string GenerateCommandSytanx(string key, MethodInfo method){   
            var builder = new StringBuilder(key);
            
            foreach(var parameter in method.GetParameters()){
                if(parameter.HasDefaultValue){
                    builder.AppendFormat(" [<{0}>]", parameter.Name);
                }else{
                    builder.AppendFormat(" <{0}>", parameter.Name);
                }
            }

            return builder.ToString();
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute{
        public string Name { get; set; }

        public CommandAttribute() { }

        public CommandAttribute(string name){
            Name = name;
        }
    }
}