using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 专门负责管理历史记录UI列表的生成和刷新。
/// </summary>
public class RecordAreaManager : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("用于实例化单个记录的UI预制件")]
    [SerializeField] private GameObject recordUIPrefab;
    [Tooltip("所有记录UI的父容器，应挂载Vertical Layout Group")]
    [SerializeField] private Transform recordsParent;

    [Header("颜色配置")]
    [Tooltip("将评估结果映射到具体的颜色")]
    [SerializeField] private EvaluationColor[] evaluationColors;
    [Tooltip("用于等待中问题的颜色")]
    [SerializeField] private Color pendingColor = Color.gray;

    private Dictionary<EvaluationResult, Color> _colorMap = new Dictionary<EvaluationResult, Color>();

    private void Awake()
    {
        // 将数组配置转换为字典，方便快速查找
        foreach (var mapping in evaluationColors)
        {
            _colorMap[mapping.result] = mapping.color;
        }
    }

    /// <summary>
    /// 刷新整个记录区域的显示。
    /// </summary>
    public void RefreshDisplay(string clueId)
    {
        // 1. 清空所有旧的记录UI
        foreach (Transform child in recordsParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 获取该线索下所有已完成的记录
        IReadOnlyList<DialogueRecord> records = DialogueHistoryManager.Instance.GetRecordsForClue(clueId);

        // 3. 实例化新的UI
        foreach (var record in records)
        {
            GameObject newRecordUI = Instantiate(recordUIPrefab, recordsParent);
            var controller = newRecordUI.GetComponent<DialogueRecordUI>();
            if (controller != null)
            {
                // 从字典查找颜色，如果找不到则用默认白色
                _colorMap.TryGetValue(record.result.evaluation, out Color bgColor);
                controller.Initialize(record, bgColor);
            }
        }
    }
    
    /// <summary>
    /// 在列表末尾插入一个灰色的“等待中”记录。
    /// </summary>
    public void AddPendingRecord(string question)
    {
        GameObject newRecordUI = Instantiate(recordUIPrefab, recordsParent);
        var controller = newRecordUI.GetComponent<DialogueRecordUI>();
        controller?.InitializePending(question, pendingColor);
    }
}

/// <summary>
/// 用于在Inspector中方便地配置枚举到颜色的映射。
/// </summary>
[Serializable]
public struct EvaluationColor
{
    public EvaluationResult result;
    public Color color;
} 