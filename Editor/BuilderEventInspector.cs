using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

internal class BuilderEventInspector
{
    static readonly string BuilderEventInspectorName = "BuilderEventInspector";
    EditorWindowCodeBuilder editorWindowCodeBuilder;
    ScrollView scrollView;
    BuilderWindowInternal internalBuilder;
    string Path => AssetDatabase.GetAssetPath(internalBuilder.TreeAsset);
    string ClassName => System.IO.Path.GetFileNameWithoutExtension(Path);
    VisualElement CurrentSelection => internalBuilder.CurrentSelection;

    internal BuilderEventInspector(BuilderWindowInternal internalBuilder)
    {
        this.internalBuilder = internalBuilder;
    }

    public void OnSelectVisualElement(VisualElement e)
    {
        editorWindowCodeBuilder = new EditorWindowCodeBuilder(Path);
        BuildEventView(e);
    }

    void BuildEventView(VisualElement e)
    {
        if (scrollView == null)
        {
            scrollView = new ScrollView();
            scrollView.name = BuilderEventInspectorName;
        }
        scrollView.Clear();
        if (e == null) { return; }
        var types = new List<CodeGenDescription>();
        if (e is Button)
        {
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
        }
        else if (e is INotifyValueChanged<string>)
        {
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
        }
        else if (e is INotifyValueChanged<bool>)
        {
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
        }
        else if (e is INotifyValueChanged<int>)
        {
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
        }
        else if (e is INotifyValueChanged<float>)
        {
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
        }
        foreach (var type in
            TypeCache.GetTypesDerivedFrom<EventBase>()
            .Where(x => !x.IsAbstract && !x.IsGenericType)
            .Select(x => new CodeGenDescription(x, e.name))
            .OrderBy(x => x.Type.Name))
        {
            types.Add(type);
        }
        AddDescGroup(types, scrollView, "Events", false, e);
        var view = internalBuilder.BuilderInspector.Q<ScrollView>(BuilderEventInspectorName);
        if (view == null)
        {
            internalBuilder.BuilderInspector.Add(scrollView);
        }
    }

    void AddDescGroup(
        List<CodeGenDescription> descList,
        VisualElement elementAddTo,
        string groupLabel,
        bool open,
        VisualElement targetElement)
    {
        var group = new Foldout();
        var savedColor = GUI.backgroundColor;
        group.style.backgroundColor = Color.gray;
        group.value = open;
        group.text = groupLabel;

        var eventFields = new VisualElement();
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
                field.value = type.CallbackMethodName;
                GenerateCode(type);
            });
            field.AddManipulator(doubleClickManipulator);
            eventFields.Add(field);
        }
        group.Add(eventFields);
        var warnLabel = new Label("Set the VisualElement name.");
        warnLabel.visible = false;
        group.Add(warnLabel);

        group.schedule.Execute(_ =>
        {
            var isEnabled = !string.IsNullOrEmpty(targetElement.name);
            scrollView.SetEnabled(isEnabled);
            eventFields.visible = isEnabled;
            warnLabel.visible = !isEnabled;

        }).Every(100);

        elementAddTo.Add(group);
    }

    void SaveWindowScript(string assetPath, string script)
    {
        var ioPath = Application.dataPath.Replace("Assets", "") + assetPath;
        System.IO.File.WriteAllText(ioPath, script);
        AssetDatabase.Refresh();
    }

    void GenerateCode(CodeGenDescription desc)
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
            Debug.Log($"Add Method {stubCode}");
            var script = EditorWindowCodeGen.GetStubbedCodeAtSecondLastIndexOfBlacket(registerWindowScript.text, stubCode);
            SaveWindowScript(windowRegisterFilePath, script);
        }

        editorWindowCodeBuilder.StubCode(desc, CurrentSelection);
    }
}