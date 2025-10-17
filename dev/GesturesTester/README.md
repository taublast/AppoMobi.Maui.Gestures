# Gesture Tester - macCatalyst Pointer Testing App

This test application is designed to validate gesture and pointer functionality in the AppoMobi.Maui.Gestures library, with a specific focus on **macCatalyst pointer gesture detection**.

## Purpose

Based on the code analysis documented in `/Claude.md`, this tester focuses on validating:

1. **macCatalyst Mouse Button Detection** - Testing the reliability of left/right/middle click detection
2. **Hover Tracking** - Mouse movement without button press
3. **Trackpad Scrolling** - Two-finger scroll gesture detection
4. **Multi-Touch Gestures** - Pinch/zoom and rotation
5. **Manual Mode** - Dynamic touch handling in ScrollView
6. **Pointer Data** - Validation of all pointer event properties

## Known Issues to Test

The analysis identified several concerns with macCatalyst implementation:

### CRITICAL: Right-Click Detection Unreliable (Issue #3)
**Location:** `TouchRecognizer.Apple.cs:238-265`

The right-click detection is based on heuristics:
```csharp
if (touch.TapCount == 0 || force > 0.8 * maxForce)
    return MouseButton.Right; // Heuristic, may be wrong!
```

**Expected Behavior:**
- Left-click should reliably trigger "Left Click" button
- Right-click should reliably trigger "Right Click" button
- Event log should show correct button identification

**What to Check:**
- Does left-click get misidentified as right-click?
- Does right-click get detected at all?
- Are button events consistent?

### Issue #4: Trackpad Detection Hacky
**Location:** `TouchRecognizer.Apple.cs:341-424`

Uses invisible UIScrollView overlay for trackpad detection.

**What to Check:**
- Does trackpad scrolling work in the Canvas area?
- Does it interfere with the main page ScrollView?
- Is performance acceptable?

### Issue #5: Parent Recognizer Cancellation Disabled
**Location:** `TouchRecognizer.Apple.cs:534-543`

Critical Manual mode code is commented out.

**What to Check:**
- Can the slider be moved horizontally while page is in ScrollView?
- Does vertical scrolling work when not touching slider?
- Can Manual mode dynamically take control?

## Test Areas

### 1. Pointer Tracking Canvas (Yellow Area)
**Purpose:** Test raw pointer tracking and event detection

**Tests:**
- Move mouse over canvas (should show red circle following cursor)
- Left-click on canvas
- Right-click on canvas
- Hover without clicking
- Two-finger scroll on trackpad

**Expected Results:**
- Red circle follows pointer precisely
- Event log shows all movements and clicks
- Pointer type (Mouse/Pen/Touch) correctly identified
- Button numbers accurate

### 2. Interactive Button Tests (Blue/Red Buttons)
**Purpose:** Test button-specific pointer events

**Tests:**
- Left-click on blue button
- Right-click on blue button
- Left-click on red button
- Right-click on red button

**Expected Results:**
- Correct button identification in feedback label
- Event log shows MouseButton enum correctly
- ButtonNumber property accurate (1=Left, 2=Right, etc.)

### 3. Multi-Touch Test (Green Area)
**Purpose:** Test multi-finger gestures

**Tests:**
- Pinch with two fingers
- Trackpad pinch gesture
- Rotation gesture

**Expected Results:**
- Scale and rotation values update
- TouchesCount shows correct finger count
- Manipulation info accurate

### 4. Manual Mode Test (Slider in ScrollView)
**Purpose:** Test dynamic touch mode control

**Tests:**
- Try to scroll page vertically by swiping on slider
- Try to move slider horizontally
- Verify smooth transitions between scrolling and slider control

**Expected Results:**
- Horizontal slider movement works
- Vertical scrolling works when not on slider
- No conflicts or stuck gestures

## Event Log

The bottom panel shows detailed event information:

```
[001] 12:34:56.789 Canvas      Moved    [POINTER Mouse Btn:Left(1) Pressed] Touches:1 Δ(5,3) V:350
```

**Format:**
- `[001]` - Event counter
- `12:34:56.789` - Timestamp
- `Canvas` - Source control
- `Moved` - Touch action type
- `[POINTER ...]` - Pointer-specific data (only for mouse/pen)
- `Touches:1` - Number of active touches
- `Δ(5,3)` - Delta movement (pixels)
- `V:350` - Velocity (pixels/second)

## Running the Test

### On macCatalyst (Mac)

```bash
cd dev/GesturesTester
dotnet build -f net9.0-maccatalyst
dotnet run -f net9.0-maccatalyst
```

### What to Report

When testing, document:

1. **Left-Click Accuracy:**
   - Are left-clicks always detected as MouseButton.Left?
   - Any false positives/negatives?

2. **Right-Click Accuracy:**
   - Are right-clicks detected?
   - What percentage of right-clicks are misidentified?
   - Are they detected as Left or ignored?

3. **Hover Tracking:**
   - Does pointer movement without click work?
   - Is it smooth?
   - Any lag or missing events?

4. **Trackpad Scrolling:**
   - Is two-finger scroll detected?
   - Does it conflict with page ScrollView?
   - Performance acceptable?

5. **Manual Mode:**
   - Can slider be moved while page is scrollable?
   - Are there any stuck states?

6. **Event Data Accuracy:**
   - Are Pointer.Button values correct?
   - Is DeviceType accurate (Mouse/Pen/Touch)?
   - Are ButtonNumber values correct?

## Expected Test Results Matrix

| Test | Windows | macCatalyst | Likely Result |
|------|---------|-------------|---------------|
| Left Click | ✅ Works | ❓ Test | Should work |
| Right Click | ✅ Works | ❌ Unreliable | **MAIN CONCERN** |
| Middle Click | ✅ Works | ❌ Won't work | UIKit limitation |
| Hover Tracking | ✅ Works | ✅ Should work | UIHoverGestureRecognizer |
| Trackpad Scroll | N/A | ⚠️ Hacky | May interfere |
| Manual Mode | ✅ Works | ⚠️ Limited | Commented code |
| Pen Pressure | ✅ Works | ✅ Apple Pencil | During touch only |

## Analysis Reference

See `/Claude.md` for full code analysis including:
- 10 identified issues with severity levels
- Platform-specific implementation details
- Architecture breakdown
- Performance considerations
- Recommendations for fixes

## Next Steps After Testing

1. Document exact failure scenarios for right-click detection
2. Measure trackpad detection accuracy
3. Test Manual mode edge cases
4. Consider implementing fixes:
   - UIContextMenuInteraction for right-click
   - Ctrl+Click fallback
   - Long-press as right-click alternative
5. Update README.md with platform limitations

---

**Test Priority:** Focus on right-click detection reliability - this is the primary concern from code analysis.
