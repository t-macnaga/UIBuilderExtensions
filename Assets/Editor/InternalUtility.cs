using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
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

internal class BuilderWindowInternal
{
    EditorWindow builderWindow;
    PropertyInfo selectionProp;
    PropertyInfo visualTreeAssetProp;
    VisualElement prevSelection;
    object builderSelection;
    object builderDocument;
    public VisualTreeAsset TreeAsset => visualTreeAssetProp.GetValue(builderDocument) as VisualTreeAsset;
    IEnumerable<VisualElement> Selections => selectionProp.GetValue(builderSelection) as IEnumerable<VisualElement>;
    public System.Action<VisualElement> OnSelectionChanged { get; set; }
    public VisualElement CurrentSelection { get; private set; }

    public void Update()
    {
        InitializeIfNeeded();
        if (builderWindow == null) { return; }
        CurrentSelection = Selections.FirstOrDefault();
        if (prevSelection != CurrentSelection)
        {
            prevSelection = CurrentSelection;
            OnSelectionChanged(CurrentSelection);
        }
    }

    void InitializeIfNeeded()
    {
        if (builderWindow != null) { return; }
        builderWindow = Resources.FindObjectsOfTypeAll<EditorWindow>()
        .FirstOrDefault(x => x.GetType().Name == "Builder");
        if (builderWindow == null) { return; }
        builderSelection = builderWindow.GetType().GetProperty("selection").GetValue(builderWindow);
        var builderToolbar = builderWindow.GetType().GetProperty("toolbar").GetValue(builderWindow);
        builderDocument = builderToolbar.GetType().GetProperty("document", BindingFlags.NonPublic | BindingFlags.Instance)
        .GetValue(builderToolbar);
        selectionProp = builderSelection.GetType().GetProperty("selection");
        visualTreeAssetProp = builderDocument.GetType().GetProperty("visualTreeAsset");
    }
}