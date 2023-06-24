using System.Collections.Generic;
using Ametrin.Utils;

namespace Ametrin.AutoRegistry{
    public interface IRegistry<TKey, TValue>{
        public IReadOnlyCollection<TKey> Keys {get;}
        public int Count {get;}
        public TValue this[TKey key] {get;}
        public Result<TValue> TryGet(TKey key);
    }

    public interface IMutableRegistry<TKey, TValue> : IRegistry<TKey, TValue>{
        public new TValue this[TKey key] { get; set; }
        public Result TryRegister(TKey key, TValue value);
    }
}
