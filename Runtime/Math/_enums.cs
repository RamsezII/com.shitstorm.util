using System;

namespace _UTIL_
{
    public enum DIRS_ENUM : byte
    {
        Top,
        Right,
        Down,
        Left,
    }

    [Flags]
    public enum DIRS_FLAGS : byte
    {
        Top = 1 << DIRS_ENUM.Top,
        Right = 1 << DIRS_ENUM.Right,
        Down = 1 << DIRS_ENUM.Down,
        Left = 1 << DIRS_ENUM.Left,
    }
}