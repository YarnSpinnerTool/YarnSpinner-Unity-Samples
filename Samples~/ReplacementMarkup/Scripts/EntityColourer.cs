/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

#nullable enable

namespace Yarn.Unity.Samples
{
    [System.Serializable]
    public struct EntityMap
    {
        public string name;
        public UnityEngine.Color colour;
    }

    // also make a basic replacer that automatically replaces [i] and [b] with <i> and <b>
    public class EntityColourer : Yarn.Unity.ReplacementMarkupHandler
    {
        public EntityMap[] entities = System.Array.Empty<EntityMap>();
        public override ReplacementMarkerResult ProcessReplacementMarker(MarkupAttribute marker, StringBuilder childBuilder, List<MarkupAttribute> childAttributes, string localeCode)
        {
            // this works in one of two ways
            // if we have a name string property we use that
            // otherwise read the contents of the text within the marker

            string nameText = string.Empty;
            if (marker.TryGetProperty("name", out string? value))
            {
                nameText = value.ToLower();
            }

            if (nameText == string.Empty)
            {
                nameText = childBuilder.ToString().ToLower();
            }

            int invisibleCharactersAdded = 0;

            foreach (var entity in entities)
            {
                if (entity.name.ToLower() == nameText)
                {
                    var prefix = $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(entity.colour)}><b>";
                    var suffix = "</b></color>";
                    childBuilder.Insert(0, prefix);
                    childBuilder.Append(suffix);
                    invisibleCharactersAdded += prefix.Length + suffix.Length;
                }
            }

            return new ReplacementMarkerResult(invisibleCharactersAdded);
        }

        protected void Start()
        {
            var lineProvider = (LineProviderBehaviour)GameObject.FindAnyObjectByType<DialogueRunner>().LineProvider;
            lineProvider.RegisterMarkerProcessor("name", this);
        }
    }
}