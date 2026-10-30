using System;
using System.Globalization;
using Bunker26.Party;
using Bunker26.Party.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// BA Runner integration sample. Keep this file in the BA Runner project's default gameplay
/// assembly (for example Assets/Scripts/Integrations), not inside Bunker26.Party's runtime asmdef.
/// It transfers the Party leader's device/scheme to the single Story-mode PlayerInput.
/// </summary>
[DisallowMultipleComponent]
public sealed class PartyGameplayInputAdapter : MonoBehaviour
{
    [Header("BA Runner References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerController playerController;

    [Header("Input Actions")]
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string keyboard1MoveAction = "Move_Player1";
    [SerializeField] private string keyboard2MoveAction = "Move_Player2";
    [SerializeField] private string gamepadMoveAction = "Move_Player1";
    [SerializeField] private string pauseAction = "Pause";

    [Header("Fallback Control Schemes")]
    [SerializeField] private string keyboard1Scheme = "Keyboard1";
    [SerializeField] private string keyboard2Scheme = "Keyboard2";
    [SerializeField] private string gamepadScheme = "Gamepad";
    [SerializeField] private string joystickScheme = "Joystick";

    [Header("Optional Profile Rebinding")]
    [Tooltip("Assign the same GameInputDefinition used by Party Setup to apply the leader's saved profile overrides.")]
    [SerializeField] private GameInputDefinition gameInputDefinition;
    [SerializeField] private bool applySavedProfileOverrides = true;

    private PartyService _party;
    private InputActionMap _playerMap;
    private InputAction _pause;
    private readonly System.Collections.Generic.List<InputAction> _moveActions =
        new System.Collections.Generic.List<InputAction>();

    private string _activeLeaderId;
    private string _activeSignature;
    private string _activeScheme;
    private string _activeBindingGroup;
    private bool _disabledByAdapter;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        if (playerInput == null || playerController == null)
        {
            Debug.LogError("[PartyGameplayInputAdapter] Assign PlayerInput and PlayerController.", this);
            enabled = false;
            return;
        }

        PartyRuntime runtime = PartyRuntime.GetOrCreate(8, true);
        _party = runtime.Party;
        if (_party == null)
        {
            Debug.LogError("[PartyGameplayInputAdapter] PartyRuntime has no PartyService.", this);
            enabled = false;
            return;
        }

        _playerMap = playerInput.actions != null
            ? playerInput.actions.FindActionMap(playerActionMapName, false)
            : null;
        if (_playerMap == null)
        {
            Debug.LogError("[PartyGameplayInputAdapter] Action Map not found: " + playerActionMapName, this);
            enabled = false;
            return;
        }

        SubscribeMovement(keyboard1MoveAction);
        SubscribeMovement(keyboard2MoveAction);
        SubscribeMovement(gamepadMoveAction);
        _pause = _playerMap.FindAction(pauseAction, false);
        if (_pause != null) _pause.started += HandlePause;
        else Debug.LogError("[PartyGameplayInputAdapter] Pause action not found: " + pauseAction, this);

        // Device choice is controlled by Party, not by whichever device was used most recently.
        playerInput.neverAutoSwitchControlSchemes = true;
        _party.PartyChanged += HandlePartyChanged;
        ApplyLeaderInput();
    }

    private void OnDestroy()
    {
        if (_party != null) _party.PartyChanged -= HandlePartyChanged;

        for (int i = 0; i < _moveActions.Count; i++)
        {
            InputAction action = _moveActions[i];
            if (action == null) continue;
            action.started -= HandleMove;
            action.performed -= HandleMove;
            action.canceled -= HandleMove;
        }

        if (_pause != null) _pause.started -= HandlePause;
    }

    private void SubscribeMovement(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName) || _playerMap == null) return;
        InputAction action = _playerMap.FindAction(actionName, false);
        if (action == null)
        {
            Debug.LogWarning("[PartyGameplayInputAdapter] Movement action not found: " + actionName, this);
            return;
        }
        if (_moveActions.Contains(action)) return;

