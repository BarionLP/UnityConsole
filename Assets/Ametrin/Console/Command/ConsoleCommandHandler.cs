using System;

namespace Ametrin.Console.Command{
    #nullable enable
    public sealed class ConsoleCommandHandler : IConsoleHandler {
        public bool PassPrefix => false;

        public void Execute(string input)=> CommandManager.Execute(input);

        public string? GetSyntax(string value){
            var inputParts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(inputParts.Length == 0) return null;
            return CommandManager.GetSyntax(inputParts[0]);
        }
    }
}