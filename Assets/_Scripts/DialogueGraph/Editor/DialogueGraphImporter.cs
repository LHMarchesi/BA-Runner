using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;


[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
        var nodeIDMap = new Dictionary<INode, string>();

        foreach (var node in editorGraph.GetNodes())
        {
            nodeIDMap[node] = Guid.NewGuid().ToString();
        }

        var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
        if (startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if (entryPort != null)
            {
                runtimeGraph.EntryNodeID = nodeIDMap[entryPort.GetNode()];
            }
        }

        foreach (var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue;

            var runtimeNode = new RuntimeDialogueNode { NodeID = nodeIDMap[iNode] };
            if (iNode is DialogueNode dialogueNode)
            {
                ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
            }
            else if (iNode is ChoiceNode choiceNode)
            {
                ProcessChoiceNode(choiceNode, runtimeNode, nodeIDMap);
            }

            runtimeGraph.AllNodes.Add(runtimeNode);
        }
        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);

        foreach (RuntimeDialogueNode savedNode in runtimeGraph.AllNodes)
        {
            if (savedNode == null)
                continue;
        }
    }

    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        runtimeNode.Image = GetPortValue<Sprite>(node.GetInputPortByName("Image"));
        runtimeNode.Delay = GetPortValue<float>(node.GetInputPortByName("Delay"));
        runtimeNode.Music = GetPortValue<AudioClip>(node.GetInputPortByName("Music"));
        runtimeNode.SoundEffectKey = GetPortValue<string>(node.GetInputPortByName("Sound Effect Key"));
        runtimeNode.SoundEffect = GetPortValue<AudioClip>(node.GetInputPortByName("Sound Effect"));
        runtimeNode.SoundEffectDelay = GetPortValue<float>(node.GetInputPortByName("Sound Effect Delay"));
        float importedVolume = GetPortValue<float>(node.GetInputPortByName("Sound Effect Volume"));

        runtimeNode.SoundEffectVolume = importedVolume <= 0f ? 1f : Mathf.Clamp01(importedVolume);

        var nextNodePOrt = node.GetOutputPortByName("out").firstConnectedPort;
        if (nextNodePOrt != null) { runtimeNode.NextNodeID = nodeIDMap[nextNodePOrt.GetNode()]; }
    }


    private void ProcessChoiceNode(
    ChoiceNode node,
    RuntimeDialogueNode runtimeNode,
    Dictionary<INode, string> nodeIDMap
)
    {
        runtimeNode.SpeakerName =
            GetPortValue<string>(
                node.GetInputPortByName("Speaker")
            );

        runtimeNode.DialogueText =
            GetPortValue<string>(
                node.GetInputPortByName("Dialogue")
            );

        runtimeNode.Image =
            GetPortValue<Sprite>(
                node.GetInputPortByName("Image")
            );


        var choiceOutputPorts =
            node.GetOutputPorts()
                .Where(
                    p => p.name.StartsWith("Choice ")
                );


        foreach (var outputPort in choiceOutputPorts)
        {
            var index =
                outputPort.name.Substring(
                    "Choice ".Length
                );


            var textPort =
                node.GetInputPortByName(
                    $"Choice {index} Text"
                );

            var flagsPort =
                node.GetInputPortByName(
                    $"Choice {index} Flags"
                );

            var clearFlagsPort =
                node.GetInputPortByName(
                    $"Choice {index} ClearFlags"
                );

            var endsCinematicPort =
                node.GetInputPortByName(
                    $"Choice {index} Ends Cinematic"
                );


            var choiceData =
                new ChoiceData
                {
                    ChoiceText =
                        GetPortValue<string>(
                            textPort
                        ),

                    DestinationNodeID =
                        outputPort.firstConnectedPort != null
                            ? nodeIDMap[
                                outputPort
                                    .firstConnectedPort
                                    .GetNode()
                            ]
                            : null,

                    FlagsToSet =
                        ParseFlags(
                            GetPortValue<string>(
                                flagsPort
                            )
                        ),

                    FlagsToClear =
                        ParseFlags(
                            GetPortValue<string>(
                                clearFlagsPort
                            )
                        ),

                    EndsCinematic =
                        GetPortValue<bool>(
                            endsCinematicPort
                        )
                };


            runtimeNode.Choices.Add(
                choiceData
            );
        }
    }

    private List<string> ParseFlags(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new List<string>();

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(f => f.Trim())
                  .Where(f => f.Length > 0)
                  .ToList();
    }

    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if (port.isConnected)
        {
            if (port.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.variable.TryGetDefaultValue(out T value);
                return value;
            }
        }

        port.TryGetValue(out T fallbackValue);
        return fallbackValue;
    }
}

