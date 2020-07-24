using UnityEditor;

[InitializeOnLoad]
class BuilderExtensions
{
    static BuilderExtensions()
    {
        var internalBuilder = new BuilderWindowInternal();
        var builderEventInspector = new BuilderEventInspector(internalBuilder);
        internalBuilder.OnSelectionChanged = builderEventInspector.OnSelectVisualElement;
        EditorApplication.update += internalBuilder.Update;
    }
}