﻿using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

public class CodeGenDescription
{
    public System.Type Type;
    public string CallbackMethodName; // TypeがChangeEvent<string>だと文字列がそうならないから 
    public string DisplayEventTypeName;
    public string EventTypeName;
    public CodeGenDescription(System.Type type)
    {
        Type = type;
        CallbackMethodName = "On" + type.Name;
        DisplayEventTypeName = type.Name;
    }
}

public class UIEventBuilderInspector : EditorWindow
{
    VisualTreeAsset treeAsset;
    ObjectField treeAssetField;
    VisualElement container;
    VisualElement leftPane;
    VisualElement rightPane;
    VisualElement currentSelection;
    EditorWindowCodeBuilder editorWindowCodeBuilder;
    ScrollView scrollView;
    string ClassName => treeAssetField.value.name;
    string Path => AssetDatabase.GetAssetPath(treeAssetField.value);

    [MenuItem("Window/UI/UI Event Builder Inspector")]
    static void Init()
    {
        GetWindow<UIEventBuilderInspector>();
    }

    void OnEnable()
    {
        Selection.selectionChanged += SelectionChange;
        treeAssetField = new ObjectField("uxml");
        treeAssetField.objectType = typeof(VisualTreeAsset);
        rootVisualElement.Add(treeAssetField);

        var rootPane = new VisualElement();
        rootPane.style.flexDirection = FlexDirection.Row;
        rootVisualElement.Add(rootPane);

        leftPane = new VisualElement();
        leftPane.style.width = Screen.width / 2;
        leftPane.style.height = Screen.height;
        rightPane = new VisualElement();
        rightPane.style.width = Screen.width / 2;
        rightPane.style.height = Screen.height;
        rootPane.Add(leftPane);
        rootPane.Add(rightPane);
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
            editorWindowCodeBuilder = new EditorWindowCodeBuilder(Path);
            BuildTree();
        }
    }

    void BuildTree()
    {
        if (container != null)
        {
            leftPane.Remove(container);
        }
        container = (treeAssetField.value as VisualTreeAsset).CloneTree();
        container.Query<VisualElement>().Where(x => x.enabledInHierarchy).ForEach(e =>
        {
            if (e is Button)
            {
                e.RegisterCallback<MouseUpEvent>(evt =>
                {
                    OnSelectVisualElement(e);
                });
            }
            else
            {
                e.RegisterCallback<MouseDownEvent>(evt =>
                {
                    OnSelectVisualElement(e);
                });
            }
        });
        // container.Query<Button>().ForEach(button => AddContextualMenu(button));
        leftPane.Add(container);
    }

    void OnSelectVisualElement(VisualElement e)
    {
        currentSelection = e;
        BuildEventView(e);
    }

    void BuildEventView(VisualElement e)
    {
        if (scrollView == null)
        {
            scrollView = new ScrollView();
        }
        scrollView.SetEnabled(!string.IsNullOrEmpty(e.name));

        scrollView.Clear();
        var types = new List<CodeGenDescription>();
        if (e is Button)
        {
            types = new List<CodeGenDescription>
            {
                new CodeGenDescription(typeof(Button))
                {
                    CallbackMethodName = $"OnClick_{e.name}",
                    DisplayEventTypeName="OnClick"
                }
            };
        }
        else if (e is INotifyValueChanged<string>)
        {
            Debug.Log("string value changed.");
            // types = TypeCache.GetTypesDerivedFrom<INotifyValueChanged<string>>();
            types = new List<CodeGenDescription>
            {
                new CodeGenDescription(typeof(ChangeEvent<string>))
                {
                    CallbackMethodName=$"OnValueChanged_{e.name}",
                    DisplayEventTypeName = "OnValueChanged",
                    EventTypeName = "ChangeEvent<string>"
                }
            };
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
        else
        {
            types = TypeCache.GetTypesDerivedFrom<EventBase>()
            .Where(x => !x.IsAbstract)
            .Select(x => new CodeGenDescription(x))// Type = x, EventTypeName = x.Name })
            .OrderBy(x => x.Type.Name)
            .ToList();
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
        rightPane.Add(scrollView);
    }

    // void AddContextualMenu(Button button)
    // {
    // button.AddManipulator(new ContextualMenuManipulator(evt =>
    // {
    //     evt.menu.AppendAction(
    //         actionName: "MenuAction",
    //         action: (a) => Debug.Log(""),
    //         actionStatusCallback: (a) => DropdownMenuAction.Status.Normal);
    // }));
    // }

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
            stubCode = EditorWindowCodeGen.GetRegisterClickedCode(currentSelection.name);
        }
        else
        {
            stubCode = EditorWindowCodeGen.GetRegisterCallbackCode(desc, currentSelection.name);
        }
        if (!registerWindowScript.text.Contains(stubCode))
        {
            Debug.Log($"Not Contains {stubCode}");
            var script = EditorWindowCodeGen.GetStubbedCodeAtSecondLastIndexOfBlacket(registerWindowScript.text, stubCode);
            SaveWindowScript(windowRegisterFilePath, script);
        }

        editorWindowCodeBuilder.StubCode(desc, currentSelection);
    }
}