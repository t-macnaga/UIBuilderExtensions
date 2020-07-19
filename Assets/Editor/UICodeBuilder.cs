using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

public class UICodeBuilder : EditorWindow
{
    EditorWindowCodeBuilder editorWindowCodeBuilder;
    ScrollView scrollView;
    BuilderWindowInternal internalBuilder;
    string ClassName => internalBuilder.TreeAsset.name;
    string Path => AssetDatabase.GetAssetPath(internalBuilder.TreeAsset);
    VisualElement CurrentSelection => internalBuilder.CurrentSelection;

    [MenuItem("Window/UI/UI Code Builder")]
    static void Init()
    {
        GetWindow<UICodeBuilder>();
    }

    void OnEnable()
    {
        internalBuilder = new BuilderWindowInternal();
        internalBuilder.OnSelectionChanged = OnSelectVisualElement;
        rootVisualElement.schedule.Execute(_ => internalBuilder.Update()).Every(1000);
    }

    void OnSelectVisualElement(VisualElement e)
    {
        editorWindowCodeBuilder = new EditorWindowCodeBuilder(Path);
        BuildEventView(e);
    }

    void BuildEventView(VisualElement e)
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
        }
        else if (e is INotifyValueChanged<string>)
        {
            Debug.Log("string value changed.");
            // types = TypeCache.GetTypesDerivedFrom<INotifyValueChanged<string>>();
            types = new List<CodeGenDescription> { new CodeGenDescription(e) };
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
        // else
        // {
        //     types = TypeCache.GetTypesDerivedFrom<EventBase>()
        //     .Where(x => !x.IsAbstract)
        //     .Select(x => new CodeGenDescription(x))// Type = x, EventTypeName = x.Name })
        //     .OrderBy(x => x.Type.Name)
        //     .ToList();
        // }
        foreach (var type in
            TypeCache.GetTypesDerivedFrom<EventBase>()
            .Where(x => !x.IsAbstract)
            .Select(x => new CodeGenDescription(x, e.name))
            .OrderBy(x => x.Type.Name))
        {
            types.Add(type);
        }
        foreach (var type in types)
        {
            var field = new TextField(type.DisplayEventTypeName);
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
            scrollView.Add(field);
        }
        rootVisualElement.Add(scrollView);
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
            Debug.Log($"Not Contains {stubCode}");
            var script = EditorWindowCodeGen.GetStubbedCodeAtSecondLastIndexOfBlacket(registerWindowScript.text, stubCode);
            SaveWindowScript(windowRegisterFilePath, script);
        }

        editorWindowCodeBuilder.StubCode(desc, CurrentSelection);
    }
}