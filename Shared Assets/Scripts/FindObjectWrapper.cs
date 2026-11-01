using UnityEngine;

namespace Yarn.Unity.Samples
{
    public static class FindObjectWrapper
    {
        public static T[] FindObjectsByType<T>(FindObjectsInactive inactives = FindObjectsInactive.Exclude) where T : Object
        {
    #if UNITY_6000_4_OR_NEWER
            return GameObject.FindObjectsByType<T>(inactives);
    #else
            return GameObject.FindObjectsByType<T>(inactives, FindObjectsSortMode.None);
    #endif
        }
    }
}