using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class StartNode : Node
{
    protected override void OnDefinePorts(
        IPortDefinitionContext context
    )
    {
        context.AddOutputPort("out").Build();
    }
}

[Serializable]
public class EndNode : Node
{
    protected override void OnDefinePorts(
        IPortDefinitionContext context
    )
    {
        context.AddInputPort("in").Build();
    }
}

[Serializable]
public class DialogueNode : Node
{
    protected override void OnDefinePorts(
        IPortDefinitionContext context
    )
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();

        // Contenido
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();
        context.AddInputPort<Sprite>("Image").Build();

        // Avance automático del diálogo
        context.AddInputPort<float>("Delay").Build();

        // Audio
        context.AddInputPort<AudioClip>("Music").Build();
        context.AddInputPort<AudioClip>("Sound Effect").Build();

        context
            .AddInputPort<float>("Sound Effect Delay")
            .Build();
    }
}

[Serializable]
public class ChoiceNode : Node
{
    private const string optionID = "portCount";

    protected override void OnDefinePorts(
        IPortDefinitionContext context
    )
    {
        context.AddInputPort("in").Build();

        // Contenido
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();
        context.AddInputPort<Sprite>("Image").Build();

        // Audio
        context.AddInputPort<AudioClip>("Music").Build();
        context.AddInputPort<AudioClip>("Sound Effect").Build();

        context
            .AddInputPort<float>("Sound Effect Delay")
            .Build();

        // Elecciones
        var option =
            GetNodeOptionByName(optionID);

        option.TryGetValue(
            out int portCount
        );

        for (int i = 0; i < portCount; i++)
        {
            context
                .AddInputPort<string>(
                    $"Choice {i} Text"
                )
                .Build();

            context
                .AddInputPort<string>(
                    $"Choice {i} Flags"
                )
                .Build();

            context
                .AddInputPort<string>(
                    $"Choice {i} ClearFlags"
                )
                .Build();

            context
                .AddOutputPort(
                    $"Choice {i}"
                )
                .Build();
        }
    }

    protected override void OnDefineOptions(
        IOptionDefinitionContext context
    )
    {
        context.AddOption<int>(optionID);
    }
}