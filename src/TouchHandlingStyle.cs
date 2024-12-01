namespace AppoMobi.Maui.Gestures;

public enum TouchHandlingStyle
{
    Default,

    /// <summary>
    /// Lock input for self, useful inside scroll view, panning controls like slider etc
    /// </summary>
    Lock,

    ///// <summary>
    ///// Tries to let other views consume the touch event if this view doesn't handle it
    ///// </summary>
    //Share,

    /// <summary>
    /// Same as InputTransparent=true
    /// </summary>
    Disabled
}
