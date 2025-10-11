namespace AppoMobi.Maui.Gestures;

public enum TouchHandlingStyle
{
    Default,

    /// <summary>
    /// Locks totally input for self, useful inside scroll view, panning controls like slider etc
    /// </summary>
    Lock,

    /// <summary>
    /// You control how it works with with parent controls, if you set the effect `WillLock` property to Locked/Unlocked at runtime.
    /// This allows to work simultaneously inside a ScrollView, you could set WillLock to locked when you consume panning, and unlock it for the panning is wrong direction..
    /// </summary>
    Manual,

    /// <summary>
    /// Same as InputTransparent=true
    /// </summary>
    Disabled
}
