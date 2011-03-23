using System;
using System.IO;

public static class Debug
{
    public static void Write(string name, string value)
    {
        var w = new StreamWriter(@"c:\debug.out", true);
        var n = DateTime.Now;
        w.WriteLine(n.ToShortDateString() + " " + n.ToShortTimeString() + " Name:" + name + " Value:" + value);
        w.Close();
        w.Dispose();
    }
}