        _moveActions.Add(action);
        action.started += HandleMove;
        action.performed += HandleMove;
        action.canceled += HandleMove;
    }

    private void HandlePartyChanged()
    {
        ApplyLeaderInput();
    }

    private void ApplyLeaderInput()
    {
        PartyParticipant leader = _party != null ? _party.Leader : null;
        if (leader == null)
        {
            DisableGameplayInput("There is no local Party leader.");
            return;
        }

        InputAssignment assignment = leader.inputAssignment;
        if (assignment == null || !assignment.IsAssigned)
        {
            DisableGameplayInput("The Party leader has no input assigned.");
            return;
        }

        // Resolve the physical device first. Unity may expose a controller as either Gamepad
        // or generic Joystick, even when Party stores both in its current Gamepad assignment type.
        InputDevice device;
        if (assignment.deviceKind == InputDeviceKind.Keyboard)
        {
            device = Keyboard.current;
            if (device == null && InputSystem.devices.Count > 0) device = InputSystem.devices[0];
        }
        else if (assignment.deviceKind == InputDeviceKind.Gamepad)
        {
            if (!assignment.isConnected)
            {
                DisableGameplayInput("The Party leader's assigned controller is disconnected.");
                return;
            }

            device = FindSupportedController(assignment.deviceId);
        }
        else
        {
            DisableGameplayInput("The Party leader's input type is not supported.");
            return;
        }

        if (device == null)
        {
            DisableGameplayInput("The physical device assigned to the Party leader could not be found.");
            return;
        }

        string schemeName;
        string bindingGroup;
        string error;
        if (!TryResolveScheme(assignment, out schemeName, out bindingGroup, out error))
        {
            DisableGameplayInput(error);
            return;
        }

        // A generic HID Joystick does not match a control scheme that requires <Gamepad>.
        // Give it its own control scheme and binding group in the Input Actions asset.
        if (assignment.deviceKind == InputDeviceKind.Gamepad && device is Joystick && !(device is Gamepad))
        {
            schemeName = joystickScheme;
            bindingGroup = joystickScheme;

            if (!HasControlScheme(schemeName))
            {
                DisableGameplayInput("The Input Actions asset has no control scheme named '" +
                    schemeName + "'. Add a Joystick control scheme and assign the Stick binding to its Joystick group.");
                return;
            }
        }

        string signature = leader.id + "|" + leader.profileId + "|" + schemeName + "|" +
                           assignment.deviceKind + "|" + assignment.deviceId + "|" + assignment.keyboardSlot;
        if (_activeLeaderId == leader.id && _activeSignature == signature &&
            playerInput.currentControlScheme == schemeName)
            return;

        try
        {
            // Stop horizontal momentum from stale input before changing ownership.
            playerController.SetHorizontalInput(0f);
            if (assignment.deviceKind == InputDeviceKind.Keyboard && Mouse.current != null)
                playerInput.SwitchCurrentControlScheme(schemeName, device, Mouse.current);
            else
                playerInput.SwitchCurrentControlScheme(schemeName, device);

            if (applySavedProfileOverrides)
                ApplyLeaderOverrides(leader, schemeName);

            _activeLeaderId = leader.id;
            _activeSignature = signature;
            _activeScheme = schemeName;
            _activeBindingGroup = bindingGroup;

            if (_disabledByAdapter)
            {
                playerInput.ActivateInput();
                _disabledByAdapter = false;
            }

            Debug.Log("[PartyGameplayInputAdapter] Active leader: " + leader.DisplayName +
                      " | Device: " + device.displayName + " (" + device.GetType().Name + ")" +
                      " | Scheme: " + schemeName + " | Group: " + bindingGroup, this);
        }
        catch (Exception exception)
        {
            _activeLeaderId = null;
            _activeSignature = null;
            DisableGameplayInput("Could not activate leader input scheme '" + schemeName + "'. " + exception.Message);
        }
    }

    private bool HasControlScheme(string schemeName)
    {
        if (playerInput == null || playerInput.actions == null || string.IsNullOrWhiteSpace(schemeName))
            return false;

        for (int i = 0; i < playerInput.actions.controlSchemes.Count; i++)
        {
            if (string.Equals(playerInput.actions.controlSchemes[i].name, schemeName,
                    StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private bool TryResolveScheme(InputAssignment assignment, out string schemeName,
        out string bindingGroup, out string error)
    {
        schemeName = string.Empty;
        bindingGroup = string.Empty;
        error = null;

        if (gameInputDefinition != null)
        {
            if (!gameInputDefinition.TryResolveScheme(assignment, out schemeName,
                    out bindingGroup, out _, out error))
                return false;
        }
        else
        {
            switch (assignment.deviceKind)
            {
                case InputDeviceKind.Keyboard:
                    schemeName = assignment.keyboardSlot == 0 ? keyboard1Scheme : keyboard2Scheme;
                    bindingGroup = schemeName;
                    break;
                case InputDeviceKind.Gamepad:
                    schemeName = gamepadScheme;
                    bindingGroup = schemeName;
                    break;
                default:
                    error = "Unsupported input assignment type.";
                    return false;
            }
        }

        if (playerInput.actions == null)
        {
            error = "PlayerInput has no InputActionAsset.";
            return false;
        }

        bool schemeExists = false;
        for (int i = 0; i < playerInput.actions.controlSchemes.Count; i++)
        {
            if (string.Equals(playerInput.actions.controlSchemes[i].name, schemeName,
                    StringComparison.OrdinalIgnoreCase))
            {
                schemeExists = true;
                break;
            }
        }
        if (!schemeExists)
        {
            error = "PlayerInput's InputActionAsset does not contain control scheme '" + schemeName + "'.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(bindingGroup)) bindingGroup = schemeName;
        return true;
    }

    private void ApplyLeaderOverrides(PartyParticipant leader, string schemeName)
    {
        if (playerInput.actions == null || gameInputDefinition == null) return;

        // Avoid carrying the previous profile's overrides into the new leader's control scheme.
        playerInput.actions.RemoveAllBindingOverrides();
        if (string.IsNullOrWhiteSpace(leader.profileId)) return;

        IInputBindingOverridesRepository repository = new JsonInputBindingOverridesRepository();
        string json = repository.Load(leader.profileId, gameInputDefinition.gameId, schemeName);
        if (!string.IsNullOrWhiteSpace(json))
            playerInput.actions.LoadBindingOverridesFromJson(json);
    }

    private static InputDevice FindSupportedController(string deviceId)
    {
        if (!int.TryParse(deviceId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            return null;

        for (int i = 0; i < InputSystem.devices.Count; i++)
        {
            InputDevice device = InputSystem.devices[i];
            if (device == null || !device.added || device.deviceId != id)
                continue;

            if (device is Gamepad || device is Joystick)
                return device;
        }
        return null;
    }

    private void DisableGameplayInput(string reason)
    {
        _activeLeaderId = null;
        _activeSignature = null;
        _activeScheme = null;
        _activeBindingGroup = null;

        if (playerController != null) playerController.SetHorizontalInput(0f);
        if (playerInput != null && !_disabledByAdapter)
        {
            playerInput.DeactivateInput();
            _disabledByAdapter = true;
        }

        Debug.LogWarning("[PartyGameplayInputAdapter] " + reason, this);
    }

    private const float AnalogDeadzone = 0f;

    private void HandleMove(InputAction.CallbackContext context)
    {
        PartyParticipant leader = _party != null ? _party.Leader : null;
        if (leader == null || leader.id != _activeLeaderId || !ContextMatchesLeader(context, leader))
            return;

        // Keyboard bindings already produce clean digital values, so keep the existing
        // PlayerController path for them. Analog controllers need to be converted into
        // the same cardinal directions used by the keyboard:
        // Up/Down = lane change, Right = accelerate.
        if (context.control != null &&
            (context.control.device is Gamepad || context.control.device is Joystick))
        {
            HandleAnalogMove(context);
            return;
        }

        playerController.OnMove(context);
    }

    private void HandleAnalogMove(InputAction.CallbackContext context)
    {
        if (context.control == null || playerController == null)
            return;

        Vector2 rawInput = context.ReadValue<Vector2>();

        if (context.canceled || rawInput.magnitude < AnalogDeadzone)
        {
            playerController.SetHorizontalInput(0f);
            return;
        }

        // Treat the stick as a 4-direction control instead of a free analog vector.
        // The strongest axis wins, which prevents a small X component while pushing
        // Up/Down from accidentally triggering acceleration.
        Vector2 filteredInput;
        if (Mathf.Abs(rawInput.y) >= Mathf.Abs(rawInput.x))
        {
            filteredInput = new Vector2(0f, Mathf.Sign(rawInput.y));
        }
        else
        {
            filteredInput = new Vector2(Mathf.Sign(rawInput.x), 0f);
        }

        playerController.SetHorizontalInput(filteredInput.x);

        // Match PlayerController's keyboard behavior: lane changes happen once when
        // the directional action starts, rather than continuously while the stick is held.
        if (!context.started)
            return;

        if (filteredInput.y > 0f)
            playerController.TryChangeLane(1);
        else if (filteredInput.y < 0f)
            playerController.TryChangeLane(-1);
    }

    private void HandlePause(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        PartyParticipant leader = _party != null ? _party.Leader : null;
        if (leader == null || leader.id != _activeLeaderId || !ContextMatchesLeader(context, leader))
            return;

        playerController.OnPause(context);
    }

    private bool ContextMatchesLeader(InputAction.CallbackContext context, PartyParticipant leader)
    {
        if (context.control == null || leader == null || leader.inputAssignment == null)
            return false;

        InputAssignment assignment = leader.inputAssignment;
        if (playerInput.currentControlScheme != _activeScheme)
            return false;

        if (assignment.deviceKind == InputDeviceKind.Keyboard)
            return context.control.device is Keyboard && BindingHasGroup(context, _activeBindingGroup);

        if (assignment.deviceKind == InputDeviceKind.Gamepad)
        {
            InputDevice device = context.control.device;
            return (device is Gamepad || device is Joystick) &&
                   device.deviceId.ToString(CultureInfo.InvariantCulture) == assignment.deviceId &&
                   BindingHasGroup(context, _activeBindingGroup);
        }

        return false;
    }

    private static bool BindingHasGroup(InputAction.CallbackContext context, string expectedGroup)
    {
        if (string.IsNullOrWhiteSpace(expectedGroup) || context.action == null || context.control == null)
            return false;

        // CallbackContext doesn't expose the triggering binding directly in Input System 1.7.
        // Resolve the binding from the control that triggered this action instead.
        int bindingIndex = context.action.GetBindingIndexForControl(context.control);
        if (bindingIndex < 0 || bindingIndex >= context.action.bindings.Count)
            return false;

        string groups = context.action.bindings[bindingIndex].groups;
        if (string.IsNullOrWhiteSpace(groups)) return false;

        string[] values = groups.Split(';');
        for (int i = 0; i < values.Length; i++)
        {
            if (string.Equals(values[i].Trim(), expectedGroup, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
