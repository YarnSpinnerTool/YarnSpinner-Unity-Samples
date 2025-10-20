namespace Yarn.Unity.Samples
{

#if !USE_TMP
using TMP_Text = Yarn.Unity.TMPShim;
#endif

#if USE_TMP
    public static class TMPTextWrappingExtensions
    {
        public static void SetTextWrapping(this TMPro.TMP_Text text, bool enabled)
        {
    #if UNITY_6000_0_OR_NEWER
            text.textWrappingMode = TMPro.TextWrappingModes.Normal;
    #else
            text.enableWordWrapping = true;
    #endif
        }
    }
#else
    public static class TMPTextWrappingExtensions
    {
        public static void SetTextWrapping(this TMP_Text text, bool enabled)
        {
            Debug.LogWarning("This method has no functionality and shouldn't be called with TMP is not installed");
        }
    }
#endif

}