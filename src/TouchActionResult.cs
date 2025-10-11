namespace AppoMobi.Maui.Gestures;

public enum TouchActionResult
{
    Touch,
    Down,
    Up,
    Tapped,
    LongPressing,
    Panning,
    //Panned,
    //Swiped,
    Wheel,

    /// <summary>
    /// Mouse/pen pointer movement without press
    /// </summary>
    Pointer 
}
