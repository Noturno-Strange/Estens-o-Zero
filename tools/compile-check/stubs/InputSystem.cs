// Stubs de compilação do Input System 1.20 (subset usado pelo projeto).
using UnityEngine;

namespace UnityEngine.InputSystem.Controls
{
    public class ButtonControl { public bool isPressed => false; public bool wasPressedThisFrame => false; public bool wasReleasedThisFrame => false; }
    public class KeyControl : ButtonControl { }
    public class Vector2Control { public Vector2 ReadValue() => default; }
}

namespace UnityEngine.InputSystem
{
    using UnityEngine.InputSystem.Controls;

    public class Keyboard
    {
        public static Keyboard current { get; }
        public KeyControl aKey, bKey, cKey, dKey, eKey, fKey, gKey, hKey, iKey, jKey, kKey, lKey, mKey, nKey, oKey, pKey, qKey, rKey, sKey, tKey, uKey, vKey, wKey, xKey, yKey, zKey;
        public KeyControl digit0Key, digit1Key, digit2Key, digit3Key, digit4Key, digit5Key, digit6Key, digit7Key, digit8Key, digit9Key;
        public KeyControl tabKey, escapeKey, spaceKey, enterKey, leftShiftKey, rightShiftKey, leftCtrlKey, backspaceKey;
        public KeyControl f1Key, f2Key, f3Key, f4Key, f5Key;
        public KeyControl upArrowKey, downArrowKey, leftArrowKey, rightArrowKey;
        public KeyControl this[Key key] => null;
    }

    public enum Key { None, Space, Enter, Tab, A, B, C, D, E, F, G, Q, R, T }

    public class Mouse
    {
        public static Mouse current { get; }
        public Vector2Control delta { get; }
        public Vector2Control scroll { get; }
        public ButtonControl leftButton { get; }
        public ButtonControl rightButton { get; }
    }
}
