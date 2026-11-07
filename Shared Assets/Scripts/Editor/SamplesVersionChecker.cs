using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;

#if UNITY_6000_6_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#else
using Yarn.Unity.Shim;
#endif

#nullable enable

namespace Yarn.Unity.Samples.Editor
{
    public class SamplesVersionChecker : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset? m_VisualTreeAsset = default;

        [NoAutoStaticsCleanup]
        private static System.Version SamplesPackageVersion
        {
            get
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName("dev.yarnspinner.unity.samples");

                // Take the reported package version number, trimming off any -suffix, and splitting by version numbers.
                // Then, concatenate extra zeroes on the end to ensure we have at least 3 values, and then take the first three.
                // Use that to construct a major-minor-patch version number.
                var versionNumbers =
                     (packageInfo.version
                     .Split('-')
                     .FirstOrDefault() ?? "0.0.0")
                     .Split('.')
                     .Select(field => int.TryParse(field, out var result) ? result : 0)
                     .Concat(Enumerable.Repeat(0, 3))
                     .Take(3)
                     .ToList();

                return new System.Version(
                    versionNumbers[0],
                    versionNumbers[1],
                    versionNumbers[2]
                );
            }
        }

        private static System.Version YarnSpinnerVersion =>
             Assembly.GetAssembly(typeof(DialogueRunner)).GetName().Version;

        // [MenuItem("Window/UI Toolkit/SamplesVersionChecker")]
        // public static void ShowExample()
        // {
        //     SamplesVersionChecker wnd = GetWindow<SamplesVersionChecker>();
        //     wnd.titleContent = new GUIContent("SamplesVersionChecker");
        // }

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
            labelFromUXML.style.flexGrow = 1;
            root.Add(labelFromUXML);

            var label = labelFromUXML.Q<Label>("label");
            if (label != null)
            {
                label.text = label.text.Replace("{CURRENT_YS_VERSION}", YarnSpinnerVersion.ToString(3));
                label.text = label.text.Replace("{SAMPLES_VERSION}", SamplesPackageVersion.ToString(3));
            }

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


        private static bool ShouldShowSamples()
        {
            // turns out that headless mode doesn't prevent windows being summoned
            if (Application.isBatchMode)
            {
                return false;
            }

            var currentVersion = YarnSpinnerVersion;

            // Don't warn about this version this session
            const string WarnAboutSessionKey = "yarn_warned_about_session";

            string expectedWarnAboutSessionValue = $"{YarnSpinnerVersion};{SamplesPackageVersion}";

            bool hasShownBefore = SessionState.GetString(WarnAboutSessionKey, "") == expectedWarnAboutSessionValue;

            if (!hasShownBefore && currentVersion < SamplesPackageVersion)
            {
                // Don't warn about this version mismatch again for the remainder of the session
                SessionState.SetString(WarnAboutSessionKey, expectedWarnAboutSessionValue);
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
            SamplesVersionChecker window = GetWindow<SamplesVersionChecker>(true, "Yarn Spinner Package is out of date", true);
            window.minSize = new Vector2(350, 450);
            window.maxSize = window.minSize;
            window.Show();
        }
    }
}