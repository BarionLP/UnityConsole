using System;
using UnityEngine;

namespace Ametrin.Console.Command{
    public sealed class Command : ScriptableObject{
        [field: SerializeField] public string Prefix { get; private set; }
        [field: SerializeField] public Type[] ArgumentTypes { get; private set; }
        [field: SerializeField] public bool RunAsync { get; private set; }
        public int ArgumentCount => ArgumentTypes.Length;
    }
}
