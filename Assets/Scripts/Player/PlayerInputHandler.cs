using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MAIGame.Player
{
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        public event Action RestartRequested;
        public event Action EchoCreateRequested;
        public event Action EchoSelectNextRequested;
        public event Action EchoSelectPreviousRequested;
        public event Action EchoSwapRequested;

        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            MovementInput = ReadMovement(keyboard);
            LookInput = mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
            JumpPressedThisFrame = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;

            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartRequested?.Invoke();
            }

            if (keyboard != null && keyboard.qKey.wasPressedThisFrame)
            {
                EchoCreateRequested?.Invoke();
            }

            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                EchoSwapRequested?.Invoke();
            }

            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
            {
                EchoSelectNextRequested?.Invoke();
            }

            if (mouse != null)
            {
                float scrollY = mouse.scroll.ReadValue().y;
                if (scrollY > 0f)
                {
                    EchoSelectNextRequested?.Invoke();
                }
                else if (scrollY < 0f)
                {
                    EchoSelectPreviousRequested?.Invoke();
                }
            }
        }

        private static Vector2 ReadMovement(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            Vector2 input = Vector2.zero;

            if (keyboard.aKey.isPressed)
            {
                input.x -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                input.x += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                input.y -= 1f;
            }

            if (keyboard.wKey.isPressed)
            {
                input.y += 1f;
            }

            return input.sqrMagnitude > 1f ? input.normalized : input;
        }
    }
}
