/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Yarn.Markup;

#nullable enable

namespace Yarn.Unity.Samples
{
    public class SpriteReplacementMarkerProcessor : ReplacementMarkupHandler
    {
        public LineProviderBehaviour? lineProvider;
        public Color buff = Color.green;
        public Color debuff = Color.red;
        public override ReplacementMarkerResult ProcessReplacementMarker(MarkupAttribute marker, StringBuilder childBuilder, List<MarkupAttribute> childAttributes, string localeCode)
        {

            var start = "<b>[<color=#{0}><sprite=\"effects\" name=\"{1}\">";
            var end = "</color>]</b>";

            string prefix;

            int invisibleCharactersAdded = 0;

            switch (marker.Name.ToLower())
            {
                case "lightning":
                    prefix = string.Format(start, ColorUtility.ToHtmlStringRGB(debuff), "lightning");
                    break;

                case "ice":
                    prefix = string.Format(start, ColorUtility.ToHtmlStringRGB(buff), "water");
                    break;

                case "heart":
                    prefix = string.Format(start, ColorUtility.ToHtmlStringRGB(buff), "heart");
                    break;

                case "fire":
                    prefix = string.Format(start, ColorUtility.ToHtmlStringRGB(debuff), "fire");
                    break;

                default:
                    var diagnostic = new LineParser.MarkupDiagnostic($"was unable to find a matching sprite for {marker.Name}");
                    return new ReplacementMarkerResult(new List<LineParser.MarkupDiagnostic>() { diagnostic }, 0);
            }

            childBuilder.Insert(0, prefix);
            childBuilder.Append(end);

            invisibleCharactersAdded += (prefix.Length - 1) + (end.Length - 1);

            // we now need to move any children attributes down by two characters
            // because we added a [ at the front and sprite
            // so all indices are now off by two and it's up to us to fix that
            for (int i = 0; i < childAttributes.Count; i++)
            {
                childAttributes[i] = childAttributes[i].Shift(2);
            }

            return new ReplacementMarkerResult(invisibleCharactersAdded);
        }

        void Start()
        {
            if (lineProvider == null)
            {
                lineProvider = (LineProviderBehaviour)GameObject.FindAnyObjectByType<DialogueRunner>().LineProvider;
            }
            lineProvider.RegisterMarkerProcessor("lightning", this);
            lineProvider.RegisterMarkerProcessor("ice", this);
            lineProvider.RegisterMarkerProcessor("heart", this);
            lineProvider.RegisterMarkerProcessor("fire", this);
        }
    }
}