using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using AmetrinStudios.Utils;

namespace Ametrin.AutoRegistry{
    public sealed class ScriptableObjectRegistry<TKey, T> where T : ScriptableObject{
        private readonly Func<T, TKey> KeyProvider;
        private readonly Dictionary<TKey, T> Entries = new();
        public int Count => Entries.Count;

        public ScriptableObjectRegistry(Func<T, TKey> keyProvider, bool autoInit = false){
            KeyProvider = keyProvider;

            if(autoInit) Init();
        }

        public Result<T> TryGet(TKey key){
            if(Entries.TryGetValue(key, out var value)){
                return value;
            }
            return ResultStatus.ValueDoesNotExist;
        }

        public void Init(){
            var values = AssetDatabase.FindAssets($"t: {typeof(T).Name}").Select(AssetDatabase.GUIDToAssetPath).Select((path) => AssetDatabase.LoadAssetAtPath<T>(path));
            foreach (var item in values){
                Entries.Add(KeyProvider(item), item);
            }
        }
    }
}
