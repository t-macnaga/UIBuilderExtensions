using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Text;

public class EditorWindowCodeBuilder
{
    MonoScript monoScript;
    string assetPath;
    string windowEventFilePath;
    public EditorWindowCodeBuilder(string assetPath)
    {
        this.assetPath = assetPath;
        LoadMonoScript();
    }

    public void Save(string className)
    {
        LoadMonoScript();
        if (monoScript == null)
        {
            var scriptCode = EditorWindowCodeGen.GenerateEditorWindowCode(className);
            // var eventScriptCode = EditorWindowCodeGen.GenerateEditorWindowEventCode(className);
            SaveWindowScript(windowEventFilePath, scriptCode);
        }
    }

    void LoadMonoScript()
    {
        windowEventFilePath = assetPath.Replace(".uxml", ".cs");
        monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(windowEventFilePath);
    }

    public void StubCode(CodeGenDescription desc, VisualElement ve)
    {
        LoadMonoScript();
        if (desc.Type == typeof(Button))
        {
            if (!ContainsCallback(desc.CallbackMethodName))
            {
                var stubCode = EditorWindowCodeGen.GetMethodCode(
                    methodName: desc.CallbackMethodName,
                    debugLogMessage: $"{ve.name} Clicked");
                StubCodeAndSave(windowEventFilePath, monoScript.text, stubCode);
            }
        }
        else
        {
            if (!ContainsCallback(desc.CallbackMethodName))
            {
                var stubCode = EditorWindowCodeGen.GetEventMethodCode(
                    methodName: desc.CallbackMethodName,
                    eventTypeName: desc.EventTypeName,
                    debugLogMessage: $"On {desc.EventTypeName}"
                );
                StubCodeAndSave(windowEventFilePath, monoScript.text, stubCode);
            }
        }
    }

    public bool ContainsCallback(string methodName)
    {
        if (monoScript == null) { return false; }
        return monoScript.text.Contains(methodName);
    }

    void StubCodeAndSave(string assetPath, string allScript, string stubCode)
    {
        var ioPath = Application.dataPath.Replace("Assets", "") + assetPath;
        var lastIndex = allScript.LastIndexOf("}");
        var text = allScript.Substring(0, lastIndex);

        var sb = new StringBuilder();
        sb.Append(text);
        sb.AppendLine("");
        var line = sb.ToString().Split('\n').Length;
        sb.AppendLine(stubCode);
        sb.AppendLine("}");

        var script = sb.ToString();
        SaveWindowScript(assetPath, script);
        InternalUtility.OpenFileOnSpecificLineAndColumn(ioPath, line, 0);
    }

    void SaveWindowScript(string assetPath, string script)
    {
        var ioPath = Application.dataPath.Replace("Assets", "") + assetPath;
        System.IO.File.WriteAllText(ioPath, script);
        AssetDatabase.Refresh();
    }
}