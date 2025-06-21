using System.Diagnostics;

partial class Util
{
    public static StackTrace GetStackTrace(in int skipFrames = 1)
    {
        // skipFrames = 1 pour ne pas inclure cet appel à GetStackTrace dans la stack
        return new StackTrace(skipFrames, true);
    }
}