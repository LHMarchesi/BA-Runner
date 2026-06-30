using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour

//Necesito que si es el ultimo dialogo muestre boton de continue, ademas este DialogueManager tendria que tener saber que nivel esta jugando y de ahi tomar el RuntimeGraph

{
    public RuntimeDialogueGraph CurrentRuntimeGraph;

    [Header("UIComponents")]
    public GameObject dialoguePanel;
    public Button continueButton;
    public Image backgroundImage;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;
    public Button choiceButtonPrefab;
    public Transform choiceButtonContainer;

    private Dictionary<string, RuntimeDialogueNode> nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode currentNode;

    public Slider delayProgressBar;
    private Coroutine autoAdvanceCoroutine;
    private bool isOutro;
    private bool dialogueEnded;

    private void Start()
    {
        EnterCinematics();
        continueButton.gameObject.SetActive(false);
        delayProgressBar.gameObject.SetActive(false);
    }


    private void EnterCinematics()
    {
        isOutro = GameManager.Instance.IsOutro;

        SetRuntineGraphFromLevel();
        PlayCinematicMusic();
        if (CurrentRuntimeGraph == null)
        {
            if (isOutro)
            {
                ProgressionManager.Instance.AdvanceLevel();
            }
            else
            {
                LoadGameplay();
            }

            return;
        }

        InitializeNode();
    }

    private void SetRuntineGraphFromLevel()
    {
        Level_Scriptable currentLevel = ProgressionManager.Instance.CurrentLevel;

        if (currentLevel == null)
        {
            CurrentRuntimeGraph = null;
            return;
        }

        CurrentRuntimeGraph = isOutro
            ? currentLevel.outroDialogueGraph
            : currentLevel.introDialogueGraph;
    }

    private void InitializeNode()
    {
        delayProgressBar.gameObject.SetActive(true);
        if (CurrentRuntimeGraph == null)
        {
            EndDialogue();
            return;
        }

        dialogueEnded = false;
        nodeLookup.Clear();

        foreach (var node in CurrentRuntimeGraph.AllNodes)
        {
            nodeLookup[node.NodeID] = node;
        }

        if (!string.IsNullOrEmpty(CurrentRuntimeGraph.EntryNodeID))
        {
            ShowDialogue(CurrentRuntimeGraph.EntryNodeID);
        }
        else
        {
            EndDialogue();
        }
    }


    private void ShowDialogue(string entryNodeID)
    {
        if (!nodeLookup.ContainsKey(entryNodeID))
        {
            EndDialogue();
            return;
        }

        currentNode = nodeLookup[entryNodeID];

        dialoguePanel.SetActive(true);
        backgroundImage.sprite = currentNode.Image;
        backgroundImage.gameObject.SetActive(true);
        SpeakerNameText.text = currentNode.SpeakerName;
        DialogueText.text = currentNode.DialogueText;

        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentNode.Choices.Count > 0)
        {
            foreach (var choice in currentNode.Choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choice.ChoiceText;
                }
                ChoiceData capturedChoice = choice;
                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log($"[Choice] Elegida: '{capturedChoice.ChoiceText}' | FlagsToSet: [{string.Join(", ", capturedChoice.FlagsToSet)}]");
                        ProgressionManager.Instance.ApplyChoice(capturedChoice);
                        Debug.Log($"[Flags] HasFlag {capturedChoice.FlagsToSet}: {ProgressionManager.Instance.HasFlag(capturedChoice.FlagsToSet[0])}");
                        AdvanceNodeByChoice(capturedChoice);
                    });
                }
            }
        }
        else
        {
            if (currentNode.Delay > 0f)
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine(currentNode.Delay));
        }
    }

    private void EndDialogue()
    {
        continueButton.gameObject.SetActive(true);
        dialogueEnded = true;
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            OnContinuePressed();
        });
        currentNode = null;

        foreach (Transform child in choiceButtonContainer) { Destroy(child.gameObject); }
    }

    private void AdvanceNodeByChoice(ChoiceData choice)
    {
        StopAutoAdvance();
        string destination = !string.IsNullOrEmpty(choice.DestinationNodeID)
            ? choice.DestinationNodeID
            : currentNode.NextNodeID; // fallback al nodo siguiente lineal

        if (!string.IsNullOrEmpty(destination))
            ShowDialogue(destination);
        else
            EndDialogue();
    }
    private void AdvanceNode()
    {
        StopAutoAdvance();
        if (currentNode == null) return;

        if (!string.IsNullOrEmpty(currentNode.NextNodeID))
            ShowDialogue(currentNode.NextNodeID);
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator AutoAdvanceCoroutine(float delay)
    {
        float elapsed = 0f;

        if (delayProgressBar != null)
        {
            delayProgressBar.gameObject.SetActive(true);
            delayProgressBar.value = 0f;
        }

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            if (delayProgressBar != null)
                delayProgressBar.value = elapsed / delay;
            yield return null;
        }

        if (delayProgressBar != null)
            delayProgressBar.gameObject.SetActive(false);

        AdvanceNode();
    }
    private void PlayCinematicMusic()
    {
        var level = ProgressionManager.Instance.CurrentLevel;

        if (level == null)
            return;

        AudioClip clip =
            isOutro
            ? level.outroMusic
            : level.introMusic;

        AudioManager.Instance.PlayMusic(clip);
    }
    private void StopAutoAdvance()
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
        if (delayProgressBar != null)
            delayProgressBar.gameObject.SetActive(false);
    }


    public void OnContinuePressed()
    {
        if (isOutro)
        {
            ProgressionManager.Instance.AdvanceLevel();
        }
        else
        {
            LoadGameplay();
        }
    }

    void LoadGameplay()
    {
        SceneManager.LoadScene("SampleScene"); // tu escena real
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        bool mousePressed =
        Mouse.current != null &&
        Mouse.current.leftButton.wasPressedThisFrame;

        bool keyPressed =
            Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame;

        if (!mousePressed && !keyPressed)
            return;

        if (dialogueEnded)
        {
            OnContinuePressed();
            return;
        }

        if (currentNode == null)
            return;

        if (currentNode.Choices.Count == 0)
        {
            if (string.IsNullOrEmpty(currentNode.NextNodeID))
                OnContinuePressed();
            else
                AdvanceNode();
        }
    }
}
