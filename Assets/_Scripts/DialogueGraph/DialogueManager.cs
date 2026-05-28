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
    private int currentLevelIndex;
    private bool isOutro;

    private void Start()
    {
        EnterCinematics();
        continueButton.gameObject.SetActive(false);
    }


    private void EnterCinematics()
    {
        currentLevelIndex = ProgressionManager.Instance.CurrentLevelIndex;
        isOutro = GameManager.Instance.IsOutro;
        SetRuntineGraphFromLevel();
        InitializeNode();
    }

    private void SetRuntineGraphFromLevel()
    {
        Level_Scriptable currentLevel = ProgressionManager.Instance.CurrentLevel;
        if (isOutro)
        {
            CurrentRuntimeGraph = currentLevel.outroDialogueGraph;  // supongamos que tienes este campo
        }
        else
        {
            CurrentRuntimeGraph = currentLevel.introDialogueGraph;  // igual para intro
        }
    }

    private void InitializeNode()
    {
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

                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                        {
                            ShowDialogue(choice.DestinationNodeID);
                        }
                        else
                        {
                            AdvanceNode();
                        }
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
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            OnContinuePressed();
        });
        currentNode = null;

        foreach (Transform child in choiceButtonContainer) { Destroy(child.gameObject); }
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
        if (Mouse.current.leftButton.wasPressedThisFrame && currentNode != null && currentNode.Choices.Count == 0)
        {
            AdvanceNode();
        }
    }

}
