using UnityEngine;

/// <summary>
/// ScriptableObject 用于存储一个可调查线索（事物）的相关信息。
/// </summary>
[CreateAssetMenu(fileName = "NewClue", menuName = "SeaTurtleSoup/Clue")]
public class ClueSO : ScriptableObject
{
    [Header("线索基本信息")]
    [Tooltip("线索的唯一ID，用于程序识别，例如：painting_final")]
    public string clueID;
    
    [Tooltip("线索的显示名称，例如：最后的画作《我，诞生》")]
    public string clueName;

    [Header("线索相关知识")]
    [Tooltip("这里只填写与该线索直接相关的信息，用于LLM判断问题相关性。")]
    [TextArea(10, 20)]
    public string clueDescription;
} 