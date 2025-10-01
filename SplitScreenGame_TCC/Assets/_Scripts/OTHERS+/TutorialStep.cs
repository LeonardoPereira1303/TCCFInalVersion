using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    public enum StepType
    {
        Container,
        Cutting,
        Stove,
        Delivery
    }

    public StepType stepType;
    [TextArea] public string instruction;
}
