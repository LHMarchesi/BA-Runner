using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DeveloperConsole : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_InputField input;
    [SerializeField] private InputActionReference toggleConsoleAction;

    private bool isOpen;

    private void OnEnable()
    {
        toggleConsoleAction.action.Enable();
        toggleConsoleAction.action.performed += OnToggleConsole;

        input.onSubmit.AddListener(_ => ExecuteCommand());
    }

    private void OnDisable()
    {
        toggleConsoleAction.action.performed -= OnToggleConsole;
        toggleConsoleAction.action.Disable();
    }

    private void OnToggleConsole(InputAction.CallbackContext context)
    {
        isOpen = !isOpen;
        panel.SetActive(isOpen);

        if (isOpen)
        {
            input.text = "";
            input.ActivateInputField();
        }
    }
  

    public void ExecuteCommand()
    {
        string command = input.text;

        DeveloperCommands.Execute(command);

        input.text = "";
        input.ActivateInputField();
    }
}

public static class DeveloperCommands
{
    public static void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        string[] args = command.Split(' ');

        switch (args[0].ToLower())
        {
            case "level":

                if (args.Length < 2)
                {
                    Debug.Log("Uso: level NombreDelNivel");
                    return;
                }

                ProgressionManager.Instance.LoadLevel(args[1]);
                break;

            case "win":

                EventBus<OnLevelCompletedEvent>.Raise(
                    new OnLevelCompletedEvent());

                break;

            case "lose":

                EventBus<OnPlayerDeathEvent>.Raise(
                    new OnPlayerDeathEvent());

                break;

            default:

                Debug.Log($"Comando desconocido: {args[0]}");
                break;
        }
    }
}
