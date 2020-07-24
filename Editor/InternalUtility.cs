using UnityEditor;
using System.Reflection;

public static class InternalUtility
{
    public static void OpenFileOnSpecificLineAndColumn(string filePath, int line, int column)
    {
        Assembly.GetAssembly(typeof(EditorApplication))
        .GetType("UnityEditor.LogEntries")
        .GetMethod(nameof(OpenFileOnSpecificLineAndColumn), BindingFlags.Static | BindingFlags.Public)
        .Invoke(null, new object[] { filePath, line, column });
    }
}
