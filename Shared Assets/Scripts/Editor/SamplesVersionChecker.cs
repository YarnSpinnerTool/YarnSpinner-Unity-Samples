using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;

#nullable enable

namespace Yarn.Unity.Samples.Editor
{
    public class SamplesVersionChecker : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset? m_VisualTreeAsset = default;

        [MenuItem("Window/UI Toolkit/SamplesVersionChecker")]
        public static void ShowExample()
        {
            SamplesVersionChecker wnd = GetWindow<SamplesVersionChecker>();
            wnd.titleContent = new GUIContent("SamplesVersionChecker");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            if (m_VisualTreeAsset == null)
            {
                Debug.LogError($"Unable to create UXML tree, {nameof(m_VisualTreeAsset)} is null.");
                return;
            }

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            var button = labelFromUXML.Q<Button>("close");
            if (button == null)
            {
                Debug.LogError("Unable to locate the button inside the UXML");
                return;
            }
            button.clicked += () =>
            {
                this.Close();
            };
        }

        private static System.Version supportedVersion = new(3,3);
        private static bool ShouldShowSamples()
        {
            // turns out that headless mode doesn't prevent windows being summoned
            if (Application.isBatchMode)
            {
                return false;
            }

            var currentVersion = Assembly.GetAssembly(typeof(DialogueRunner)).GetName().Version;
            if (currentVersion < supportedVersion)
            {
                return true;
            }

            return false;
        }
        [InitializeOnLoadMethod]
        internal static void TryShowingWindowOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                // do a check to see if it is time to show
                if (ShouldShowSamples())
                {
                    ShowAbout();
                }
            };
        }
        public static void ShowAbout()
        {
            SamplesVersionChecker window = GetWindow<SamplesVersionChecker>(true, "Old Yarn Spinner", true);
            window.minSize = new Vector2(350, 450);
            window.maxSize = window.minSize;
            window.Show();
        }
    }
}