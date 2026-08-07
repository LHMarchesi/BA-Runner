using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
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

    [Header("Localization")]
    [SerializeField] private string localizationTable = "LocalizationTableBARUNNER";

   

    private readonly Dictionary<string, RuntimeDialogueNode> nodeLookup =
        new Dictionary<string, RuntimeDialogueNode>();

    private RuntimeDialogueNode currentNode;

    private Coroutine autoAdvanceCoroutine;
    private Coroutine selectRoutine;
    private Coroutine continueTransitionCoroutine;
    private Coroutine nodeSoundCoroutine;

    private bool isOutro;
    private bool dialogueEnded;
    private bool isTransitioning;

    private int inputBlockedUntilFrame = -1;

    private void Start()
    {
        dialogueEnded = false;
        isTransitioning = false;

        continueButton.gameObject.SetActive(false);
        continueButton.interactable = false;
        continueButton.onClick.RemoveListener(HandleContinueButton);
        continueButton.onClick.AddListener(HandleContinueButton);

        if (delayProgressBar != null)
        {
            delayProgressBar.gameObject.SetActive(false);
        }

        ClearChoiceButtons();
        EnterCinematics();
    }

    private string GetLocalizedText(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        string cleanKey = key.Trim();

        string localizedText =
            LocalizationSettings.StringDatabase.GetLocalizedString(
                localizationTable,
                cleanKey
            );

        if (string.IsNullOrWhiteSpace(localizedText))
        {
            Debug.LogWarning(
                $"[Localization] No se encontró traducción para " +
                $"'{cleanKey}' en la tabla '{localizationTable}'."
            );

            return $"[{cleanKey}]";
        }

        return localizedText;
    }
    private void EnterCinematics()
    {
        if (GameManager.Instance == null)
        {
           
            return;
        }

        isOutro = GameManager.Instance.IsOutro;

        SetRuntimeGraphFromLevel();
        PlayCinematicMusic();

        if (CurrentRuntimeGraph == null)
        {
            if (isOutro)
            {
                if (ProgressionManager.Instance == null)
                {
                                      return;
                }

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
        if (ProgressionManager.Instance == null)
        {
            Debug.LogError(
                "[Dialogue] ProgressionManager.Instance es null."
            );

            CurrentRuntimeGraph = null;
            return;
        }

        Level_Scriptable currentLevel =
            ProgressionManager.Instance.CurrentLevel;

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

        foreach (RuntimeDialogueNode node in CurrentRuntimeGraph.AllNodes)
        {
            if (node == null)
                continue;

            if (string.IsNullOrWhiteSpace(node.NodeID))
            {
                Debug.LogWarning(
                    "[Dialogue] Se encontró un nodo sin NodeID."
                );

                continue;
            }

            string nodeID = node.NodeID.Trim();

            nodeLookup[nodeID] = node;
        }

        ValidateGraphConnections();

        if (!string.IsNullOrWhiteSpace(CurrentRuntimeGraph.EntryNodeID))
        {
            ShowDialogue(
                CurrentRuntimeGraph.EntryNodeID.Trim()
            );
        }
        else
        {

            EndDialogue();
        }
    }

    private void ValidateGraphConnections()
    {
        if (CurrentRuntimeGraph == null)
            return;

        foreach (RuntimeDialogueNode node in CurrentRuntimeGraph.AllNodes)
        {
            if (node == null)
                continue;

            if (!string.IsNullOrWhiteSpace(node.NextNodeID))
            {
                string nextNodeID =
                    node.NextNodeID.Trim();
            }

            if (node.Choices == null)
                continue;

            foreach (ChoiceData choice in node.Choices)
            {
                if (choice == null ||
                    string.IsNullOrWhiteSpace(choice.DestinationNodeID))
                {
                    continue;
                }

                string destination =
                    choice.DestinationNodeID.Trim();

                if (!nodeLookup.ContainsKey(destination))
                {
                    Debug.LogError(
                        $"[Dialogue Validation] La elección " +
                        $"'{choice.ChoiceText}' del nodo '{node.NodeID}' " +
                        $"apunta al nodo inexistente '{destination}'."
                    );
                }
            }
        }
    }

    private void ShowDialogue(string nodeID)
    {
      

        if (string.IsNullOrWhiteSpace(nodeID))
        {
            Debug.LogWarning(
                "[Dialogue] Se intentó mostrar un NodeID vacío."
            );

            EndDialogue();
            return;
        }

        nodeID = nodeID.Trim();

        if (!nodeLookup.TryGetValue(
                nodeID,
                out RuntimeDialogueNode node
            ))
        {
            Debug.LogError(
                $"[Dialogue] No se encontró el nodo '{nodeID}'."
            );

            EndDialogue();
            return;
        }
        StopAutoAdvance();
        StopPendingNodeSound();
        currentNode = node;
        dialogueEnded = false;

        

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        continueButton.gameObject.SetActive(false);
        continueButton.interactable = false;

        if (backgroundImage != null)
        {
            backgroundImage.sprite = currentNode.Image;

            backgroundImage.gameObject.SetActive(
                currentNode.Image != null
            );
        }

        if (SpeakerNameText != null)
        {
            SpeakerNameText.text =
                currentNode.SpeakerName;
        }

        if (DialogueText != null)
        {
            DialogueText.text =
                GetLocalizedText(currentNode.DialogueText);
        }

        ClearChoiceButtons();

        bool hasChoices =
            currentNode.Choices != null &&
            currentNode.Choices.Count > 0;

        if (hasChoices)
        {
            SetupChoiceButtons(currentNode.Choices);
            return;
        }

        if (currentNode.Delay > 0f)
        {
            autoAdvanceCoroutine = StartCoroutine(
                AutoAdvanceCoroutine(currentNode.Delay)
            );
        }

        PlayNodeAudio(currentNode);
    }

    private void SetupChoiceButtons(
        List<ChoiceData> choices
    )
    {
        ClearChoiceButtons();

        if (choices == null || choices.Count == 0)
        {
            EndDialogue();
            return;
        }

        bool choicesAreExclusive =
            currentNode != null &&
            currentNode.ChoicesAreExclusive;

        int activeButtonCount =
            Mathf.Min(
                choices.Count,
                choiceButtons.Count
            );

        if (choices.Count > choiceButtons.Count)
        {
            Debug.LogWarning(
                $"[Dialogue] Hay {choices.Count} elecciones, pero solamente " +
                $"{choiceButtons.Count} botones disponibles."
            );
        }

        for (int i = 0; i < activeButtonCount; i++)
        {
            ChoiceData capturedChoice = choices[i];
            Button button = choiceButtons[i];

            if (button == null)
            {
                Debug.LogWarning(
                    $"[Dialogue] Choice Button {i} no está asignado."
                );

                continue;
            }

            if (button == continueButton)
            {
                Debug.LogError(
                    "[Dialogue] Continue Button está incluido en choiceButtons. " +
                    "Elimínalo de esa lista en el Inspector."
                );

                continue;
            }

            button.gameObject.SetActive(true);
            button.interactable = true;
            button.onClick.RemoveAllListeners();

            TextMeshProUGUI buttonText =
                button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null &&
                capturedChoice != null)
            {
                buttonText.text =
                    GetLocalizedText(capturedChoice.ChoiceText);
            }

            button.onClick.AddListener(() =>
            {
                BlockDialogueInputForCurrentFrame();

                if (capturedChoice == null)
                {
                    Debug.LogError(
                        "[Dialogue] La ChoiceData seleccionada es null."
                    );

                    EndDialogue();
                    return;
                }

                Debug.Log(
                    $"[Choice] Elegida: '{capturedChoice.ChoiceText}'"
                );

                ClearChoiceButtons();

                if (ProgressionManager.Instance == null)
                {
                    Debug.LogError(
                        "[Dialogue] ProgressionManager.Instance es null."
                    );

                    EndDialogue();
                    return;
                }

                ProgressionManager.Instance.ApplyChoice(
                    capturedChoice,
                    choices,
                    choicesAreExclusive
                );

                if (capturedChoice.FlagsToSet != null &&
                    capturedChoice.FlagsToSet.Count > 0)
                {
                    string firstFlag =
                        capturedChoice.FlagsToSet[0];

                    Debug.Log(
                        $"[Flags] HasFlag {firstFlag}: " +
                        $"{ProgressionManager.Instance.HasFlag(firstFlag)}"
                    );
                }

                AdvanceNodeByChoice(capturedChoice);
            });
        }

        SetupExplicitNavigation(activeButtonCount);

        if (activeButtonCount > 0)
        {
            Button firstButton =
                choiceButtons[0];

            if (firstButton != null &&
                firstButton != continueButton &&
                firstButton.gameObject.activeInHierarchy)
            {
                SelectAfterLayout(
                    firstButton.gameObject
                );
            }
        }
    }

    private void SetupExplicitNavigation(
        int activeButtonCount
    )
    {
        if (activeButtonCount <= 0)
            return;

        for (int i = 0; i < activeButtonCount; i++)
        {
            Button current = choiceButtons[i];

            if (current == null ||
                current == continueButton)
            {
                continue;
            }

            Button previous =
                choiceButtons[
                    (i - 1 + activeButtonCount) %
                    activeButtonCount
                ];

            Button next =
                choiceButtons[
                    (i + 1) %
                    activeButtonCount
                ];

            Navigation navigation =
                new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = previous,
                    selectOnLeft = previous,
                    selectOnDown = next,
                    selectOnRight = next
                };

            current.navigation = navigation;
        }
    }

    private void ClearChoiceButtons()
    {
        ClearCurrentSelection();

        foreach (Button button in choiceButtons)
        {
            if (button == null ||
                button == continueButton)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.interactable = false;
            button.gameObject.SetActive(false);
        }
    }

    private void EndDialogue()
    {
        StopAutoAdvance();
        StopPendingNodeSound();
        ClearChoiceButtons();

        currentNode = null;
        dialogueEnded = true;
        isTransitioning = false;

        /*
         * Solo impide que el input usado para cerrar el último
         * nodo active Continue durante este mismo frame.
         */
        BlockDialogueInputForCurrentFrame();

        if (continueButton == null)
        {
            Debug.LogError(
                "[Dialogue] Continue Button no está asignado."
            );

            return;
        }

        continueButton.gameObject.SetActive(true);
        continueButton.interactable = true;

        Debug.Log(
            $"[Dialogue] Diálogo finalizado. " +
            $"Continue disponible. IsOutro: {isOutro}"
        );

        SelectAfterLayout(
            continueButton.gameObject
        );
    }

    private void AdvanceNodeByChoice(
        ChoiceData choice
    )
    {
        StopAutoAdvance();

        if (choice == null)
        {
            Debug.LogError(
                "[Dialogue] AdvanceNodeByChoice recibió una ChoiceData null."
            );

            EndDialogue();
            return;
        }

        string destination = null;

        if (!string.IsNullOrWhiteSpace(
                choice.DestinationNodeID
            ))
        {
            destination =
                choice.DestinationNodeID.Trim();
        }
        else if (
            currentNode != null &&
            !string.IsNullOrWhiteSpace(
                currentNode.NextNodeID
            )
        )
        {
            destination =
                currentNode.NextNodeID.Trim();
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            Debug.Log(
                $"[Dialogue] La elección '{choice.ChoiceText}' " +
                "termina la cinemática."
            );

            EndDialogue();
            return;
        }

        if (!nodeLookup.ContainsKey(destination))
        {
            Debug.LogError(
                $"[Dialogue] La elección '{choice.ChoiceText}' apunta al nodo " +
                $"inexistente '{destination}'. Se finalizará la cinemática."
            );

            EndDialogue();
            return;
        }

        Debug.Log(
            $"[Dialogue] La elección '{choice.ChoiceText}' continúa hacia " +
            $"el nodo '{destination}'."
        );

        ShowDialogue(destination);
    }

    private void AdvanceNode()
    {
        StopAutoAdvance();

        if (currentNode == null)
            return;

        if (!string.IsNullOrWhiteSpace(
                currentNode.NextNodeID
            ))
        {
            ShowDialogue(
                currentNode.NextNodeID.Trim()
            );
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator AutoAdvanceCoroutine(
        float delay
    )
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
            {
                delayProgressBar.value =
                    Mathf.Clamp01(elapsed / delay);
            }

            yield return null;
        }

        if (delayProgressBar != null)
        {
            delayProgressBar.gameObject.SetActive(false);
        }

        autoAdvanceCoroutine = null;

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
        {
            delayProgressBar.gameObject.SetActive(false);
        }
    }

    private void SelectAfterLayout(
        GameObject target
    )
    {
        if (target == null)
            return;

        if (selectRoutine != null)
        {
            StopCoroutine(selectRoutine);
            selectRoutine = null;
        }

        selectRoutine = StartCoroutine(
            SelectAfterLayoutRoutine(target)
        );
    }

    private IEnumerator SelectAfterLayoutRoutine(
        GameObject target
    )
    {
        yield return null;

        if (target == null ||
            !target.activeInHierarchy)
        {
            selectRoutine = null;
            yield break;
        }

        Canvas.ForceUpdateCanvases();

        if (choiceButtonContainer != null &&
            choiceButtonContainer.gameObject.activeInHierarchy)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                choiceButtonContainer
            );
        }

        yield return new WaitForEndOfFrame();

        if (target == null ||
            !target.activeInHierarchy)
        {
            selectRoutine = null;
            yield break;
        }

        Selectable selectable =
            target.GetComponent<Selectable>();

        if (selectable == null ||
            !selectable.IsActive() ||
            !selectable.IsInteractable() ||
            EventSystem.current == null)
        {
            selectRoutine = null;
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
        selectable.Select();

        selectRoutine = null;
    }

    private void PlayCinematicMusic()
    {
        if (ProgressionManager.Instance == null)
            return;

        Level_Scriptable level =
            ProgressionManager.Instance.CurrentLevel;

        if (level == null)
            return;

        AudioClip clip = isOutro
            ? level.outroMusic
            : level.introMusic;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(clip);
        }
    }

    private void PlayNodeAudio(RuntimeDialogueNode node)
    {
        if (node == null)
            return;

        AudioClip musicClip = node.Music;
        AudioClip soundEffectClip = node.SoundEffect;

        AudioManager audioManager =
            AudioManager.Instance;

        if (musicClip != null)
        {
            audioManager.PlayMusic(
                musicClip
            );
        }

        /*
         * SoundEffect null:
         * no hay nada más que hacer.
         */
        if (soundEffectClip == null)
            return;

        float safeDelay =
            Mathf.Max(
                0f,
                node.SoundEffectDelay
            );

        if (safeDelay <= 0f)
        {
            audioManager.PlaySFX(
                soundEffectClip
            );

            return;
        }

        string sourceNodeID =
            node.NodeID;

        nodeSoundCoroutine = StartCoroutine(
            PlayNodeSoundAfterDelay(
                sourceNodeID,
                soundEffectClip,
                safeDelay
            )
        );
    }
    private IEnumerator PlayNodeSoundAfterDelay(
     string sourceNodeID,
     AudioClip clip,
     float delay
 )
    {
        yield return new WaitForSeconds(delay);

        nodeSoundCoroutine = null;

        /*
         * El jugador pudo haber avanzado a otro nodo
         * antes de que terminara el delay.
         */
        if (currentNode == null)
            yield break;

        if (currentNode.NodeID != sourceNodeID)
            yield break;

        /*
         * El clip pudo quedar vacío o Missing.
         */
        if (clip == null)
            yield break;

        AudioManager audioManager =
            AudioManager.Instance;

        if (audioManager == null)
            yield break;

        audioManager.PlaySFX(
            clip
        );
    }


    private void StopPendingNodeSound()
    {
        if (nodeSoundCoroutine == null)
            return;

        StopCoroutine(nodeSoundCoroutine);
        nodeSoundCoroutine = null;
    }
    private void HandleContinueButton()
    {
       

        if (IsDialogueInputBlocked())
        {
           
            return;
        }

        if (!dialogueEnded)
        {  

            return;
        }

        if (isTransitioning)
        {
            return;
        }

        if (continueTransitionCoroutine != null)
        {
            StopCoroutine(continueTransitionCoroutine);
        }

        continueTransitionCoroutine = StartCoroutine(
            ContinueTransitionRoutine()
        );
    }

    /*
     * Se conserva como método público por si algún otro objeto
     * ya lo invoca, pero redirige al mismo flujo seguro.
     */
    public void OnContinuePressed()
    {
        HandleContinueButton();
    }

    private IEnumerator ContinueTransitionRoutine()
    {
        isTransitioning = true;

        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        ClearCurrentSelection();

        /*
         * Dejamos terminar por completo el evento de UI antes
         * de cambiar de estado o cargar otra escena.
         */
        yield return null;

        continueTransitionCoroutine = null;

    
        try
        {
            if (isOutro)
            {
                if (ProgressionManager.Instance == null)
                {
                    RestoreContinueButton();
                    yield break;
                }

                ProgressionManager.Instance.AdvanceLevel();
            }
            else
            {
                LoadGameplay();
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            RestoreContinueButton();
        }
    }

    private void RestoreContinueButton()
    {
        isTransitioning = false;

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
        }

        SelectAfterLayout(
            continueButton != null
                ? continueButton.gameObject
                : null
        );
    }

    private void LoadGameplay()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[Dialogue] GameManager.Instance es null."
            );

            RestoreContinueButton();
            return;
        }

        GameManager.Instance.ChangeState(
            GameState.Playing
        );

        SceneManager.LoadScene("SampleScene");
    }

    private void Update()
    {
        if (isTransitioning)
            return;

        if (IsDialogueInputBlocked())
            return;

        /*
         * Cuando aparece Continue, solamente su propio onClick
         * puede iniciar la transición. Esto evita reutilizar el
         * clic o Submit de la elección.
         */
        if (dialogueEnded)
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

        bool keyboardPressed =
            Keyboard.current != null &&
            (
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame
            );

        if (!mousePressed &&
            !keyboardPressed)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                currentNode.NextNodeID
            ))
        {
            EndDialogue();
        }
        else
        {
            AdvanceNode();
        }
    }

    private void BlockDialogueInputForCurrentFrame()
    {
        inputBlockedUntilFrame =
            Time.frameCount;
    }

    private bool IsDialogueInputBlocked()
    {
        return Time.frameCount <=
               inputBlockedUntilFrame;
    }

    private void ClearCurrentSelection()
    {
        if (selectRoutine != null)
        {
            StopCoroutine(selectRoutine);
            selectRoutine = null;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnDestroy()
    {
        StopAutoAdvance();
        StopPendingNodeSound();

        if (selectRoutine != null)
        {
            StopCoroutine(selectRoutine);
            selectRoutine = null;
        }

        if (continueTransitionCoroutine != null)
        {
            StopCoroutine(continueTransitionCoroutine);
            continueTransitionCoroutine = null;
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                HandleContinueButton
            );
        }
    }
}