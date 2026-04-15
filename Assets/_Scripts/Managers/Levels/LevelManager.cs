using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private ProgessBar progessBar;
    [SerializeField] private List<Level_Scriptable> levels;

    private int currentLevelIndex;
    private float levelProgession;

    public static Action OnLevelComplete;
    private Level_Scriptable CurrentLevel => levels[currentLevelIndex];

    private void Start()
    {
        LoadLevel(0);
        levelProgession = 0;
    }

    void LoadLevel(int index)
    {
        currentLevelIndex = index;
        levelProgession = 0;
        progessBar.UpdateProgess(levelProgession, CurrentLevel.maxLevelProgession);
        AudioManager.Instance.PlayMusic(CurrentLevel.levelMusic);
    }


    public void IncreaseLevelProgession()
    {
        if (levelProgession < CurrentLevel.maxLevelProgession)
        {
            levelProgession += Time.deltaTime;
            progessBar.UpdateProgess(levelProgession, CurrentLevel.maxLevelProgession);
        }
        else
        {
            OnLevelComplete?.Invoke();
            NextLevel();
        }
    }

    void NextLevel()
    {
        if (currentLevelIndex + 1 < levels.Count)
        {
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
            Debug.Log("Juego completado");
        }
    }


    private void Update()
    {
        IncreaseLevelProgession();
    }
}
