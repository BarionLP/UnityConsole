using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Ametrin.Utils;

namespace Ametrin.AutoRegistry{
    public interface IAutoRegistry<TKey, TValue> : IRegistry<TKey, TValue> where TValue : UnityEngine.Object{
        void Init();
    }

    public interface IMutableAutoRegistry<TKey, TValue> : IMutableRegistry<TKey, TValue>, IAutoRegistry<TKey, TValue> where TValue : UnityEngine.Object{
        public new void Init();
    }
}
