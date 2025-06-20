#nullable enable

namespace Yarn.Unity.Samples
{
    using UnityEngine;
    using UnityEngine.Events;

    public class AnimationBool : MonoBehaviour
    {
        [SerializeField] private Animator? animator;
        [SerializeField] string parameter = "Visible";

        public void OnValidate()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        public void SetValue(bool value)
        {
            if (animator != null && animator.gameObject.activeSelf)
            {
                animator.SetBool(parameter, value);
            }
        }

        internal bool IsValid
        {
            get
            {
                return animator != null && animator.gameObject.activeSelf && !string.IsNullOrEmpty(parameter);
            }
        }

        internal bool GetValue()
        {
            if (animator != null && animator.gameObject.activeSelf)
            {
                return animator.GetBool(parameter);
            }
            else
            {
                return false;
            }
        }

        [YarnCommand("turn_on")]
        public void TurnOn()
        {
            SetValue(true);
        }

        [YarnCommand("turn_off")]
        public void TurnOff()
        {
            SetValue(false);
        }
    }

#if UNITY_EDITOR
    namespace Editor
    {
        using UnityEditor;
        [CustomEditor(typeof(AnimationBool))]
        public class AnimationBoolEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (target is not AnimationBool animationBool)
                {
                    return;
                }

                using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || !animationBool.IsValid))
                {
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var newValue = EditorGUILayout.Toggle("Value", animationBool.GetValue());
                        if (change.changed)
                        {
                            animationBool.SetValue(newValue);
                        }
                    }
                }
                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Enter play mode to modify the value at runtime.", MessageType.Info);
                }
            }
        }
    }
#endif
}
