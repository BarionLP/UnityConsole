using System;
using System.Linq;
using System.Collections.Generic;
using AmetrinStudios.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.DedicatedServer;

namespace Ametrin.Console.Command{
    [CreateAssetMenu]
    public sealed class Command : ScriptableObject{
        [SerializeField] private string _Prefix;
        [SerializeField] private Argument[] _Arguments;
        [SerializeField] private bool _RunAsync;
        [SerializeField] private UnityEvent<ExecutedEventArgs> OnExecuted = new();

        public string Prefix => _Prefix;
        public Argument[] Arguments => _Arguments;
        public bool RunAsync => _RunAsync;
        public int ArgumentCount => Arguments.Length;

        public void Execute(object[] args){
            OnExecuted?.Invoke(new(args));
        }


        [Serializable]
        public sealed class Argument{
            [field: SerializeField] public string Name {get; private set;}
            [SerializeField] private string TypeName = typeof(int).Name;
            [field: SerializeField] public string Default {get; private set;} = string.Empty;
            public Type Type => CommandArgumentHelper.SupportedTypes.FirstOrDefault(type => type.Name == TypeName) ?? typeof(int);
        }

        public sealed class ExecutedEventArgs{
            public readonly object[] Arguments;

            public ExecutedEventArgs(object [] args){
                Arguments = args;
            }
        }
    }

    public static class CommandArgumentHelper{
        public static ICollection<Type> SupportedTypes => Parser.Keys;
        private static readonly Dictionary<Type, ICommandArgumentParser> Parser = new();

        static CommandArgumentHelper(){
            AddArgumentParser<int>(new IntCommandArgumentParser());
            AddArgumentParser<double>(new DoubleCommandArgumentParser());
            AddArgumentParser<float>(new FloatCommandArgumentParser());
        }

        public static Result<T> TryParse<T>(string text){
            if(!Parser.TryGetValue(typeof(T), out var parser)){
                return ResultStatus.ValueDoesNotExist;
            }
            return parser.TryParse(text) as Result<T>;
        }
        public static Result<object> TryParse(this Command.Argument argument, string text)
        {
            if (Parser.TryGetValue(argument.Type, out var parser)){
                if(parser.TryParse(text).TryGet(out var value)){
                    return value;
                }
            }
            return string.IsNullOrWhiteSpace(argument.Default) ? ResultStatus.ValueDoesNotExist : argument.Default;
        }

        public static void AddArgumentParser<T>(ICommandArgumentParser parser){
            Parser.Add(typeof(T), parser);
        }
    }

    public sealed class IntCommandArgumentParser : ICommandArgumentParser{
        public bool TryParse(string text, out object result)
        {
            if (int.TryParse(text, out var value)){
                result = value;
                return true;
            }
            result = null;
            return false;
        }
    }

    public sealed class DoubleCommandArgumentParser : ICommandArgumentParser{
        public bool TryParse(string text, out object result)
        {
            if (double.TryParse(text, out var value)){
                result = value;
                return true;
            }
            result = null;
            return false;
        }
    }
    
    public sealed class FloatCommandArgumentParser : ICommandArgumentParser{
        public bool TryParse(string text, out object result){
            if(float.TryParse(text, out var value)){
                result = value;
                return true;
            }
            result = null;
            return false;
        }
    }

    public interface ICommandArgumentParser{
        public bool TryParse(string text, out object result);
        public Result<object> TryParse(string text){
            if(TryParse(text, out var value)){
                return value;
            }
            return ResultStatus.Failed;
        }
        public string[] GetSuggestions(string text) => Array.Empty<string>();
    }
}
