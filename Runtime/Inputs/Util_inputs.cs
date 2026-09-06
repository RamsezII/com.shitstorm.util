#if ENABLE_INPUT_SYSTEM
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.InputSystem.InputActionSetupExtensions;

namespace _UTIL_
{
    public enum KeyboardSpecial
    {
        /// <summary><c>&lt;Keyboard&gt;/shift</c>; synthetic <c>Button</c> combining <c>leftShift</c> and <c>rightShift</c>.</summary>
        Shift,
        /// <summary><c>&lt;Keyboard&gt;/ctrl</c>; synthetic <c>Button</c> combining <c>leftCtrl</c> and <c>rightCtrl</c>.</summary>
        Ctrl,
        /// <summary><c>&lt;Keyboard&gt;/alt</c>; synthetic <c>Button</c> combining <c>leftAlt</c> and <c>rightAlt</c>.</summary>
        Alt
    }

    public enum GamepadSpecial
    {
        /// <summary><c>&lt;Gamepad&gt;/leftStick</c>; <c>Vector2</c> movement. The click is <c>Gamepad.leftStickButton</c>, path <c>&lt;Gamepad&gt;/leftStickPress</c>, type <c>Button</c>.</summary>
        LeftStick,
        /// <summary><c>&lt;Gamepad&gt;/leftStick/x</c>; horizontal <c>float</c> axis from -1 to 1 with an axis deadzone.</summary>
        LeftStickX,
        /// <summary><c>&lt;Gamepad&gt;/leftStick/y</c>; vertical <c>float</c> axis from -1 to 1 with an axis deadzone.</summary>
        LeftStickY,

        /// <summary><c>&lt;Gamepad&gt;/rightStick</c>; <c>Vector2</c> movement. The click is <c>Gamepad.rightStickButton</c>, path <c>&lt;Gamepad&gt;/rightStickPress</c>, type <c>Button</c>.</summary>
        RightStick,
        /// <summary><c>&lt;Gamepad&gt;/rightStick/x</c>; horizontal <c>float</c> axis from -1 to 1 with an axis deadzone.</summary>
        RightStickX,
        /// <summary><c>&lt;Gamepad&gt;/rightStick/y</c>; vertical <c>float</c> axis from -1 to 1 with an axis deadzone.</summary>
        RightStickY,

        /// <summary><c>&lt;Gamepad&gt;/dpad</c>; normalized <c>Vector2</c> synthesized from the four <c>Button</c> controls <c>dpad/up</c>, <c>down</c>, <c>left</c>, and <c>right</c>.</summary>
        Dpad,
        /// <summary><c>&lt;Gamepad&gt;/dpad/x</c>; horizontal <c>float</c> component of the normalized D-pad: negative left, positive right.</summary>
        DpadX,
        /// <summary><c>&lt;Gamepad&gt;/dpad/y</c>; vertical <c>float</c> component of the normalized D-pad: negative down, positive up.</summary>
        DpadY,
    }

    public enum MouseSpecial
    {
        /// <summary><c>&lt;Mouse&gt;/position</c>; absolute <c>Vector2</c> position in Unity window or Display space.</summary>
        Position,
        /// <summary><c>&lt;Mouse&gt;/position/x</c>; absolute horizontal <c>float</c> coordinate in window or Display space.</summary>
        PositionX,
        /// <summary><c>&lt;Mouse&gt;/position/y</c>; absolute vertical <c>float</c> coordinate in window or Display space.</summary>
        PositionY,

        /// <summary><c>&lt;Mouse&gt;/delta</c>; <c>Vector2</c> movement accumulated during the update, then automatically reset before the next one.</summary>
        Delta,
        /// <summary><c>&lt;Mouse&gt;/delta/x</c>; horizontal <c>float</c> component of the accumulated delta, reset on every update.</summary>
        DeltaX,
        /// <summary><c>&lt;Mouse&gt;/delta/y</c>; vertical <c>float</c> component of the accumulated delta, reset on every update.</summary>
        DeltaY,

        /// <summary><c>&lt;Mouse&gt;/scroll</c>; delta-style <c>Vector2</c> scrolling: horizontal X and vertical Y, accumulated then reset on every update.</summary>
        Scroll,
        /// <summary><c>&lt;Mouse&gt;/scroll/x</c>; horizontal scrolling as a <c>float</c>, unavailable on most mice.</summary>
        ScrollX,
        /// <summary><c>&lt;Mouse&gt;/scroll/y</c>; vertical scrolling from the main wheel as a <c>float</c>.</summary>
        ScrollY,
    }

    public enum AxisCompositePart
    {
        /// <summary>The negative direction of a <c>1DAxis</c> composite.</summary>
        Negative,
        /// <summary>The positive direction of a <c>1DAxis</c> composite.</summary>
        Positive,
    }

    public enum Vector2CompositePart
    {
        /// <summary>The upward direction of a <c>2DVector</c> composite.</summary>
        Up,
        /// <summary>The downward direction of a <c>2DVector</c> composite.</summary>
        Down,
        /// <summary>The leftward direction of a <c>2DVector</c> composite.</summary>
        Left,
        /// <summary>The rightward direction of a <c>2DVector</c> composite.</summary>
        Right,
    }

