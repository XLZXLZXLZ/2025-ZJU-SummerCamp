
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using BigBata.InGame.UI;

/// <summary>
/// 游戏总管理器，负责游戏流程控制、状态管理和协调其他管理器。
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("故事配置")]
    [Tooltip("当前游戏加载的故事蓝图")]
    [SerializeField] private StoryBlueprintSO _currentStory;
    public StoryBlueprintSO currentStory => _currentStory;


    public static event Action OnGameStarted;

    [Header("Game Flow Control")]
    [Tooltip("结局动画所在的场景名称")]
    [SerializeField] private string endingSceneName = "EndingScene";
    [Tooltip("暂停菜单的引用")]
    [SerializeField] private PausePanel pausePanel;


    protected override void Awake()
    {
        base.Awake();
        // 在这里可以添加其他需要在游戏一开始就初始化的逻辑
    }
    
    private void Start()
    {
        // No longer needs to subscribe to UI events
    }

    private void OnDestroy()
    {
        // No longer needs to unsubscribe from UI events
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null && !pausePanel.IsVisible)
            {
                pausePanel.Show();
            }
        }
    }

    public void GameStart()
    {
        Debug.Log("Game Started!");
        OnGameStarted?.Invoke();
    }

    public void StartEndingSequence()
    {
        Debug.Log($"准备跳转到结局场景: {endingSceneName}");
        SceneManager.LoadScene(endingSceneName);
    }
} 