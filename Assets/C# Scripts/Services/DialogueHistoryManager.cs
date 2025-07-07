using System.Collections.Generic;
using UnityEngine;

// 使用using别名，让字典的类型声明更简洁易读
using DialogueLog = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<DialogueRecord>>;

/// <summary>
/// 负责管理和存储玩家的所有问答历史记录。
/// 采用单例模式，方便全局访问。
/// </summary>
public class DialogueHistoryManager : Singleton<DialogueHistoryManager>
{
    // 使用private set来保护字典不被外部直接修改，只能通过方法来操作
    public DialogueLog CompletedRecords { get; private set; } = new DialogueLog();

    // Key是问题原文，Value是该问题对应的ClueID。
    private Dictionary<string, string> _pendingQuestions = new Dictionary<string, string>();

    /// <summary>
    /// 当玩家发起一个新提问时调用，将问题加入等待缓存。
    /// </summary>
    public void RegisterPendingQuestion(string clueId, string question)
    {
        if (!_pendingQuestions.ContainsKey(question))
        {
            _pendingQuestions.Add(question, clueId);
            Debug.Log($"[DialogueHistory] '{question}' 已加入等待队列。");
        }
    }

    /// <summary>
    /// 当LLM返回结果后调用，将问题从缓存移出并存入永久记录。
    /// </summary>
    public void CommitCompletedQuestion(string question, InferenceResult result)
    {
        if (_pendingQuestions.TryGetValue(question, out string clueId))
        {
            _pendingQuestions.Remove(question);
            var newRecord = new DialogueRecord(clueId, question, result);

            if (!CompletedRecords.ContainsKey(clueId))
            {
                CompletedRecords.Add(clueId, new List<DialogueRecord>());
            }
            CompletedRecords[clueId].Add(newRecord);

            Debug.Log($"[DialogueHistory] '{question}' 的结果已记录到线索 '{clueId}' 下。");
            // 在这里可以触发一个事件，通知UI刷新
        }
    }

    /// <summary>
    /// 获取某个特定线索下的所有问答记录。
    /// </summary>
    /// <returns>一个只读的记录列表，如果不存在则返回空列表。</returns>
    public IReadOnlyList<DialogueRecord> GetRecordsForClue(string clueId)
    {
        if (CompletedRecords.TryGetValue(clueId, out List<DialogueRecord> records))
        {
            return records;
        }
        return System.Array.AsReadOnly(new DialogueRecord[0]); // 返回一个空的只读数组，更高效
    }

    /// <summary>
    /// 删除一条特定的历史记录。
    /// </summary>
    /// <param name="recordToDelete">要删除的记录对象</param>
    public void DeleteSingleRecord(DialogueRecord recordToDelete)
    {
        if (recordToDelete == null) return;

        if (CompletedRecords.TryGetValue(recordToDelete.clueID, out List<DialogueRecord> records))
        {
            if (records.Remove(recordToDelete))
            {
                Debug.Log($"[DialogueHistory] 已从线索 '{recordToDelete.clueID}' 中删除记录: '{recordToDelete.question}'");
            }
        }
    }

    /// <summary>
    /// 删除某个特定线索下的所有历史记录。
    /// </summary>
    public void DeleteRecordsForClue(string clueId)
    {
        if (CompletedRecords.ContainsKey(clueId))
        {
            CompletedRecords.Remove(clueId);
            Debug.Log($"[DialogueHistory] 线索 '{clueId}' 的所有记录已删除。");
        }
    }

    /// <summary>
    /// 删除所有历史记录，完全重置。
    /// </summary>
    public void DeleteAllRecords()
    {
        CompletedRecords.Clear();
        _pendingQuestions.Clear(); // 同样清空等待队列
        Debug.Log("[DialogueHistory] 所有历史记录已清空。");
    }
} 