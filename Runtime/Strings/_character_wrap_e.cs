#if UNITY_EDITOR
using TMPro;
using UnityEditor;

static partial class Util_zspaces_e
{
    const string button_prefixe = "CONTEXT/" + nameof(TMP_InputField) + "/";

    //----------------------------------------------------------------------------------------------------------

    [MenuItem(button_prefixe + nameof(AllSpaceToNowrap))]
    static void AllSpaceToNowrap(MenuCommand command)
    {
        var inputfield = (TMP_InputField)command.context;
        string new_text = inputfield.text.AllSpaceToNowrap();
        inputfield.text = new_text;
    }

    [MenuItem(button_prefixe + nameof(AllNowrapToSpace))]
    static void AllNowrapToSpace(MenuCommand command)
    {
        var inputfield = (TMP_InputField)command.context;
        string new_text = inputfield.text.AllNowrapToSpace();
        inputfield.text = new_text;
    }
}
#endif