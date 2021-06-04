using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;


namespace System.Collections
{
    public class CompareNaural : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return CompareExtension.StrCmpLogicalW(x, y);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CompareExtension
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string x, string y);
    }
}
