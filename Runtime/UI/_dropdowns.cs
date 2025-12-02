using UnityEngine.UI;

partial class Util
{
    public static string Get_ItemName_From_DropdownToggle(this Toggle toggle)
    {
        string name = toggle.name;
        int index = name.IndexOf(':');
        if (index == -1)
            return name;
        return name[(index + 2)..];
    }
}