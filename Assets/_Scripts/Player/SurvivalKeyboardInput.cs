using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivalKeyboardInput : MonoBehaviour
{
    public enum KeyboardLayout
    {
        WASD,
        Arrows
    }

    [SerializeField]
    private KeyboardLayout layout;

    [SerializeField]
    private PlayerController player;

    private void Update()
    {
        if (
            Keyboard.current == null ||
            player == null ||
            !player.IsAlive
        )
        {
            return;
        }

        switch (layout)
        {
            case KeyboardLayout.WASD:
                HandleWASD();
                break;

            case KeyboardLayout.Arrows:
                HandleArrows();
                break;
        }
    }

    private void HandleWASD()
    {
        float horizontal = 0f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        player.SetHorizontalInput(
            horizontal
        );

        if (
            Keyboard.current.wKey
                .wasPressedThisFrame
        )
        {
            player.TryChangeLane(1);
        }

        if (
            Keyboard.current.sKey
                .wasPressedThisFrame
        )
        {
            player.TryChangeLane(-1);
        }
    }

    private void HandleArrows()
    {
        float horizontal = 0f;

        if (
            Keyboard.current.rightArrowKey
                .isPressed
        )
        {
            horizontal += 1f;
        }

        if (
            Keyboard.current.leftArrowKey
                .isPressed
        )
        {
            horizontal -= 1f;
        }

        player.SetHorizontalInput(
            horizontal
        );

        if (
            Keyboard.current.upArrowKey
                .wasPressedThisFrame
        )
        {
            player.TryChangeLane(1);
        }

        if (
            Keyboard.current.downArrowKey
                .wasPressedThisFrame
        )
        {
            player.TryChangeLane(-1);
        }
    }
}