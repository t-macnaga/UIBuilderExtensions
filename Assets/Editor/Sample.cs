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

    void OnClick_button2()
    {
        Debug.Log("button2 Clicked");
    }


    void OnValueChanged_textField(ChangeEvent<string> evt)
    {
        Debug.Log("On ChangeEvent<string>");
    }


    void OnClick_button1()
    {
        Debug.Log("button1 Clicked");
    }

}
