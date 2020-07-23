using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

[InitializeOnLoad]
public class BuilderExtensions
{
    static EditorWindowCodeBuilder editorWindowCodeBuilder;
    static ScrollView scrollView;
    static BuilderWindowInternal internalBuilder;
    static VisualElement builderInspector;
    static VisualElement BuilderInspector
    {
        get
        {
            if (builderInspector == null)
            {
                builderInspector = internalBuilder.builderWindow.rootVisualElement.Q("inspector");
            }
            return builderInspector;
        }
    }
    static string Path => AssetDatabase.GetAssetPath(internalBuilder.TreeAsset);
    static string ClassName => System.IO.Path.GetFileNameWithoutExtension(Path);
    static VisualElement CurrentSelection => internalBuilder.CurrentSelection;

    static BuilderExtensions()
    {
        internalBuilder = new BuilderWindowInternal();
        internalBuilder.OnSelectionChanged = OnSelectVisualElement;
        EditorApplication.update += internalBuilder.Update;
    }

    static void OnSelectVisualElement(VisualElement e)
    {
        editorWindowCodeBuilder = new EditorWindowCodeBuilder(Path);
        BuildEventView(e);
    }

    static void BuildEventView(VisualElement e)
    {
        if (scrollView == null)
        {
            scrollView = new ScrollView();
        }
        scrollView.Clear();
        if (e == null) { return; }

        scrollView.SetEnabled(!string.IsNullOrEmpty(e.name));
        var types = new List<CodeGenDescription>();
        if (e is Button)
        {
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
            AddDescGroup(types, scrollView, "Button Event", true);
        }
        else if (e is INotifyValueChanged<string>)
        {
            Debug.Log("string value changed.");
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
            AddDescGroup(types, scrollView, "Value Change Event", true);
        }
        else if (e is INotifyValueChanged<bool>)
        {
            Debug.Log("bool value changed.");
            // types = TypeCache.GetTypesDerivedFrom<INotifyValueChanged<string>>();
            // types = new List<string>
            // {
            //     nameof(ChangeEvent<bool>)// "ChangeEvent<string>"
            // };
        }
        types.Clear();
        foreach (var type in
            TypeCache.GetTypesDerivedFrom<EventBase>()
            .Where(x => !x.IsAbstract)
            .Select(x => new CodeGenDescription(x, e.name))
            .OrderBy(x => x.Type.Name))
        {
            types.Add(type);
        }
        AddDescGroup(types, scrollView, "All Events", false);
        // rootVisualElement.Add(scrollView);
        BuilderInspector.Add(scrollView);
    }

    static void AddDescGroup(List<CodeGenDescription> descList, VisualElement elementAddTo, string groupLabel, bool open)
    {
        var group = new Foldout();
        var savedColor = GUI.backgroundColor;
        group.style.backgroundColor = Color.gray;
        group.value = open;
        group.text = groupLabel;
        foreach (var type in descList)
        {
            var field = new TextField(type.DisplayEventTypeName);
            field.style.backgroundColor = savedColor;
            field.isReadOnly = true;
            field.value = editorWindowCodeBuilder.ContainsCallback(type.CallbackMethodName) ?
            type.CallbackMethodName : string.Empty;

            var doubleClickManipulator = new MouseDoubleClickManipulator()
            .RegisterDoubleClick(() =>
            {
                Debug.Log($"Double Button Clicked.");
                field.value = type.CallbackMethodName;
                GenerateCode(type);
            });
            field.AddManipulator(doubleClickManipulator);
            group.Add(field);
        }
        elementAddTo.Add(group);
    }

    static void SaveWindowScript(string assetPath, string script)
    {
        var ioPath = Application.dataPath.Replace("Assets", "") + assetPath;
        System.IO.File.WriteAllText(ioPath, script);
        AssetDatabase.Refresh();
    }

    static void GenerateCode(CodeGenDescription desc)
    {
        var treeAssetPath = Path;
        editorWindowCodeBuilder.Save(ClassName);

        var windowRegisterFilePath = Path.Replace(".uxml", ".RegisterCallbacks.cs");
        var registerWindowScript = AssetDatabase.LoadAssetAtPath<MonoScript>(windowRegisterFilePath);
        if (registerWindowScript == null)
        {
            var registerScriptCode = EditorWindowCodeGen.GenerateEditorWindowRegisterCode(ClassName, treeAssetPath);
            SaveWindowScript(windowRegisterFilePath, registerScriptCode);
        }

        registerWindowScript = AssetDatabase.LoadAssetAtPath<MonoScript>(windowRegisterFilePath);
        var stubCode = string.Empty;
        if (desc.Type == typeof(Button))
        {
            stubCode = EditorWindowCodeGen.GetRegisterClickedCode(CurrentSelection.name, desc.CallbackMethodName);
        }
        else
        {
            stubCode = EditorWindowCodeGen.GetRegisterCallbackCode(desc, CurrentSelection.name);
        }
        if (!registerWindowScript.text.Contains(stubCode))
        {
            Debug.Log($"Not Contains {stubCode}");
            var script = EditorWindowCodeGen.GetStubbedCodeAtSecondLastIndexOfBlacket(registerWindowScript.text, stubCode);
            SaveWindowScript(windowRegisterFilePath, script);
        }

        editorWindowCodeBuilder.StubCode(desc, CurrentSelection);
    }
}