using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Nodes")]
    [SerializeField] private List<LevelNodeUI> levelNodes = new List<LevelNodeUI>();

    [Header("Connections")]
    [SerializeField] private List<LevelConnectionUI> levelConnections = new List<LevelConnectionUI>();

    private void Start()
    {
        InitializeNodes();
        RefreshLevelSelector();
    }

    private void OnEnable()
    {
        RefreshLevelSelector();
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

        if (ProgressionManager.Instance.CurrentLevel == level &&
            !ProgressionManager.Instance.IsLevelCompleted(level))
            return LevelNodeState.Current;

        if (ProgressionManager.Instance.IsLevelCompleted(level))
            return LevelNodeState.Completed;

        return LevelNodeState.Unlocked;
    }

    public void SelectLevel(Level_Scriptable level)
    {
        if (level == null)
            return;

        if (!ProgressionManager.Instance.IsLevelDiscovered(level))
            return;

        ProgressionManager.Instance.LoadLevel(level);
    }
}
