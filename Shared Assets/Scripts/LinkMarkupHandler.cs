#nullable enable

namespace Yarn.Unity.Samples
{
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using Yarn.Unity;
    using TMPro;
    using Yarn.Markup;

#if UNITY_EDITOR
    using UnityEditor;
#endif
#if USE_INPUTSYSTEM
    using UnityEngine.InputSystem;
#endif

    public class LinkMarkupHandler : ReplacementMarkupHandler
    {
        [SerializeField] private TextMeshProUGUI? text;
        [SerializeField] private DialogueRunner? runner;

        private int yarnLinkIndex = 0;
        private Dictionary<string, (string, bool)> paths = new Dictionary<string, (string, bool)>();

#if UNITY_EDITOR
        void Awake()
        {
            if (runner != null)
            {
                runner.LineProvider.RegisterMarkerProcessor("link", this);
            }
        }

        public override ReplacementMarkerResult ProcessReplacementMarker(MarkupAttribute marker, StringBuilder childBuilder, List<MarkupAttribute> childAttributes, string localeCode)
        {
            string? value;
            if (!marker.TryGetProperty("link", out value))
            {
                return new ReplacementMarkerResult(new List<LineParser.MarkupDiagnostic>() { new LineParser.MarkupDiagnostic("The link markup has no path!") }, 0);
            }

            bool external = false;
            marker.TryGetProperty("external", out external);

            // if it's an internal path we should verify it is valid
            if (!external)
            {
                var path = AssetDatabase.GUIDToAssetPath(value);
                if (string.IsNullOrEmpty(path))
                {
                    return new ReplacementMarkerResult(new List<LineParser.MarkupDiagnostic>() { new LineParser.MarkupDiagnostic("The link guid is invalid") }, 0);
                }
            }

            var originalLength = childBuilder.Length;
            paths[$"{yarnLinkIndex}"] = (value, external);

            childBuilder.Insert(0, $"<color=#6794D9><b><u>");
            childBuilder.Append("</u></b></color>");

            childBuilder.Insert(0, $"<link=\"{yarnLinkIndex}\">");
            childBuilder.Append("</link>");
            yarnLinkIndex += 1;

            return new ReplacementMarkerResult(childBuilder.Length - originalLength);
        }

        void Update()
        {
            var mousePressed = false;
            Vector2 mousePosition = Vector2.zero;

#if USE_INPUTSYSTEM
            mousePressed = Mouse.current.leftButton.wasReleasedThisFrame;
            mousePosition = Mouse.current.position.value;
#else
        mousePressed = Input.GetMouseButtonDown(0);
        mousePosition = Input.mousePosition;
#endif

            if (mousePressed)
            {
                if (text == null)
                {
                    return;
                }

                var tmpIndex = TMP_TextUtilities.FindIntersectingLink(text, mousePosition, null);
                if (tmpIndex == -1)
                {
                    return;
                }

                var key = text.textInfo.linkInfo[tmpIndex].GetLinkID();

                if (paths.TryGetValue(key, out var element))
                {
                    if (element.Item2)
                    {
                        Application.OpenURL("https://" + element.Item1);
                    }
                    else
                    {
                        var path = AssetDatabase.GUIDToAssetPath(element.Item1);
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(path));
                    }
                }
            }
        }
#endif
    }
}