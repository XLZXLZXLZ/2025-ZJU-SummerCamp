using UnityEngine;

/// <summary>
/// 游戏总管理器，负责游戏流程控制、状态管理和协调其他管理器。
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("故事配置")]
    [Tooltip("当前游戏加载的故事蓝图")]
    [SerializeField] private StoryBlueprintSO currentStory;

    /// <summary>
    /// 获取当前游戏使用的核心故事蓝图。
    /// </summary>
    public StoryBlueprintSO CurrentStory => currentStory;

    // 此处将用于未来的游戏状态管理、事件触发等核心逻辑。
} 