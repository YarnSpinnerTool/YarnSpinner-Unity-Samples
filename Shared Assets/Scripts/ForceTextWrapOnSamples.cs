namespace Yarn.Unity.Samples
{
    using UnityEngine;
    using TMPro;

    public class ForceTextWrapOnSamples : MonoBehaviour
    {
        public void OnValidate()
        {
            var allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
            foreach (var text in allTexts)
            {
                text.SetTextWrapping(true);
            }
        }
    }
    
#if UNITY_EDITOR
    namespace Editor
    {
        using UnityEditor;

        [CustomEditor(typeof(ForceTextWrapOnSamples))]
        public class ForceTextWrapOnSamplesEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUILayout.HelpBox("This object forces text wrapping on all Text Mesh Pro objects in the samples.\nThis fixes an issue in how Unity serialises word wrap differently in 2022 and 6 and won't be necessary in your own games.", MessageType.Info);
            }
        }
    }
#endif
}