#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static partial class Util_e_OLD
{
    [MenuItem("CONTEXT/" + nameof(RectTransform) + "/" + nameof(_EDITOR_) + "/" + nameof(FillParent))]
    static void FillParent(MenuCommand command) => ((RectTransform)command.context).FillParent();
}
#endif