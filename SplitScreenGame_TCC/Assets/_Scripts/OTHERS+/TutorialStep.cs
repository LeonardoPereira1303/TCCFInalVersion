using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    public enum StepType
    {
        Container,
        Cutting,
        Stove,
        Delivery,
        Counter,
        Teleport,
        PickPlate
    }

    public StepType stepType;
    [TextArea] public string instruction;
    public HighlightableCounter highlightTarget; // <- usado para glow
    public Sprite image;
}
