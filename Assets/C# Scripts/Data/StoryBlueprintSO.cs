using UnityEngine;

/// <summary>
/// ScriptableObject 用于存储一个海龟汤故事的完整背景真相（汤底）。
/// </summary>
[CreateAssetMenu(fileName = "StoryBlueprint", menuName = "SeaTurtleSoup/Story Blueprint")]
public class StoryBlueprintSO : ScriptableObject
{
    [Header("故事核心真相")]
    [Tooltip("这里填写故事的完整背景、人物关系和最终谜底，即“汤底”。")]
    [TextArea(15, 30)]
    public string fullStorySolution;

    [Header("最终考核得分点")]
    [Tooltip("最终审判时，用于判断玩家陈述的多个核心得分点。")]
    public System.Collections.Generic.List<string> finalScoringPoints;
    
    [Tooltip("玩家需要答对至少几个得分点才算胜利。")]
    [Range(1, 10)]
    public int scoreThreshold = 3;
} 