using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Animations;

namespace Yarn.Unity.Samples.Editor
{

    [CustomPropertyDrawer(typeof(AnimationStateAttribute))]
    public class AnimationStatePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (property.propertyType != SerializedPropertyType.String)
                {
                    EditorGUI.HelpBox(position, "Invalid property type " + property.propertyType, MessageType.Error);
                    return;
                }

                var attribute = this.attribute as AnimationStateAttribute
                    ?? throw new System.InvalidOperationException($"Target is not {nameof(AnimationStateAttribute)}");
                var animator = property.serializedObject.FindProperty(attribute.AnimatorPropertyName)?.objectReferenceValue as Animator;

                if (animator == null)
                {
                    // No known animator to get states from, so just show
                    // the text field
                    var text = EditorGUI.TextField(position, label, property.stringValue);
                    property.stringValue = text;
                    return;
                }

                AnimatorController controller;

                if (animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
                {
                    controller = overrideController.runtimeAnimatorController as AnimatorController;
                }
                else
                {
                    controller = animator.runtimeAnimatorController as AnimatorController;
                }

                if (controller == null)
                {
                    using (new EditorGUI.DisabledScope())
                    {
                        EditorGUI.LabelField(position, label, new GUIContent("no controller found!"));
                    }
                    return;

                }

                var layers = new List<(string DisplayName, string StateName)>();
                bool includeLayerNames = controller.layers.Length > 1;
                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    ChildAnimatorState[] animStates = layer.stateMachine.states;
                    foreach (ChildAnimatorState childState in animStates)
                    {
                        string displayName = includeLayerNames
                            ? (layer.name + "/" + childState.state.name)
                            : childState.state.name;

                        layers.Add((DisplayName: displayName, StateName: childState.state.name));
                    }
                }


                var selectedIndex = layers.FindIndex(l => l.StateName == property.stringValue);
                var parameterContent = layers.Select(l => new GUIContent(l.DisplayName)).ToArray();

                selectedIndex = EditorGUI.Popup(position, label, selectedIndex, parameterContent);

                if (selectedIndex >= 0 && selectedIndex < layers.Count)
                {
                    property.stringValue = layers[selectedIndex].StateName;
                }
            }
        }
    }
}