using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ametrin.Console.Command.Editors{

    [CustomEditor(typeof(Command))]
    public sealed class CommandEditor : Editor{
        [SerializeField] private VisualTreeAsset UXML;

        public override VisualElement CreateInspectorGUI(){
            return UXML.Instantiate();
        }
    }
}