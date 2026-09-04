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

        public static CompositeSyntax AddCompositeBinding_2DVector(this InputAction action) => action.AddCompositeBinding("2DVector");

        public static CompositeSyntax With_keyboard(this CompositeSyntax composite, in Vector2CompositePart part, in Key key) => composite.With(part switch
        {
            Vector2CompositePart.Up => "Up",
            Vector2CompositePart.Down => "Down",
            Vector2CompositePart.Left => "Left",
            Vector2CompositePart.Right => "Right",
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        }, $"<Keyboard>/{key}");

        public static BindingSyntax AddBinding_keyboard(this InputAction action, in Key key) => action.AddBinding($"<Keyboard>/{key}");
        public static BindingSyntax AddBinding_keyboard_special(this InputAction action, in KeyboardSpecial key) => action.AddBinding($"<Keyboard>/{key}");
        public static BindingSyntax AddBinding_gamepad(this InputAction action, in GamepadButton button) => action.AddBinding($"<Gamepad>/{button}");
        public static BindingSyntax AddBinding_gamepad_special(this InputAction action, in GamepadSpecial control) => action.AddBinding($"<Gamepad>/{control.Path()}");
        public static BindingSyntax AddBinding_mouse_special(this InputAction action, in MouseSpecial control) => action.AddBinding($"<Mouse>/{control.Path()}");
        public static BindingSyntax AddBinding_mouse(this InputAction action, in MouseButton button) => action.AddBinding($"<Mouse>/{button}Button");
    }
}
#endif