    public static partial class Util_inputs
    {
        public static string Path(this Key key) => key switch
        {
            Key.None => throw new ArgumentOutOfRangeException(nameof(key)),
            Key.Digit0 => "0",
            Key.Digit1 => "1",
            Key.Digit2 => "2",
            Key.Digit3 => "3",
            Key.Digit4 => "4",
            Key.Digit5 => "5",
            Key.Digit6 => "6",
            Key.Digit7 => "7",
            Key.Digit8 => "8",
            Key.Digit9 => "9",
            _ when Enum.IsDefined(typeof(Key), key) => key.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(key)),
        };

        public static string Path(this KeyboardSpecial key) => key switch
        {
            KeyboardSpecial.Shift => "shift",
            KeyboardSpecial.Ctrl => "ctrl",
            KeyboardSpecial.Alt => "alt",
            _ => throw new ArgumentOutOfRangeException(nameof(key)),
        };

        public static string Path(this GamepadButton button) => button switch
        {
            GamepadButton.DpadUp => "dpad/up",
            GamepadButton.DpadDown => "dpad/down",
            GamepadButton.DpadLeft => "dpad/left",
            GamepadButton.DpadRight => "dpad/right",
            GamepadButton.North => "buttonNorth",
            GamepadButton.East => "buttonEast",
            GamepadButton.South => "buttonSouth",
            GamepadButton.West => "buttonWest",
            GamepadButton.LeftStick => "leftStickPress",
            GamepadButton.RightStick => "rightStickPress",
            GamepadButton.LeftShoulder => "leftShoulder",
            GamepadButton.RightShoulder => "rightShoulder",
            GamepadButton.Start => "start",
            GamepadButton.Select => "select",
            GamepadButton.LeftTrigger => "leftTrigger",
            GamepadButton.RightTrigger => "rightTrigger",
            _ => throw new ArgumentOutOfRangeException(nameof(button)),
        };

        public static string Path(this GamepadSpecial control) => control switch
        {
            GamepadSpecial.LeftStick => "leftStick",
            GamepadSpecial.LeftStickX => "leftStick/x",
            GamepadSpecial.LeftStickY => "leftStick/y",

            GamepadSpecial.RightStick => "rightStick",
            GamepadSpecial.RightStickX => "rightStick/x",
            GamepadSpecial.RightStickY => "rightStick/y",

            GamepadSpecial.Dpad => "dpad",
            GamepadSpecial.DpadX => "dpad/x",
            GamepadSpecial.DpadY => "dpad/y",

            _ => throw new ArgumentOutOfRangeException(nameof(control)),
        };

        public static string Path(this MouseButton button) => button switch
        {
            MouseButton.Left => "leftButton",
            MouseButton.Right => "rightButton",
            MouseButton.Middle => "middleButton",
            MouseButton.Forward => "forwardButton",
            MouseButton.Back => "backButton",
            _ => throw new ArgumentOutOfRangeException(nameof(button)),
        };

        public static string Path(this MouseSpecial control) => control switch
        {
            MouseSpecial.Position => "position",
            MouseSpecial.PositionX => "position/x",
            MouseSpecial.PositionY => "position/y",

            MouseSpecial.Delta => "delta",
            MouseSpecial.DeltaX => "delta/x",
            MouseSpecial.DeltaY => "delta/y",

            MouseSpecial.Scroll => "scroll",
            MouseSpecial.ScrollX => "scroll/x",
            MouseSpecial.ScrollY => "scroll/y",

            _ => throw new ArgumentOutOfRangeException(nameof(control)),
        };

        public static CompositeSyntax AddCompositeBinding_1DAxis(this InputAction action) => action.AddCompositeBinding("1DAxis");

        public static CompositeSyntax With_gamepad(this CompositeSyntax composite, in AxisCompositePart part, in GamepadButton button) => composite.With(part switch
        {
            AxisCompositePart.Negative => "Negative",
            AxisCompositePart.Positive => "Positive",
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        }, $"<Gamepad>/{button.Path()}");

        public static CompositeSyntax With_keyboard(this CompositeSyntax composite, in AxisCompositePart part, in Key key) => composite.With(part switch
        {
            AxisCompositePart.Negative => "Negative",
            AxisCompositePart.Positive => "Positive",
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        }, $"<Keyboard>/{key.Path()}");

        public static CompositeSyntax AddCompositeBinding_2DVector(this InputAction action) => action.AddCompositeBinding("2DVector");

        public static CompositeSyntax With_keyboard(this CompositeSyntax composite, in Vector2CompositePart part, in Key key) => composite.With(part switch
        {
            Vector2CompositePart.Up => "Up",
            Vector2CompositePart.Down => "Down",
            Vector2CompositePart.Left => "Left",
            Vector2CompositePart.Right => "Right",
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        }, $"<Keyboard>/{key.Path()}");

        public static BindingSyntax AddBinding_keyboard(this InputAction action, in Key key) => action.AddBinding($"<Keyboard>/{key.Path()}");
        public static BindingSyntax AddBinding_keyboard_special(this InputAction action, in KeyboardSpecial key) => action.AddBinding($"<Keyboard>/{key.Path()}");
        public static BindingSyntax AddBinding_gamepad(this InputAction action, in GamepadButton button) => action.AddBinding($"<Gamepad>/{button.Path()}");
        public static BindingSyntax AddBinding_gamepad_special(this InputAction action, in GamepadSpecial control) => action.AddBinding($"<Gamepad>/{control.Path()}");
        public static BindingSyntax AddBinding_mouse_special(this InputAction action, in MouseSpecial control) => action.AddBinding($"<Mouse>/{control.Path()}");
        public static BindingSyntax AddBinding_mouse(this InputAction action, in MouseButton button) => action.AddBinding($"<Mouse>/{button.Path()}");
    }
}
#endif
