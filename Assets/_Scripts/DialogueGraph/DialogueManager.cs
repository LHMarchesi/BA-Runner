using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public RuntimeDialogueGraph CurrentRuntimeGraph;

    [Header("UI Components")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI SpeakerNameText;
    [SerializeField] private TextMeshProUGUI DialogueText;

    [Header("Choice Buttons")]
    [SerializeField] private RectTransform choiceButtonContainer;
    [SerializeField] private List<Button> choiceButtons = new List<Button>();

    [Header("Delay")]
    [SerializeField] private Slider delayProgressBar;

    private Dictionary<string, RuntimeDialogueNode> nodeLookup = new Dictionary<string, RuntimeDialogueNode>();

    private RuntimeDialogueNode currentNode;

    private Coroutine autoAdvanceCoroutine;
    private Coroutine selectRoutine;

    private bool isOutro;
    private bool dialogueEnded;
    private bool isTransitioning;

    private void Start()
    {
        continueButton.gameObject.SetActive(false);

       if (delayProgressBar != null)
            delayProgressBar.gameObject.SetActive(false);

        ClearChoiceButtons();

        EnterCinematics();
    }

    private void EnterCinematics()
    {
        isOutro = GameManager.Instance.IsOutro;

        SetRuntimeGraphFromLevel();
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

    private void SetRuntimeGraphFromLevel()
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
        if (CurrentRuntimeGraph == null)
        {
            EndDialogue();
            return;
        }

        dialogueEnded = false;
        isTransitioning = false;

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

    private void ShowDialogue(string nodeID)
    {
        if (!nodeLookup.ContainsKey(nodeID))
        {
            EndDialogue();
            return;
        }

        StopAutoAdvance();

        currentNode = nodeLookup[nodeID];
        dialogueEnded = false;

        dialoguePanel.SetActive(true);

        continueButton.gameObject.SetActive(false);

        if (backgroundImage != null)
        {
            backgroundImage.sprite = currentNode.Image;
            backgroundImage.gameObject.SetActive(currentNode.Image != null);
        }

        if (SpeakerNameText != null)
            SpeakerNameText.text = currentNode.SpeakerName;

        if (DialogueText != null)
            DialogueText.text = currentNode.DialogueText;

        ClearChoiceButtons();

        bool hasChoices =
            currentNode.Choices != null &&
            currentNode.Choices.Count > 0;

        if (hasChoices)
        {
            SetupChoiceButtons(currentNode.Choices);
        }
        else
        {
            if (currentNode.Delay > 0f)
            {
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine(currentNode.Delay));
            }
        }
    }

    private void SetupChoiceButtons(List<ChoiceData> choices)
    {
        ClearChoiceButtons();

        int activeButtonCount = Mathf.Min(choices.Count, choiceButtons.Count);

        if (choices.Count > choiceButtons.Count)
        {
            Debug.LogWarning(
                $"Hay más choices ({choices.Count}) que botones disponibles ({choiceButtons.Count}). " +
                $"Solo se mostrarán {choiceButtons.Count}."
            );
        }

        for (int i = 0; i < activeButtonCount; i++)
        {
            ChoiceData capturedChoice = choices[i];
            Button button = choiceButtons[i];

            button.gameObject.SetActive(true);
            button.interactable = true;
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                Debug.Log($"[Choice] Elegida: '{capturedChoice.ChoiceText}'");

                ProgressionManager.Instance.ApplyChoice(capturedChoice);

                if (capturedChoice.FlagsToSet != null && capturedChoice.FlagsToSet.Count > 0)
                {
                    Debug.Log(
                        $"[Flags] HasFlag {capturedChoice.FlagsToSet[0]}: " +
                        $"{ProgressionManager.Instance.HasFlag(capturedChoice.FlagsToSet[0])}"
                    );
                }

                AdvanceNodeByChoice(capturedChoice);
            });
        }

        SetupExplicitNavigation(activeButtonCount);

        if (activeButtonCount > 0)
        {
            SelectAfterLayout(choiceButtons[0].gameObject);
        }
    }

    private void SetupExplicitNavigation(int activeButtonCount)
    {
        if (activeButtonCount <= 0)
            return;

        for (int i = 0; i < activeButtonCount; i++)
        {
            Button current = choiceButtons[i];

            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.Explicit;

            Button previous = choiceButtons[(i - 1 + activeButtonCount) % activeButtonCount];
            Button next = choiceButtons[(i + 1) % activeButtonCount];

            navigation.selectOnUp = previous;
            navigation.selectOnLeft = previous;
            navigation.selectOnDown = next;
            navigation.selectOnRight = next;

            current.navigation = navigation;
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (Button button in choiceButtons)
        {
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    private void EndDialogue()
    {
        StopAutoAdvance();

        dialogueEnded = true;
        currentNode = null;

        ClearChoiceButtons();

        continueButton.gameObject.SetActive(true);
        continueButton.interactable = true;

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinuePressed);

        SelectAfterLayout(continueButton.gameObject);
    }

    private void AdvanceNodeByChoice(ChoiceData choice)
    {
        StopAutoAdvance();

        string destination = !string.IsNullOrEmpty(choice.DestinationNodeID)
            ? choice.DestinationNodeID
            : currentNode.NextNodeID;

        if (!string.IsNullOrEmpty(destination))
        {
            ShowDialogue(destination);
            return;
        }

        EndDialogueFromChoice();
    }

    private void EndDialogueFromChoice()
    {
        dialogueEnded = true;
        currentNode = null;

        ClearChoiceButtons();

        continueButton.gameObject.SetActive(false);

        OnContinuePressed();
    }

    private void AdvanceNode()
    {
        StopAutoAdvance();

        if (currentNode == null)
            return;

        if (!string.IsNullOrEmpty(currentNode.NextNodeID))
        {
            ShowDialogue(currentNode.NextNodeID);
        }
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

    private void SelectAfterLayout(GameObject target)
    {
        if (selectRoutine != null)
        {
            StopCoroutine(selectRoutine);
        }

        selectRoutine = StartCoroutine(SelectAfterLayoutRoutine(target));
    }

    private IEnumerator SelectAfterLayoutRoutine(GameObject target)
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (choiceButtonContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(choiceButtonContainer);
        }

        yield return new WaitForEndOfFrame();

        Selectable selectable = target.GetComponent<Selectable>();

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
        selectable.Select();

        selectRoutine = null;
    }

    private void PlayCinematicMusic()
    {
        var level = ProgressionManager.Instance.CurrentLevel;

        if (level == null)
            return;

        AudioClip clip = isOutro
            ? level.outroMusic
            : level.introMusic;

        AudioManager.Instance.PlayMusic(clip);
    }

    public void OnContinuePressed()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        if (isOutro)
        {
            ProgressionManager.Instance.AdvanceLevel();
        }
        else
        {
            LoadGameplay();
        }
    }

    private void LoadGameplay()
    {
        SceneManager.LoadScene("SampleScene");
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    private void Update()
    {
        if (isTransitioning)
            return;

        if (currentNode == null)
            return;

        bool hasChoices =
            currentNode.Choices != null &&
            currentNode.Choices.Count > 0;

        
        if (hasChoices)
            return;

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

        if (string.IsNullOrEmpty(currentNode.NextNodeID))
        {
            EndDialogue();
        }
        else
        {
            AdvanceNode();
        }
    }
}