using System;
using System.Globalization;
using Bunker26.Party.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bunker26.Party.Samples
{
    /// <summary>
    /// Composition root for the sample scene. Services are created here; all UI, prefabs and icons
    /// are referenced through the Inspector rather than built at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PartySetupController : MonoBehaviour
    {
        [Header("Presentation (assign in Inspector)")]
        public PartySetupScreenView screenView;
        public PartyIconCatalog iconCatalog;
        public InputSystemDeviceMonitor inputDeviceMonitor;
        public GameInputDefinition gameInputDefinition;

        [Header("Party Settings")]
        [Min(1)] public int maxLocalPlayers = 8;
        public bool createLocalPlayerOneOnAwake = true;

        private PlayerProfileService _profiles;
        private PartyService _party;
        private InputRebindingService _inputRebindingService;
        private string _statusMessage = "Listo. Seleccioná un perfil o uní un mando.";

        public PartyService Party { get { return _party; } }
        public PlayerProfileService Profiles { get { return _profiles; } }
        public string StatusMessage { get { return _statusMessage; } }
        public InputSystemDeviceMonitor DeviceMonitor { get { return inputDeviceMonitor; } }
        public GameInputDefinition GameInputDefinition { get { return gameInputDefinition; } }

        private void Awake()
        {
            PartyRuntime runtime = PartyRuntime.GetOrCreate(maxLocalPlayers, createLocalPlayerOneOnAwake);
            _profiles = runtime.Profiles;
            _party = runtime.Party;
            _inputRebindingService = runtime.InputRebinding;

            if (inputDeviceMonitor == null)
                inputDeviceMonitor = GetComponent<InputSystemDeviceMonitor>();
        }

        private void Start()
        {
            _party.PartyChanged += HandlePartyChanged;
            if (inputDeviceMonitor != null)
            {
                inputDeviceMonitor.KeyboardButtonPressed += HandleKeyboardButtonPressed;
                inputDeviceMonitor.ControllerButtonPressed += HandleControllerButtonPressed;
                inputDeviceMonitor.DeviceChanged += HandleInputDeviceChanged;
            }
            if (screenView != null)
            {
                screenView.Initialize(this, iconCatalog);
                screenView.RefreshAll();
            }

            PartyParticipant playerOne = _party.GetParticipantByLocalSlot(1);
            bool hasRememberedProfile = playerOne != null && !string.IsNullOrWhiteSpace(playerOne.profileId);
            string playerName = hasRememberedProfile ? playerOne.DisplayName : "Unknown";
            SetStatus(playerName + (hasRememberedProfile ? " recuperado." : "") +
                " Presioná una tecla o un botón de un mando para asignar el input inicial. Usá + para sumar otros jugadores.", false);
        }

        private void OnDestroy()
        {
            if (_party != null) _party.PartyChanged -= HandlePartyChanged;
            if (inputDeviceMonitor != null)
            {
                inputDeviceMonitor.KeyboardButtonPressed -= HandleKeyboardButtonPressed;
                inputDeviceMonitor.ControllerButtonPressed -= HandleControllerButtonPressed;
                inputDeviceMonitor.DeviceChanged -= HandleInputDeviceChanged;
            }
        }

        public bool TryCreateInputRebindingSession(string participantId, out InputRebindingSession session, out string error)
        {
            session = null;
            error = null;
            PartyParticipant participant = _party != null ? _party.GetParticipant(participantId) : null;
            if (participant == null) { error = "No se encontró el jugador seleccionado."; return false; }
            if (string.IsNullOrWhiteSpace(participant.profileId))
            { error = "Elegí un perfil antes de personalizar los controles."; return false; }
            if (participant.inputAssignment == null || !participant.inputAssignment.IsAssigned)
            { error = "Asigná un dispositivo antes de personalizar los controles."; return false; }
            if (participant.inputAssignment.deviceKind == InputDeviceKind.Gamepad && !participant.inputAssignment.isConnected)
            { error = "El gamepad asignado está desconectado. Reconectalo para editar sus controles."; return false; }
            if (gameInputDefinition == null)
            { error = "Asigná GameInputDefinition en el Inspector para habilitar la personalización de controles."; return false; }
            if (!gameInputDefinition.TryResolveScheme(participant.inputAssignment, out string schemeName, out string bindingGroup,
                    out RebindDeviceFamily family, out error)) return false;

            session = _inputRebindingService.CreateSession(participant.profileId, gameInputDefinition,
                schemeName, bindingGroup, family, out error);
            return session != null;
        }

        public bool TryAddLocalPlayer(out PartyParticipant participant, out string error)
        {
            bool result = _party.TryAddLocalPlayer(out participant, out error);
            SetStatus(result ? "Jugador local creado. Elegí Teclado 1 o 2, o presioná un botón del mando que quieras asignar." : error, !result);
            return result;
        }

        public bool TrySetLeader(string participantId, out string error)
        {
            if (_party == null)
            {
                error = "La Party no está inicializada.";
                SetStatus(error, true);
                return false;
            }

            bool result = _party.TrySetLeader(participantId, out error);
            SetStatus(result ? "Líder cambiado. Se actualizarán los controles activos." : error, !result);
            return result;
        }

        public bool TryAssignProfile(string participantId, string profileId, out string error)
        {
            bool result = _party.AssignProfile(participantId, profileId, out error);
            if (result) SetStatus("Perfil asignado. La selección preferida se guardó para este jugador local.", false);
            else SetStatus(error, true);
            return result;
        }

        public bool ClearProfile(string participantId)
        {
            bool result = _party.ClearProfile(participantId);
            SetStatus(result ? "Perfil quitado. El jugador ahora aparece como Unknown." : "No se encontró el jugador.", !result);
            return result;
        }

        public PlayerProfile CreateProfile(string name, string iconId)
        {
            PlayerProfile profile = _profiles.Create(name, iconId);
            SetStatus("Perfil creado y guardado. Podés asignarlo a un jugador.", false);
            return profile;
        }

        public bool SaveProfile(PlayerProfile profile, out string error)
        {
            error = null;
            if (profile == null) { error = "No se encontró el perfil para guardar."; SetStatus(error, true); return false; }
            try
            {
                _profiles.Save(profile);
                _party.RefreshProfile(profile.id);
                SetStatus("Cambios del perfil guardados.", false);
                return true;
            }
            catch (Exception exception)
            {
                error = "No se pudo guardar el perfil: " + exception.Message;
                SetStatus(error, true);
                return false;
            }
        }

        public bool AssignKeyboardSlot(string participantId, int slot, out string error)
        {
            bool result = _party.AssignInput(participantId, InputAssignment.Keyboard(slot), out error);
            SetStatus(result ? "Asignado a Teclado " + (slot + 1) + "." : error, !result);
            return result;
        }

        public bool AssignControllerInput(string participantId, string deviceId, string displayName, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                error = "No se identificó el mando seleccionado.";
                SetStatus(error, true);
                return false;
            }

            PartyParticipant assignedParticipant = null;
            var participants = _party != null ? _party.Participants : null;
            if (participants != null)
            {
                for (int i = 0; i < participants.Count; i++)
                {
                    PartyParticipant candidate = participants[i];
                    if (candidate != null && candidate.inputAssignment != null &&
                        candidate.inputAssignment.deviceKind == InputDeviceKind.Gamepad &&
                        candidate.inputAssignment.deviceId == deviceId)
                    {
                        assignedParticipant = candidate;
                        break;
                    }
                }
            }

            if (assignedParticipant != null && assignedParticipant.id != participantId)
            {
                error = "Ese mando ya está asignado a otro jugador.";
                SetStatus(error, true);
                return false;
            }

            string name = string.IsNullOrWhiteSpace(displayName) ? "Mando" : displayName;
            bool result = _party.AssignInput(participantId, InputAssignment.Gamepad(deviceId, name, true), out error);
            SetStatus(result ? name + " asignado al jugador seleccionado." : error, !result);
            return result;
        }

        public bool ReleaseInput(string participantId)
        {
            bool result = _party.ReleaseInput(participantId);
            SetStatus(result ? "Input liberado." : "No se encontró el jugador.", !result);
            return result;
        }

        public bool RemoveParticipant(string participantId)
        {
            bool result = _party.RemoveParticipant(participantId);
            SetStatus(result ? "Jugador local quitado de la Party." : "El jugador líder no se puede eliminar; podés cambiar su perfil o input.", !result);
            return result;
        }

        public bool IsKeyboardSlotAvailable(int slot, string targetParticipantId)
        {
            return _party.IsKeyboardSlotAvailable(slot, targetParticipantId);
        }

        public bool IsProfileAssignedToOtherParticipant(string profileId, string targetParticipantId)
        {
            var participants = _party.Participants;
            for (int i = 0; i < participants.Count; i++)
            {
                PartyParticipant participant = participants[i];
                if (participant.id != targetParticipantId && participant.type == PartyParticipantType.LocalHuman && participant.profileId == profileId)
                    return true;
            }
            return false;
        }

        public string GetConnectedGamepadSummary()
        {
            if (inputDeviceMonitor == null) return "Input System monitor no asignado en el Inspector.";
            var gamepads = inputDeviceMonitor.ConnectedControllers;
            if (gamepads.Count == 0) return "No hay mandos conectados. Conectá uno y presioná cualquier botón para entrar.";
            string summary = "Mandos conectados: ";
            bool hasFree = false;
            for (int i = 0; i < gamepads.Count; i++)
            {
                if (i > 0) summary += ", ";
                string id = gamepads[i].deviceId.ToString(CultureInfo.InvariantCulture);
                bool assigned = _party != null && _party.IsGamepadAssigned(id);
                summary += (string.IsNullOrWhiteSpace(gamepads[i].displayName) ? "Gamepad" : gamepads[i].displayName) + (assigned ? " (en uso)" : " (libre)");
                if (!assigned) hasFree = true;
            }
            summary += hasFree ? ". Presioná un botón en un mando libre para unirte." : ". Todos los mandos están asignados.";
            return summary;
        }

        public void SetStatus(string message, bool isError)
        {
            _statusMessage = message ?? string.Empty;
            if (screenView != null) screenView.SetStatus(_statusMessage, isError);
        }

        private void HandlePartyChanged()
        {
            if (screenView != null) screenView.RefreshAll();
        }

        private void HandleKeyboardButtonPressed(Keyboard keyboard, InputControl control)
        {
            if (keyboard == null || _party == null) return;

            // A keyboard press in a modal is part of the current UI workflow (profile editing,
            // rebinding, or explicit input selection), not an implicit assignment.
            if (screenView != null && (screenView.IsModalPanelOpen || !string.IsNullOrEmpty(screenView.PendingInputTargetId)))
                return;

            PartyParticipant playerOne = _party.GetParticipantByLocalSlot(1);
            if (playerOne == null || (playerOne.inputAssignment != null && playerOne.inputAssignment.IsAssigned))
                return;

            string error;
            if (!_party.AssignInput(playerOne.id, InputAssignment.Keyboard(0), out error))
            {
                SetStatus(error, true);
                return;
            }

            bool needsProfile = string.IsNullOrWhiteSpace(playerOne.profileId);
            SetStatus(needsProfile
                ? "Teclado 1 asignado. Elegí o creá un perfil para Player 1."
                : "Teclado 1 asignado a " + playerOne.DisplayName + ".", false);

            if (needsProfile && screenView != null)
                screenView.OpenProfileSelection(playerOne.id);
        }

        private void HandleControllerButtonPressed(InputDevice controllerDevice)
        {
            if (!InputSystemDeviceMonitor.IsSupportedController(controllerDevice) || controllerDevice == null || !controllerDevice.added)
                return;

            string deviceId = controllerDevice.deviceId.ToString(CultureInfo.InvariantCulture);
            string displayName = string.IsNullOrWhiteSpace(controllerDevice.displayName) ? "Mando" : controllerDevice.displayName;
            string targetId = screenView != null ? screenView.PendingInputTargetId : string.Empty;

            // If the user has explicitly opened the selector, controller activity assigns this
            // device to the selected participant and must not trigger an implicit join.
            if (!string.IsNullOrEmpty(targetId))
            {
                PartyParticipant target = _party.GetParticipant(targetId);
                bool alreadyAssignedToTarget = target != null && target.inputAssignment != null &&
                    target.inputAssignment.deviceKind == InputDeviceKind.Gamepad && target.inputAssignment.deviceId == deviceId;

                if (_party.IsGamepadAssigned(deviceId) && !alreadyAssignedToTarget)
                {
                    _party.MarkGamepadConnected(deviceId, displayName);
                    SetStatus("Ese mando ya está asignado a otro jugador.", true);
                    if (screenView != null) screenView.RefreshDeviceStatus();
                    return;
                }

                if (alreadyAssignedToTarget)
                {
                    _party.MarkGamepadConnected(deviceId, displayName);
                    if (screenView != null) screenView.CompleteInputAssignment(targetId);
                    SetStatus(displayName + " ya estaba asignado a ese jugador.", false);
                    return;
                }

                string assignmentError;
                if (_party.AssignInput(targetId, InputAssignment.Gamepad(deviceId, displayName, true), out assignmentError))
                {
                    if (screenView != null) screenView.CompleteInputAssignment(targetId);
                    SetStatus(displayName + " asignado al jugador seleccionado.", false);
                }
                else SetStatus(assignmentError, true);
                return;
            }

            // Never claim a controller while another modal is being operated.
            if (screenView != null && screenView.IsModalPanelOpen) return;

            // Already-assigned devices only refresh their connection status; they don't create a
            // duplicate participant. A free device may join the next available local slot.
            if (_party.IsGamepadAssigned(deviceId))
            {
                _party.MarkGamepadConnected(deviceId, displayName);
                return;
            }

            PartyParticipant participant;
            bool shouldOpenProfileSelection;
            string joinError;
            if (_party.TryJoinWithGamepad(deviceId, displayName, out participant, out shouldOpenProfileSelection, out joinError))
            {
                bool hasProfile = participant != null && !string.IsNullOrWhiteSpace(participant.profileId);
                SetStatus(displayName + " asignado a " + (hasProfile ? participant.DisplayName : "Jugador " + participant.localSlotIndex) +
                    (hasProfile ? "." : ". Elegí o creá un perfil."), false);

                if (screenView != null)
                {
                    screenView.RefreshAll();
                    if (shouldOpenProfileSelection && participant != null)
                        screenView.OpenProfileSelection(participant.id);
                }
            }
            else
            {
                SetStatus(joinError, true);
            }
        }

        private void HandleInputDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if (device == null)
            {
                if (screenView != null) screenView.RefreshDeviceStatus();
                return;
            }

            if (!InputSystemDeviceMonitor.IsSupportedController(device)) return;
            string deviceId = device.deviceId.ToString(CultureInfo.InvariantCulture);
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Enabled:
                case InputDeviceChange.ConfigurationChanged:
                    _party.MarkGamepadConnected(deviceId, device.displayName);
                    break;
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disabled:
                    _party.MarkGamepadDisconnected(deviceId);
                    break;
            }
            if (screenView != null) screenView.RefreshDeviceStatus();
        }
    }
}
