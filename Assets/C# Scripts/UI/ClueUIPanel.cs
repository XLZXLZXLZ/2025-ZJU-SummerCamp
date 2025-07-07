using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

/// <summary>
/// 控制核心线索UI面板的显示和交互。
/// </summary>
public class ClueUIPanel : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Image clueImage;
    [SerializeField] private Text descriptionText;
    [SerializeField] private InputField questionInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private RecordAreaManager recordArea;

    [Header("线索数据")]
    [Tooltip("此面板能显示的所有线索列表")]
    [SerializeField] private List<ClueSO> allClues;
    
    [Header("未发现状态")]
    [SerializeField] private Sprite undiscoveredSprite;
    [SerializeField] private string undiscoveredDescription = "？？？";

    [Header("调试与测试")]
    [Tooltip("【仅测试用】点击可立即解锁当前线索")]
    [SerializeField] private Button debugUnlockButton;
    [Tooltip("【仅测试用】点击可立即锁定当前线索")]
    [SerializeField] private Button debugLockButton;
    
    [Header("动画效果")]
    [Tooltip("面板飞入/飞出的动画时长")]
    [SerializeField] private float animationDuration = 0.5f;
    [Tooltip("动画使用的缓动效果")]
    [SerializeField] private Ease easeType = Ease.OutQuint;
    [Tooltip("动画偏移量")]
    [SerializeField] private float hideOffset=100;

    // 内部状态
    private int _currentPageIndex = 0;
    private HashSet<string> _unlockedClues = new HashSet<string>();
    private InferenceService _inferenceService;
    private RectTransform _panelRectTransform;
    private Vector2 _onScreenPosition;
    private Vector2 _offScreenPosition;
    private bool _isPanelOpen = false;

    // 用于从外部（如DialogueRecordUI）请求刷新的静态事件
    public static Action OnRequestRefresh;

    private void Awake()
    {
        _inferenceService = new InferenceService();
        // 订阅刷新事件
        OnRequestRefresh += () => RefreshCurrentPage();
        
        // 初始化动画位置
        _panelRectTransform = GetComponent<RectTransform>();
        _onScreenPosition = _panelRectTransform.anchoredPosition;
        
        // 这个计算假设UI面板的锚点(Anchor)在屏幕底部，这样它才能正确地“飞出”到屏幕下方。
        // 在RectTransform组件中，将Anchor预设为 bottom/bottom-left/bottom-right 中的一种。
        _offScreenPosition = new Vector2(_onScreenPosition.x, -_panelRectTransform.rect.height - hideOffset);

        // 初始状态下，面板在屏幕外
        _panelRectTransform.anchoredPosition = _offScreenPosition;
    }

    private void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        OnRequestRefresh -= () => RefreshCurrentPage();
    }

    private void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClick);
        nextPageButton.onClick.AddListener(GoToNextPage);
        prevPageButton.onClick.AddListener(GoToPrevPage);

        // 绑定测试按钮的事件
        if(debugUnlockButton) debugUnlockButton.onClick.AddListener(DebugUnlockCurrentClue);
        if(debugLockButton) debugLockButton.onClick.AddListener(DebugLockCurrentClue);

        // 初始状态不显示页面，在第一次打开时再显示
        // DisplayPage(_currentPageIndex);
    }

    private void Update()
    {
        // 监听Tab快捷键
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePanel();
        }
    }

    /// <summary>
    /// 开关UI面板的公共接口。
    /// </summary>
    public void TogglePanel()
    {
        _isPanelOpen = !_isPanelOpen;
        if (_isPanelOpen)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
    }

    private void ShowPanel()
    {
        _panelRectTransform.DOAnchorPos(_onScreenPosition, animationDuration).SetEase(easeType);
        // 打开面板时，刷新显示内容
        RefreshCurrentPage();
    }

    private void HidePanel()
    {
        _panelRectTransform.DOAnchorPos(_offScreenPosition, animationDuration).SetEase(easeType);
    }

    private void OnSendButtonClick()
    {
        string question = questionInput.text;
        ClueSO currentClue = allClues[_currentPageIndex];

        if (string.IsNullOrEmpty(question) || !_unlockedClues.Contains(currentClue.clueID)) return;

        // 5.1 缓存记录并在UI上显示“等待中”
        DialogueHistoryManager.Instance.RegisterPendingQuestion(currentClue.clueID, question);
        recordArea.AddPendingRecord(question);
        
        // 调用推理服务
        _inferenceService.AskQuestionAboutClue(GameManager.Instance.CurrentStory, currentClue, question, (result) =>
        {
            // 5.2 提交完整记录
            DialogueHistoryManager.Instance.CommitCompletedQuestion(question, result);

            // 5.3 如果返回结果时，玩家还在看这一页，则刷新
            if (allClues[_currentPageIndex].clueID == currentClue.clueID)
            {
                recordArea.RefreshDisplay(currentClue.clueID);
            }
        });

        questionInput.text = "";
    }
    
    private void DisplayPage(int index)
    {
        _currentPageIndex = Mathf.Clamp(index, 0, allClues.Count - 1);
        
        ClueSO currentClue = allClues[_currentPageIndex];

        // 3. 根据解锁状态显示不同内容
        if (_unlockedClues.Contains(currentClue.clueID))
        {
            clueImage.sprite = currentClue.clueSprite;
            descriptionText.text = currentClue.clueDescriptionForPlayer;
            sendButton.interactable = true;
            questionInput.interactable = true;
        }
        else
        {
            clueImage.sprite = undiscoveredSprite;
            descriptionText.text = undiscoveredDescription;
            sendButton.interactable = false;
            questionInput.interactable = false;
        }

        // 4. 刷新历史记录区域
        recordArea.RefreshDisplay(currentClue.clueID);

        // 更新翻页按钮状态
        prevPageButton.interactable = (_currentPageIndex > 0);
        nextPageButton.interactable = (_currentPageIndex < allClues.Count - 1);
    }

    /// <summary>
    /// 解锁一个新线索的公共接口。
    /// </summary>
    public void UnlockClue(string clueId)
    {
        if (!_unlockedClues.Contains(clueId))
        {
            _unlockedClues.Add(clueId);
            // 如果解锁的是当前页，则刷新以显示新内容
            if (allClues[_currentPageIndex].clueID == clueId)
            {
                RefreshCurrentPage();
            }
        }
    }
    
    private void RefreshCurrentPage()
    {
        DisplayPage(_currentPageIndex);
    }

    public void GoToNextPage() => DisplayPage(_currentPageIndex + 1);
    public void GoToPrevPage() => DisplayPage(_currentPageIndex - 1);

    #region Debug Methods

    /// <summary>
    /// 【仅测试用】解锁当前页面显示的线索。
    /// </summary>
    private void DebugUnlockCurrentClue()
    {
        if (allClues.Count == 0) return;
        string currentClueId = allClues[_currentPageIndex].clueID;
        UnlockClue(currentClueId);
        Debug.Log($"[Debug] 已通过测试按钮解锁线索: {currentClueId}");
    }

    /// <summary>
    /// 【仅测试用】锁定当前页面显示的线索。
    /// </summary>
    private void DebugLockCurrentClue()
    {
        if (allClues.Count == 0) return;
        string currentClueId = allClues[_currentPageIndex].clueID;
        if (_unlockedClues.Contains(currentClueId))
        {
            _unlockedClues.Remove(currentClueId);
            RefreshCurrentPage();
            Debug.Log($"[Debug] 已通过测试按钮锁定线索: {currentClueId}");
        }
    }

    #endregion
}
