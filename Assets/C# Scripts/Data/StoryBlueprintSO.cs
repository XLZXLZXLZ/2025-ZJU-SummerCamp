using UnityEngine;

/// <summary>
/// ScriptableObject 用于存储一个海龟汤故事的完整背景真相（汤底）。
/// </summary>
[CreateAssetMenu(fileName = "StoryBlueprint", menuName = "SeaTurtleSoup/Story Blueprint")]
public class StoryBlueprintSO : ScriptableObject
{
    [Header("故事核心内容")]
    [Tooltip("给LLM看的，最详尽、最客观的故事完整真相。")]
    [TextArea(10, 20)] public string fullStorySolution;
} 