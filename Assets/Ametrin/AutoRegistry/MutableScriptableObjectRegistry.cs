using System;
using System.Collections.Generic;
using System.Linq;
using Ametrin.Utils;
using UnityEditor;
using UnityEngine;

namespace Ametrin.AutoRegistry{
    public sealed class MutableScriptableObjectRegistry<TKey, TValue> : IMutableAutoRegistry<TKey, TValue> where TValue : ScriptableObject{
        private readonly Func<TValue, TKey> KeyProvider;
        private readonly Dictionary<TKey, TValue> Entries = new();
        public int Count => Entries.Count;
        public IReadOnlyCollection<TKey> Keys => Entries.Keys;


        public TValue this[TKey key]{
            get => Entries[key];
            set{
                Entries[key] = value;
            }
        }

        public MutableScriptableObjectRegistry(Func<TValue, TKey> keyProvider, bool autoInit = false){
            KeyProvider = keyProvider;
            if(autoInit) Init();
        }

        public void Init(){
            var values = AssetDatabase.FindAssets($"t: {typeof(TValue).Name}").Select(AssetDatabase.GUIDToAssetPath).Select((path) => AssetDatabase.LoadAssetAtPath<TValue>(path));
            foreach(var item in values){
                Entries.Add(KeyProvider(item), item);
            }
        }

        public Result TryRegister(TKey key, TValue value){
            if(Entries.TryAdd(key, value)){
                return ResultStatus.Succeeded;
            }
            return ResultStatus.AlreadyExists;
        }

        public Result<TValue> TryGet(TKey key){
            if(Entries.TryGetValue(key, out var value)){
                return value;
            }
            return ResultStatus.ValueDoesNotExist;
        }
    }
}
