using _UTIL_;
using UnityEngine;

namespace _UTIL_
{
    public enum LayerIndexes : byte
    {
        Default = 0,
        TransfarentFX = 1,
        IgnoreRaycast = 2,

        Water = 4,
        UI = 5,
    }
}

partial class Util
{
    public static readonly LayerMask
        layer_default = 1 << (int)LayerIndexes.Default,
        layer_transparent = 1 << (int)LayerIndexes.TransfarentFX,
        layer_ignore = 1 << (int)LayerIndexes.IgnoreRaycast,

        layer_water = 1 << (int)LayerIndexes.Water,
        layer_ui = 1 << (int)LayerIndexes.UI;
}