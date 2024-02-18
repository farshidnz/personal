using System;
using System.IO;
using System.Reflection;

namespace Cashrewards3API.Tests.Helpers
{
    public static class AssemblyExtensions
    {
        public static string Folder(this Assembly assembly) => Path.GetDirectoryName(assembly.Location);
    }
}
