using UnityEngine.UIElements;
using System;

/// <summary>
/// マウスダブルクリック認識のManipulatorクラス
/// VisualElementにAddManipulatorして使用します。
/// </summary>
public class MouseDoubleClickManipulator : Manipulator
{
    bool isClicked;
    long triggerDoubleClickInMilliSec = 200;
    Action onSingleClick;
    Action onDoubleClick;

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        target.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        target.RegisterCallback<MouseOutEvent>(OnMouseOutEvent);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
        target.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
        target.UnregisterCallback<MouseOutEvent>(OnMouseOutEvent);
    }

    /// <summary>
    /// ダブルクリックを認識するまでに要する時間(ミリ秒)を指定します。
    /// デフォルトは200ミリ秒です。
    /// </summary>
    public MouseDoubleClickManipulator TriggerDoubleClickIn(long triggerDoubleClickInMilliSec)
    {
        this.triggerDoubleClickInMilliSec = triggerDoubleClickInMilliSec;
        return this;
    }

    public MouseDoubleClickManipulator RegisterSingleClick(Action onSingleClick)
    {
        this.onSingleClick = onSingleClick;
        return this;
    }

    public MouseDoubleClickManipulator RegisterDoubleClick(Action onDoubleClick)
    {
        this.onDoubleClick = onDoubleClick;
        return this;
    }

    void OnMouseDownEvent(MouseEventBase<MouseDownEvent> evt)
    {
        // Buttonの場合、MouseDownEventはButtonでは発生されないのでUpでおこなう
        if (target is Button) { return; }
        HandleDoubleClick();
    }

    void OnMouseUpEvent(MouseEventBase<MouseUpEvent> evt)
    {
        if (target is Button)
        {
            HandleDoubleClick();
        }
    }

    void OnMouseOutEvent(MouseEventBase<MouseOutEvent> evt)
    {
        isClicked = false;
    }

    void HandleSingleClick()
    {
        if (isClicked)
        {
            isClicked = false;
            onSingleClick?.Invoke();
        }
    }

    void HandleDoubleClick()
    {
        if (isClicked)
        {
            isClicked = false;
            onDoubleClick?.Invoke();
            return;
        }
        // 一発目ではSingleかDoubleかの判定をとるため、すぐSingleClick処理をせず、
        // triggerDoubleClickInMilliSec後にHandleSingleClickメソッドを実行します。
        // その間にクリックされたらDoubleクリックとなります。
        isClicked = true;
        target.schedule.Execute(_ => HandleSingleClick()).StartingIn(triggerDoubleClickInMilliSec);
    }
}