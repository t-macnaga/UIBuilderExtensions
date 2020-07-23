using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

public partial class SampleWindow : EditorWindowBase
{
    [MenuItem("Window/SampleWindow")]
    static void Init()
    {
        GetWindow<SampleWindow>();
    }

    void OnEnable()
    {
        InitializeComponents();
    }

    void OnClick_button2()
    {
        Debug.Log("button2 Clicked");
    }

}
