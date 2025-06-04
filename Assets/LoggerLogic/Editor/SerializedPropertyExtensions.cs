#if UNITY_EDITOR
using UnityEditor;

namespace Azen.Logger
{
    public static class SerializedPropertyExtensions
    {
        public static int IndexOf(this SerializedProperty arrayProp, SerializedProperty element)
        {
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                if (SerializedProperty.EqualContents(arrayProp.GetArrayElementAtIndex(i), element))
                    return i;
            }
            return -1;
        }
    }
}
#endif