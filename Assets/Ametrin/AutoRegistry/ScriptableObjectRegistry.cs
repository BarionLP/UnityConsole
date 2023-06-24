using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Ametrin.Utils;

namespace Ametrin.AutoRegistry{
    public sealed class ScriptableObjectRegistry<TKey, TValue> : IAutoRegistry<TKey, TValue> where TValue : ScriptableObject{
        private readonly MutableScriptableObjectRegistry<TKey, TValue> Registry;
        public int Count => Registry.Count;

        public IReadOnlyCollection<TKey> Keys => Registry.Keys;

        public TValue this[TKey key] => Registry[key];

        public ScriptableObjectRegistry(Func<TValue, TKey> keyProvider, bool autoInit = false){
            Registry = new(keyProvider, autoInit);
        }

        public Result<TValue> TryGet(TKey key) => Registry.TryGet(key);

        public void Init()=> Registry.Init();
    }
}
