using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeID;
    public List<RuntimeDialogueNode> AllNodes = new List<RuntimeDialogueNode>();
}

[Serializable]
public class RuntimeDialogueNode
{
    public string NodeID;
    public string SpeakerName;
    public string DialogueText;
    public float Delay;
    public Sprite Image;
    public List<ChoiceData> Choices = new List<ChoiceData>();
    public string NextNodeID;
}

[Serializable]
public class  ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
    [Header("Narrative Flags")]
    public List<string> FlagsToSet = new();   
    public List<string> FlagsToClear = new();
}