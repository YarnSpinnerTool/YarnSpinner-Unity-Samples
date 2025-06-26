
namespace Yarn.Unity.Samples.Editor
{
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor.Callbacks;
    using System.IO;
    using Yarn.Unity.Editor;

#nullable enable

    public class DependenciesInstallerTool : EditorWindow
    {
        [InitializeOnLoadMethod]
        public static void AddOpenSceneHook()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (scene, mode) => ShowWindowIfNeeded();
        }

        public static void ShowWindowIfNeeded()
        {
            var dependencyInstaller = FindAnyObjectByType<DependencyInstaller>(FindObjectsInactive.Include);

            if (dependencyInstaller == null)
            {
                return;
            }

            var requirements = dependencyInstaller.requirements;

            if (AreDependenciesReady(requirements) == false)
            {
                Install(dependencyInstaller);
            }

        }

        private static PackageSetupStep? GetSetupStep(DependencyInstaller.DependencyPackage package)
        {
            if (package.PackageName == "com.unity.localization")
            {
#if !USE_UNITY_LOCALIZATION
                return null;
#else
                return new UnityLocalizationSetupStep();
#endif
            }
            else if (package.PackageName == "com.unity.ugui")
            {
#if !USE_TMP
                return null;
#else
                return new TextMeshProSetupStep();
#endif
            }
            else
            {
                return null;
            }
        }

        public static bool AreDependenciesAvailable(IEnumerable<DependencyInstaller.DependencyPackage> dependencies)
        {
            return dependencies.Count() == 0 || dependencies.All(d => CheckAssemblyLoaded(d.AssemblyName));
        }

        public static bool AreDependenciesReady(IEnumerable<DependencyInstaller.DependencyPackage> dependencies)
        {
            return AreDependenciesAvailable(dependencies) && AreDependenciesConfigured(dependencies);
        }

        public static bool AreDependenciesConfigured(IEnumerable<DependencyInstaller.DependencyPackage> dependencies)
        {
            return dependencies.Count() == 0 || dependencies.All(d => CheckPackageConfigured(d));
        }

        public static bool CheckPackageConfigured(DependencyInstaller.DependencyPackage package)
        {
            var setup = GetSetupStep(package);
            if (setup == null)
            {
                return true;
            }
            return !setup.NeedsSetup;
        }

        public static bool CheckAssemblyLoaded(string name)
        {
            return System.AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(assembly => assembly.FullName.Contains(name));
        }

        public static void Install(DependencyInstaller dependencyInstaller)
        {
            DependenciesInstallerTool window = EditorWindow.GetWindow<DependenciesInstallerTool>();
            window.titleContent = new GUIContent("Install Sample Dependencies");
            window.DependencyInstaller = dependencyInstaller;
            window.ShowUtility();
        }

