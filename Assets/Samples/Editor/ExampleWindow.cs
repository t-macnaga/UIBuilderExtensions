using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ExampleWindow : EditorWindow
{
    [MenuItem("Window/ExampleWindow")]
    static void Init()
    {
        GetWindow<ExampleWindow>(desiredDockNextTo: typeof(SceneView));
    }

    void OnEnable()
    {
        var button1 = new Button(() => Debug.Log("button1"))
        {
            name = "button1",
            text = "button1"
        };
        rootVisualElement.Add(button1);
    }

    void ShowWarning()
    {
        var content = new GUIContent(string.Empty, EditorGUIUtility.FindTexture("console.warnicon"));
        content.text = "Message";
        ShowNotification(content, fadeoutWait: 5);
    }
}