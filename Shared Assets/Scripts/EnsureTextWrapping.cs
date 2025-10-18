namespace Yarn.Unity.Samples
{
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

}