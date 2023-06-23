using Ametrin.AutoRegistry;
using UnityEngine;
using System;
using System.Linq;

namespace Ametrin.Console.Command
{
    public class ConsoleCommandHandler : MonoBehaviour, IConsoleHandler{
        private static readonly ScriptableObjectRegistry<string, Command> CommandRegisty = new(command => command.Prefix);

        private void Awake(){
            CommandRegisty.Init();
            ConsoleManager.RegisterHandler('/', this);
        }

        public bool PassPrefix => false;

        public void Execute(string value){
            var args = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(!CommandRegisty.TryGet(args[0]).TryGet(out var command)){
                ConsoleManager.AddErrorMessage($"Unkown command '{args[0]}'");
                return;
            }

            try{
                command.Execute(args.Select((arg, i)=> CommandArgumentHelper.TryParse(command.Arguments[i], arg).Get()).ToArray()); 
            }catch{
                ConsoleManager.AddErrorMessage($"Invalid command '{args[0]}'");
            }
        }

        public void TP(Command.ExecutedEventArgs args){

        }
    }
}