        private DependencyInstaller? _cachedDependencyInstallerInstance;
        const string CurrentDependencyInstallerObjectIDKey = nameof(DependencyInstallerEditor) + "." + nameof(DependencyInstaller);
        private DependencyInstaller? DependencyInstaller
        {
            set
            {
                _cachedDependencyInstallerInstance = value;
                var id = GlobalObjectId.GetGlobalObjectIdSlow(value);
                SessionState.SetString(CurrentDependencyInstallerObjectIDKey, id.ToString());
            }

            get
            {
                if (_cachedDependencyInstallerInstance == null)
                {
                    // Retrieve the global object ID
                    var idString = SessionState.GetString(CurrentDependencyInstallerObjectIDKey, string.Empty);

                    // Attempt to find the DependencyInstaller, possibly from a
                    // previously loaded domain
                    if (!string.IsNullOrEmpty(idString)
                        && GlobalObjectId.TryParse(idString, out var id)
                        && GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) is DependencyInstaller installer)
                    {
                        _cachedDependencyInstallerInstance = installer;
                    }
                }
                return _cachedDependencyInstallerInstance;
            }
        }

        private Vector2 scrollViewPosition = Vector2.zero;

        protected void OnGUI()
        {
            if (this.DependencyInstaller != null)
            {
                scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition);
                DrawDependencyInstallerGUI(isWindow: true, this.DependencyInstaller.requirements);
                EditorGUILayout.EndScrollView();
            }
        }

        internal static void DrawDependencyInstallerGUI(bool isWindow, IEnumerable<DependencyInstaller.DependencyPackage>? dependencies)
        {
            var wrap = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                richText = true
            };

            if (dependencies == null)
            {
                if (isWindow == true)
                {
                    // Close the window - we likely just did a domain reload
                    // after installation
                    var window = GetWindow<DependenciesInstallerTool>();
                    window.Close();
                }
                else
                {
                    // Show an error in the inspector
                    EditorGUILayout.HelpBox($"{nameof(DependencyInstaller)} has null {nameof(dependencies)}", MessageType.Error);
                }
                return;
            }

            if (dependencies.Count() == 0)
            {
                EditorGUILayout.LabelField(
                    "This sample has no dependencies.",
                    wrap
                );
                return;
            }

            if (AreDependenciesReady(dependencies))
            {
                string message;

                if (isWindow)
                {
                    message = "All dependencies for this sample are installed and enabled. You can close this window.";
                }
                else
                {
                    message = "All dependencies for this sample are installed and enabled. You can delete this object.";
                }
                EditorGUILayout.LabelField(message, wrap);
                return;
            }

            EditorGUILayout.LabelField("This sample requires some additional packages, and won't work correctly without them.", wrap);
            EditorGUILayout.Space();

            foreach (var dependency in dependencies)
            {
                if (!CheckAssemblyLoaded(dependency.AssemblyName))
                {
                    EditorGUILayout.LabelField($"This sample requires {dependency.Name}.", wrap);

                    using (new EditorGUI.DisabledGroupScope(PackageInstaller.IsInstallationInProgress))
                    {
                        if (GUILayout.Button($"Install {dependency.Name}"))
                        {
                            PackageInstaller.Add(dependency.PackageName);
                        }
                    }

                    EditorGUILayout.Space();
                }

                var setup = GetSetupStep(dependency);
                if (setup != null && setup.NeedsSetup)
                {
                    EditorGUILayout.LabelField(setup.Description, wrap);
                    if (GUILayout.Button(setup.PerformStepButtonLabel))
                    {
                        setup.RunSetup();
                    }
                }
            }

            if (PackageInstaller.IsInstallationInProgress)
            {
                EditorGUILayout.LabelField("Installation in progress...");
            }
        }
    }

    [CustomEditor(typeof(DependencyInstaller))]
    public class DependencyInstallerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            var dependencyInstaller = target as DependencyInstaller;
            if (dependencyInstaller == null) { return; }
            DependenciesInstallerTool.DrawDependencyInstallerGUI(isWindow: false, dependencyInstaller.requirements);
        }
    }

    public static class PackageInstaller
    {
        static AddRequest? CurrentRequest;

        public static bool IsInstallationInProgress { get; private set; }

        public static void Add(string identifier)
        {
            CurrentRequest = Client.Add(identifier);
            EditorApplication.update += Progress;
            IsInstallationInProgress = true;
        }

        static void Progress()
        {
            if (CurrentRequest == null || CurrentRequest.IsCompleted)
            {
                IsInstallationInProgress = false;
                if (CurrentRequest?.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"Unable to install cinemachine: {CurrentRequest.Error.message}");
                }
                EditorApplication.update -= Progress;
                CurrentRequest = null;
            }
        }
    }

#if USE_TMP
    public class TextMeshProSetupStep : PackageSetupStep
    {
        public override string PerformStepButtonLabel => "Import TMP Essential Resources";

        public override string Description => "TextMeshPro Essential Resources needs to be installed.";

        public override bool NeedsSetup => Resources.Load<TMPro.TMP_Settings>("TMP Settings") == null;

        public override void RunSetup()
        {
            TMPro.TMP_PackageResourceImporter.ImportResources(
                importEssentials: true,
                importExamples: false,
                interactive: false
            );
        }
    }
#endif
}