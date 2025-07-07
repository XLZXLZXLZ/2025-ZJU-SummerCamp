using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 挂载在单个历史记录UI预制件上，负责显示和交互。
/// </summary>
public class DialogueRecordUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text questionText;

    private DialogueRecord _myRecord;

    /// <summary>
    /// 初始化该UI单元。
    /// </summary>
    public void Initialize(DialogueRecord record, Color bgColor)
    {
        _myRecord = record;
        questionText.text = record.question;
        backgroundImage.color = bgColor;
    }

    /// <summary>
    /// 用于显示一个等待中的问题。
    /// </summary>
    public void InitializePending(string question, Color pendingColor)
    {
        _myRecord = null; // 等待中的记录没有正式的Record对象
        questionText.text = question;
        backgroundImage.color = pendingColor;
    }

    /// <summary>
    /// 实现IPointerClickHandler接口，用于检测点击。
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 检测是否为右键点击
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 如果这是一个已完成的、有正式记录的UI
            if (_myRecord != null)
            {
                Debug.Log($"请求删除记录: {_myRecord.question}");
                // 调用历史管理器删除此记录
                DialogueHistoryManager.Instance.DeleteSingleRecord(_myRecord);

                // 通知UI面板刷新
                // 这里我们通过一个静态事件来解耦，ClueUIPanel会监听这个事件
                ClueUIPanel.OnRequestRefresh?.Invoke();
            }
        }
    }
} 