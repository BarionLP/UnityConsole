using System;

namespace Ametrin.Console.Command{
    public interface ICommandArgumentParser{
        public object Parse(string raw);

        public string[] GetSuggestions() => Array.Empty<string>();
    }
}
