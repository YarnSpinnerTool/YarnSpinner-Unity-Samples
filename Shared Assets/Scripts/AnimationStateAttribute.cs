using UnityEngine;

using System;

namespace Yarn.Unity.Samples
{

    [AttributeUsage(AttributeTargets.Field)]
    public class AnimationStateAttribute : PropertyAttribute
    {
        public AnimationStateAttribute(string animatorPropertyName)
        {
            this.AnimatorPropertyName = animatorPropertyName;
        }

        public string AnimatorPropertyName { get; }
    }
}