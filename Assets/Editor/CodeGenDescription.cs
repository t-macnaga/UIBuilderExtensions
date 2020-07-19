using UnityEngine.UIElements;

public class CodeGenDescription
{
    public System.Type Type;
    public string CallbackMethodName; // TypeがChangeEvent<string>だと文字列がそうならないから 
    public string DisplayEventTypeName;
    public string EventTypeName;
    public CodeGenDescription(VisualElement element)
    {
        if (element is Button)
        {
            Type = typeof(Button);
            CallbackMethodName = $"OnClick_{element.name}";
            DisplayEventTypeName = "OnClick";
        }
        else if (element is INotifyValueChanged<string>)
        {
            CallbackMethodName = $"OnValueChanged_{element.name}";
            DisplayEventTypeName = "OnValueChanged";
            EventTypeName = "ChangeEvent<string>";
        }
    }

    public CodeGenDescription(System.Type type, string elementName)
    {
        Type = type;
        CallbackMethodName = $"On{type.Name}_{elementName}";
        DisplayEventTypeName = type.Name;
        EventTypeName = type.Name;
    }
}