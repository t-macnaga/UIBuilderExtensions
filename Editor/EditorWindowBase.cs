using UnityEditor;
using UnityEngine.UIElements;
using System.Reflection;

/// <summary>
/// UIElementsFormDesignerでUIのイベントの登録をおこなった際に自動生成される
/// ウィンドウスクリプトの基底クラスです
/// </summary>
public abstract partial class EditorWindowBase : EditorWindow
{
    protected abstract string TreeAssetPath { get; }

    protected void InitializeComponents()
    {
        var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TreeAssetPath);
        var container = template.CloneTree();
        rootVisualElement.Add(container);
        RegisterCallbacks();
    }

    protected virtual void RegisterCallbacks()
    {
    }
}
