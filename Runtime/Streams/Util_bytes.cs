using System.Text;

public static partial class Util
{
    public static string LogBytes(this byte[] buffer, in int start, in int end)
    {
        StringBuilder log = new($"{end - start} bytes: ");
        lock (buffer)
            for (int i = start; i < end; i++)
                log.Append($"| {buffer[i],3} ");
        return log.ToString();
    }
}