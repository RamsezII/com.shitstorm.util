using System;

partial class Util
{
    public delegate void Action_ref<T>(ref T a);
    public delegate void Action_ref<T, U>(ref T a, ref U b);
    public delegate void Action_ref<T, U, V>(ref T a, ref U b, ref V c);

    public static void AddActionOnce(ref Action instance, in Action to_add)
    {
        instance -= to_add;
        instance += to_add;
    }

    public static void AddAction<T>(ref Action<T> instance, in Action<T> to_add)
    {
        instance -= to_add;
        instance += to_add;
    }
}