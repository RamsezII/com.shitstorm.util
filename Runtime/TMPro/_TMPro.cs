using TMPro;
using UnityEngine;

partial class Util
{
    public static void AutoSize(this TextMeshProUGUI tmp) => tmp.rectTransform.sizeDelta = new Vector2(tmp.preferredWidth, tmp.preferredHeight);

    public static TMP_Dropdown.OptionData CurrentData(this TMP_Dropdown dropdown) => dropdown.options[dropdown.value];
    public static string GetSelectedName(this TMP_Dropdown dropdown) => CurrentData(dropdown).text;

    public static bool TryCurrentData(this TMP_Dropdown dropdown, out TMP_Dropdown.OptionData output)
    {
        if (dropdown.options.Count == 0)
        {
            output = null;
            return false;
        }
        output = dropdown.options[dropdown.value];
        return true;
    }

    public static bool TryGetSelectedValue(this TMP_Dropdown dropdown, out string output)
    {
        if (TryCurrentData(dropdown, out var data))
        {
            output = data.text;
            return true;
        }
        output = null;
        return false;
    }
}