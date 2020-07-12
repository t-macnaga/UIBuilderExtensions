using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

public partial class Sample : EditorWindowBase
{
    [MenuItem("Window/Sample")]
    static void Init()
    {
        GetWindow<Sample>();
    }

    void OnEnable()
    {
        InitializeComponents();
    }

    void OnClick_button1()
    {
        Debug.Log("button1 Clicked");
    }


    void OnAttachToPanelEvent_button1(AttachToPanelEvent evt)
    {
        Debug.Log("On AttachToPanelEvent");
    }


    void OnMouseUpEvent_button1(MouseUpEvent evt)
    {
        Debug.Log("On MouseUpEvent");
    }

}
