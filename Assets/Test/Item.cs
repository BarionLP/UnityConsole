using Ametrin.AutoRegistry;
using UnityEngine;

[CreateAssetMenu]
public sealed class Item : ScriptableObject{
    public static readonly ScriptableObjectRegistry<string, Item> Registry = new(item => item.name);
}
