using System.Threading;

public static partial class Util
{
    public static void Lock(in object self) => Monitor.Enter(self);
    public static void Unlock(in object self) => Monitor.Exit(self);
    public static void UnlockAndLock(in object self)
    {
        Monitor.Exit(self);
        Monitor.Enter(self);
    }
}