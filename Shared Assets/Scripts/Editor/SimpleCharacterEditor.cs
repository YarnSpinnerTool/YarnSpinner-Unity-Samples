using UnityEditor;
using Yarn.Unity.Editor;

namespace Yarn.Unity.Samples.Editor
{
    [CustomEditor(typeof(SimpleCharacter))]
    public class SimpleCharacterEditor : YarnEditor { }

    [CustomEditor(typeof(SimpleCharacter2D))]
    public class SimpleCharacter2DEditor : YarnEditor { }
}