using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Nodes")]
    [SerializeField]
    private List<LevelNodeUI> levelNodes = new List<LevelNodeUI>();

    [Header("Connections")]
    [SerializeField]
    private List<LevelConnectionUI> levelConnections =
        new List<LevelConnectionUI>();

    private Coroutine initialSelectionRoutine;
    private bool hasStarted;

    private void Awake()
    {
        InitializeNodes();
    }

    private void Start()
    {
        hasStarted = true;

        RefreshLevelSelector();
        RequestInitialSelection();
    }

    private void OnEnable()
    {
        // OnEnable se ejecuta antes de Start al cargar la escena.
        // Por eso evitamos inicializar la selección dos veces.
        if (!hasStarted)
            return;

        RefreshLevelSelector();
        RequestInitialSelection();
    }

    private void OnDisable()
    {
        if (initialSelectionRoutine != null)
        {
            StopCoroutine(initialSelectionRoutine);
            initialSelectionRoutine = null;
        }
    }

    private void InitializeNodes()
    {
        foreach (LevelNodeUI node in levelNodes)
        {
            if (node == null)
                continue;

            node.Initialize(this);
        }
    }

    public void RefreshLevelSelector()
    {
        if (ProgressionManager.Instance == null)
            return;

        foreach (LevelNodeUI node in levelNodes)
        {
            if (node == null || node.Level == null)
                continue;

            LevelNodeState state = GetStateForLevel(node.Level);

            node.SetState(state);
        }

        foreach (LevelConnectionUI connection in levelConnections)
        {
            if (connection == null)
                continue;

            connection.Refresh();
        }
    }

    private LevelNodeState GetStateForLevel(Level_Scriptable level)
    {
        if (!ProgressionManager.Instance.IsLevelDiscovered(level))
            return LevelNodeState.Hidden;

        if (
            ProgressionManager.Instance.CurrentLevel == level &&
            !ProgressionManager.Instance.IsLevelCompleted(level)
        )
        {
            return LevelNodeState.Current;
        }

        if (ProgressionManager.Instance.IsLevelCompleted(level))
            return LevelNodeState.Completed;

        return LevelNodeState.Unlocked;
    }

    private void RequestInitialSelection()
    {
        if (!isActiveAndEnabled)
            return;

        if (initialSelectionRoutine != null)
        {
            StopCoroutine(initialSelectionRoutine);
        }

        initialSelectionRoutine =
            StartCoroutine(SelectInitialLevelRoutine());
    }

    private IEnumerator SelectInitialLevelRoutine()
    {
        // Esperamos un frame para que SetState pueda activar,
        // desactivar o modificar los botones.
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (EventSystem.current == null)
        {
            Debug.LogWarning(
                "No se encontró un EventSystem en la escena."
            );

            initialSelectionRoutine = null;
            yield break;
        }

        LevelNodeUI targetNode = FindPreferredNode();

        if (targetNode == null)
        {
            Debug.LogWarning(
                "No se encontró ningún LevelNodeUI seleccionable."
            );

            initialSelectionRoutine = null;
            yield break;
        }

        Button targetButton = GetNavigationButton(targetNode);

        if (targetButton == null)
        {
            initialSelectionRoutine = null;
            yield break;
        }

        // Limpiamos la selección anterior, especialmente útil
        // cuando el selector se abre como un panel.
        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(
            targetButton.gameObject
        );

        initialSelectionRoutine = null;
    }

    private LevelNodeUI FindPreferredNode()
    {
        Level_Scriptable preferredLevel = null;

        if (ProgressionManager.Instance != null)
        {
            preferredLevel =
                ProgressionManager.Instance.CurrentLevel;
        }

        // Primero intentamos seleccionar el nivel actual
        // o el último nivel guardado.
        if (preferredLevel != null)
        {
            foreach (LevelNodeUI node in levelNodes)
            {
                if (
                    node != null &&
                    node.Level == preferredLevel &&
                    IsNodeSelectable(node)
                )
                {
                    return node;
                }
            }
        }

        // Si no existe un nivel actual seleccionable,
        // usamos el primero disponible de la lista.
        foreach (LevelNodeUI node in levelNodes)
        {
            if (IsNodeSelectable(node))
            {
                return node;
            }
        }

        return null;
    }

    private bool IsNodeSelectable(LevelNodeUI node)
    {
        if (node == null)
            return false;

        if (!node.gameObject.activeInHierarchy)
            return false;

        Button button = GetNavigationButton(node);

        if (button == null)
            return false;

        if (!button.gameObject.activeInHierarchy)
            return false;

        return button.IsInteractable();
    }

    private Button GetNavigationButton(LevelNodeUI node)
    {
        if (node == null)
            return null;

        MenuButton menuButton =
            node.GetComponentInChildren<MenuButton>(true);

        if (menuButton == null)
            return null;

        return menuButton.Button;
    }

    public void SelectLevel(Level_Scriptable level)
    {
        if (level == null)
            return;

        if (ProgressionManager.Instance == null)
            return;

        if (!ProgressionManager.Instance.IsLevelDiscovered(level))
            return;

        ProgressionManager.Instance.LoadLevel(level);
    }
}