# Platform Comparison: Pointer/Mouse Support

## Feature Comparison Matrix

| Feature | Windows | macCatalyst | iOS | Android |
|---------|---------|-------------|-----|---------|
| **Basic Touch** | ✅ Full | ✅ Full | ✅ Full | ✅ Full |
| **Left Mouse Button** | ✅ Full | ✅ Basic | ❌ N/A | ✅ Full |
| **Right Mouse Button** | ✅ Full | ⚠️ Heuristic | ❌ N/A | ✅ Full |
| **Middle Mouse Button** | ✅ Full | ❌ Not Available | ❌ N/A | ✅ Full |
| **Extended Buttons (XButton1-9)** | ✅ Full | ❌ Not Available | ❌ N/A | ⚠️ XButton1-2 Only |
| **Gaming Mouse Support** | ✅ Unlimited | ❌ Not Available | ❌ N/A | ⚠️ Limited |
| **Hover Tracking** | ✅ Full | ✅ Basic | ❌ N/A | ✅ Full |
| **Trackpad Scrolling** | ❌ N/A | ✅ Basic | ❌ N/A | ❌ N/A |
| **Scroll Wheel** | ✅ Full | ❌ N/A | ❌ N/A | ✅ Full |
| **Pen/Stylus Detection** | ✅ Full | ✅ Apple Pencil | ✅ Apple Pencil | ✅ Full |
| **Pressure Sensitivity** | ✅ Full | ✅ Touch Only | ✅ Touch Only | ✅ Full |
| **Pressure During Hover** | ✅ Yes | ❌ UIKit Limitation | ❌ N/A | ⚠️ Stylus Only |
| **Multi-Button Drag** | ✅ Full | ⚠️ Limited | ❌ N/A | ✅ Full |
| **Button State Tracking** | ✅ Full | ⚠️ Limited | ❌ N/A | ✅ Full |

## Implementation Details

### Windows (Full Support)
```csharp
// Complete implementation using PointerRoutedEventArgs
- All mouse buttons (Left, Right, Middle, XButton1-9, Extended)
- Precise button detection via Windows APIs
- Full pressure sensitivity during hover and touch
- Gaming mouse support with ButtonNumber property
- Comprehensive hover tracking
```

### macCatalyst (Basic Support)
```csharp
// Limited implementation using UIKit
- Left/Right button detection via UITouch heuristics
- Apple Pencil detection via UITouchType.Stylus
- Hover tracking via UIHoverGestureRecognizer
- Trackpad scrolling via UIScrollView detection
- Pressure only during touch events (UIKit limitation)
- No extended buttons (UIKit doesn't expose them)
```

### iOS (Touch Only)
```csharp
// Touch and Apple Pencil only
- Standard touch gestures
- Apple Pencil with pressure sensitivity
- No mouse support (not applicable)
```

### Android (Good Support)
```csharp
// Comprehensive implementation using MotionEvent
- Full mouse button detection via MotionEvent.ButtonState
- Left/Right/Middle/XButton1-2 button support
- Stylus detection via MotionEventToolType.Stylus
- Hover tracking via OnHoverListener
- Scroll wheel via OnGenericMotionListener
- Pressure sensitivity for stylus input
- API level compatibility (ButtonPress/Release for API 23+)
```

## Technical Limitations

### macCatalyst Specific Limitations

1. **UIKit Constraints**:
   - `UIEvent` doesn't expose detailed mouse button information
   - Extended buttons (XButton3-9) not available through UIKit
   - Pressure data only available during actual touch events, not hover

2. **Heuristic Detection**:
   - Right-click detection based on touch characteristics
   - Not as reliable as Windows native button detection
   - May have false positives/negatives

3. **Missing Features**:
   - No middle button detection
   - No gaming mouse extended button support
   - No pressure tracking during hover

### Android Specific Limitations

1. **API Level Dependencies**:
   - `ButtonPress`/`ButtonRelease` events only available on Android API 23+
   - Older devices fall back to basic touch detection
   - Some mouse features depend on Android version

2. **Hardware Variations**:
   - Mouse support varies by device manufacturer
   - Not all Android devices support external mice
   - Gaming mouse extended buttons depend on hardware/drivers

3. **Platform Constraints**:
   - Extended buttons beyond XButton2 may not be available
   - Pressure during hover limited to stylus input only

### Workarounds Implemented

1. **Hover Tracking**:
   - macCatalyst: Added `UIHoverGestureRecognizer` for basic mouse movement detection
   - Android: Added `OnHoverListener` for comprehensive hover support
2. **Trackpad Scrolling**: Added `UIScrollView` detection for two-finger trackpad pan gestures (macCatalyst)
3. **Smart Button Handling**: Same architecture across all platforms (Left = standard, Others = Pointer)
4. **Stylus Support**:
   - macCatalyst: Apple Pencil detection and pressure sensitivity during touch events
   - Android: Full stylus support via `MotionEventToolType.Stylus` with pressure
5. **API Compatibility**: Android API level checks for ButtonPress/Release events (API 23+)

## Recommendations

### For Cross-Platform Apps
- Design UI assuming basic mouse support (Left/Right click only)
- Don't rely on extended buttons for core functionality
- Use hover for enhancement, not core features
- Test on multiple platforms (Windows, Android, macCatalyst)

### For Windows-Specific Features
- Extended buttons and gaming mouse features
- Pressure-sensitive hover interactions
- Complex multi-button operations

### For macCatalyst-Specific Features
- Apple Pencil integration
- Trackpad scrolling gestures
- Touch-first design with mouse enhancement

### For Android-Specific Features
- USB/Bluetooth mouse support
- Stylus pressure sensitivity
- Chromebook and Android TV compatibility
- DeX mode optimization

## Future Improvements

### Possible Enhancements
1. **Better Right-Click Detection**: Investigate additional UIKit APIs
2. **Middle Button Support**: Research alternative detection methods
3. **Pressure During Hover**: Monitor UIKit updates for new capabilities

### Platform Limitations
- Extended buttons will likely never be available on macCatalyst (UIKit limitation)
- Pressure during hover may require Apple to expose new APIs
- Gaming mouse features are Windows-specific by nature
