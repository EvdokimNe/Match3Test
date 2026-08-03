using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.Board.Input
{
    public static class PointerReader
    {
        public static bool TryGetPosition(out Vector2 position)
        {
            var touch = Touchscreen.current;
            if (touch != null && (touch.primaryTouch.press.isPressed || touch.primaryTouch.press.wasReleasedThisFrame))
            {
                position = touch.primaryTouch.position.ReadValue();
                return true;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                position = mouse.position.ReadValue();
                return true;
            }

            position = default;
            return false;
        }

        public static bool PressedThisFrame()
        {
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
                return true;

            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        public static bool ReleasedThisFrame()
        {
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasReleasedThisFrame)
                return true;

            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasReleasedThisFrame;
        }
    }
}
