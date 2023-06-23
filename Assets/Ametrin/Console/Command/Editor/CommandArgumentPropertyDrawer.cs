using System.Collections;
using System.Collections.Generic;
using Ametrin.Console.Command;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Ametrin.Console.Command.Editors{
    [CustomPropertyDrawer(typeof(Command.Argument))]
    public sealed class CommandArgumentPropertyDrawer : PropertyDrawer{
        public override VisualElement CreatePropertyGUI(SerializedProperty property){
            var root = new VisualElement();
            var name = new PropertyField(property.FindPropertyRelative("<Name>k__BackingField"));
            var dropDown = new DropdownField("Type", GetTypeOptions(), 0){
                bindingPath = "TypeName"
            };
            var defaultValue = new PropertyField(property.FindPropertyRelative("<Default>k__BackingField"));
            //dropDown.RegisterValueChangedCallback(OnTypeChange);
            root.Add(name);
            root.Add(dropDown);
            root.Add(defaultValue);
            
            return root;
        }

        private List<string> GetTypeOptions() => CommandArgumentHelper.SupportedTypes.Select(type => type.Name).ToList();
    }
}
