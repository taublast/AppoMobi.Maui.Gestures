namespace AppoMobi.Maui.Gestures;

/// <summary>
/// Flags enumeration for tracking multiple pressed mouse buttons simultaneously
/// </summary>
[Flags]
public enum MouseButtons
{
    /// <summary>
    /// No buttons pressed
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Left mouse button (primary button)
    /// </summary>
    Left = 1,
    
    /// <summary>
    /// Right mouse button (secondary button)
    /// </summary>
    Right = 2,
    
    /// <summary>
    /// Middle mouse button (wheel button)
    /// </summary>
    Middle = 4,
    
    /// <summary>
    /// Extended button 1 (typically Back button)
    /// </summary>
    XButton1 = 8,
    
    /// <summary>
    /// Extended button 2 (typically Forward button)
    /// </summary>
    XButton2 = 16,
    
    /// <summary>
    /// Extended button for gaming mice (button 6)
    /// </summary>
    XButton3 = 32,
    
    /// <summary>
    /// Extended button for gaming mice (button 7)
    /// </summary>
    XButton4 = 64,
    
    /// <summary>
    /// Extended button for gaming mice (button 8)
    /// </summary>
    XButton5 = 128,
    
    /// <summary>
    /// Extended button for gaming mice (button 9)
    /// </summary>
    XButton6 = 256,
    
    /// <summary>
    /// Extended button for gaming mice (button 10)
    /// </summary>
    XButton7 = 512,
    
    /// <summary>
    /// Extended button for gaming mice (button 11)
    /// </summary>
    XButton8 = 1024,
    
    /// <summary>
    /// Extended button for gaming mice (button 12)
    /// </summary>
    XButton9 = 2048
}
