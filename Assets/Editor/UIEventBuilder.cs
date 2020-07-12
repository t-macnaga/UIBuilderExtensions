using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Reflection;
using System.Text;

/// <summary>
/// UXMLレイアウトに対応したエディターウィンドウのスクリプトの生成とボタンクリックイベントへの
/// メソッド追加をサポートするウィンドウです。
/// </summary>
public class UIEventBuilder : EditorWindow
{
    VisualTreeAsset treeAsset;
    ObjectField treeAssetField;
    VisualElement container;
    string ClassName => treeAssetField.value.name;

    [MenuItem("Window/UI/UI Event Builder")]
    static void Init()
    {
        GetWindow<UIEventBuilder>()
        .rootVisualElement.schedule.Execute(_ =>
            GetWindow<UIEventBuilderInspector>(typeof(UIEventBuilder))
        ).StartingIn(1000);
    }

    void OnEnable()
    {
        Selection.selectionChanged += SelectionChange;
        treeAssetField = new ObjectField("uxml");
        treeAssetField.objectType = typeof(VisualTreeAsset);
        rootVisualElement.Add(treeAssetField);
    }

    void OnDisable()
    {
        Selection.selectionChanged -= SelectionChange;
    }

    void SelectionChange()
    {
        if (Selection.activeObject is VisualTreeAsset treeAsset)
        {
            treeAssetField.value = treeAsset;
            BuildTree();
        }
    }

    void BuildTree()
    {
        if (container != null)
        {
            rootVisualElement.Remove(container);
        }
        container = (treeAssetField.value as VisualTreeAsset).CloneTree();
        container.Query<Button>().ForEach(button => AddContextualMenu(button));
        rootVisualElement.Add(container);
    }

    void AddContextualMenu(Button button)
    {
        button.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Add OnClick",
                (a) => MenuAction(a, button),
                (a) => DropdownMenuAction.Status.Normal);
        }));
    }

    void MenuAction(DropdownMenuAction a, Button button)
    {
        var path = AssetDatabase.GetAssetPath(treeAssetField.value);
        var treeAssetPath = path;
        var windowFilePath = path.Replace(".uxml", ".cs");
        path = path.Replace(".uxml", ".cs");
        var ioPath = Application.dataPath.Replace("Assets", "") + path;
        Debug.Log(path);
        var windowScript = AssetDatabase.LoadAssetAtPath<MonoScript>(windowFilePath);
        if (windowScript == null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine("");
            sb.AppendLine($"public partial class {ClassName} : EditorWindowBase");
            sb.AppendLine("{");
            sb.AppendLine($"    protected override string TreeAssetPath => \"{treeAssetPath}\";");
            sb.AppendLine("");
            sb.AppendLine($"    [MenuItem(\"Window/{ClassName}\")]");
            sb.AppendLine($"    static void Init()");
            sb.AppendLine("    {");
            sb.AppendLine($"        GetWindow<{ClassName}>();");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    protected override void OnEnable()");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnEnable();");
            sb.AppendLine("        // insert your code here.");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            System.IO.File.WriteAllText(windowFilePath, sb.ToString());
            AssetDatabase.Refresh();
        }

        windowScript = AssetDatabase.LoadAssetAtPath<MonoScript>(windowFilePath);
        if (windowScript.text.Contains($"OnClick_{button.name}"))
        {
            Debug.Log("method exists.");
        }
        else
        {
            Debug.Log("method not exists.");
            var lastIndex = windowScript.text.LastIndexOf("}");
            Debug.Log($"last:{lastIndex}");
            var text = windowScript.text.Substring(0, lastIndex);

            var sb = new StringBuilder();
            sb.Append(text);
            sb.AppendLine("");
            var line = sb.ToString().Split('\n').Length;

            StubMethodCode(sb, $"OnClick_{button.name}", $"{button.name} Clicked");
            sb.AppendLine("}");
            Debug.Log(sb.ToString());
            System.IO.File.WriteAllText(ioPath, sb.ToString());
            OpenFileOnSpecificLineAndColumn(ioPath, line, 0);
            AssetDatabase.Refresh();
        }
    }

    void StubMethodCode(StringBuilder sb, string methodName, string debugLogMessage)
    {
        sb.AppendLine($"    void {methodName}()");
        sb.AppendLine("    {");
        sb.AppendLine($"        Debug.Log(\"{debugLogMessage}\");");
        sb.AppendLine("    }");
    }

    static void OpenFileOnSpecificLineAndColumn(string filePath, int line, int column)
    {
        Assembly.GetAssembly(typeof(EditorApplication))
        .GetType("UnityEditor.LogEntries")
        .GetMethod(nameof(OpenFileOnSpecificLineAndColumn), BindingFlags.Static | BindingFlags.Public)
        .Invoke(null, new object[] { filePath, line, column });
    }
}