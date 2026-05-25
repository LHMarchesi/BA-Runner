using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public RuntimeDialogueGraph RuntimeGraph;

    [Header("UIComponents")]
    public GameObject dialoguePanel;
    public Image backgroundImage;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;
    public Button choiceButtonPrefab;
    public Transform choiceButtonContainer;

    private Dictionary<string, RuntimeDialogueNode> nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode currentNode;

    public Slider delayProgressBar;
    private Coroutine autoAdvanceCoroutine;
    private void Start()
    {
        foreach (var node in RuntimeGraph.AllNodes)
        {
            nodeLookup[node.NodeID] = node;
        }
        if (!string.IsNullOrEmpty(RuntimeGraph.EntryNodeID))
        {
            ShowDialogue(RuntimeGraph.EntryNodeID);
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
        //Image component can be added to the dialogue panel and set here based on the speaker or other node data if needed.

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
                            EndDialogue();
                        }
                    });
                }
            }
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
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
            EndDialogue();
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

    // ── Input ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && currentNode != null && currentNode.Choices.Count == 0)
        {
            AdvanceNode();
        }
    }

}
