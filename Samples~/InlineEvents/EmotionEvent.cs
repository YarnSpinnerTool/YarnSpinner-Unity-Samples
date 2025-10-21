/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;


#if USE_TMP
using TMPro;
#else
using TMP_Text = Yarn.Unity.TMPShim;
#endif

#nullable enable

namespace Yarn.Unity.Samples
{
    public class EmotionEvent : ActionMarkupHandler
    {
        private SimpleCharacter? target;
        Dictionary<int, string> emotions = new();

        public override void OnLineDisplayComplete()
        {
            return;
        }

        public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
        {
            return;
        }

        public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
        {
            // grab the character attribute
            // grab the SimpleCharacterAnimation that matches that name
            if (!line.TryGetAttributeWithName("character", out var character))
            {
                Debug.LogWarning("line has no character");
                return;
            }
            // we need the name of the character so we can find them in the scene
            if (!character.Properties.TryGetValue("name", out var name))
            {
                Debug.LogWarning("character has no name");
                return;
            }

            var emoter = GameObject.Find(name.StringValue);
            if (emoter == null)
            {
                Debug.LogWarning($"scene has no one called {name.StringValue}");
                return;
            }

            target = emoter.GetComponent<SimpleCharacter>();
            if (target == null)
            {
                Debug.Log($"{name.StringValue} is not a SimpleCharacterAnimation");
                return;
            }

            emotions = new();

            foreach (var attribute in line.Attributes)
            {
                if (attribute.Name != "emotion")
                {
                    continue;
                }

                if (!attribute.TryGetProperty("emotion", out string? emotionKey))
                {
                    continue;
                }
                emotions[attribute.Position] = emotionKey;
            }
        }

        public override async YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                return;
            }

            if (emotions.TryGetValue(currentCharacterIndex, out var emotion))
            {
                target.SetFacialExpression(emotion);
                if (target.TryGetComponent<CharacterAppearance>(out var appearance))
                {
                    if (emotion == "angry")
                    {
                        // make them angry looking
                        var fadeColour = new Color(0.943f, 0.2315789f, 0);
                        var baseColour = new Color(0.8867924f, 0.5030531f, 0.489409f);
                        appearance.SetAppearance(baseColour, fadeColour);

                        // after becoming angry we wait a brief moment
                        // just to make it clear
                        await YarnTask.Delay(300, cancellationToken);
                    }
                    else
                    {
                        // reset them back
                        var fadeColour = new Color(0.4039216f, 0.5803922f, 0.8509804f);
                        var baseColour = new Color(0.3411765f, 0.3372549f, 0.7411765f);
                        appearance.SetAppearance(baseColour, fadeColour);
                    }
                }
            }
        }

        public override void OnLineWillDismiss()
        {
            return;
        }
    }
}