
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

    /// <summary>
    /// 一个在内存中运行的轻量级向量数据库服务。
    /// 负责存储已解锁线索的语义向量，并提供相似度检索功能。
    /// </summary>
    public class VectorDatabaseService : Singleton<VectorDatabaseService>
    {
        // 数据库本体1: 存储每个线索ID -> 对应的向量
        private readonly Dictionary<string, float[]> _vectorStore = new Dictionary<string, float[]>();

        // 数据库本体2: 存储每个线索ID -> 对应的ClueSO脚本对象（作为缓存）
        private readonly Dictionary<string, ClueSO> _clueStore = new Dictionary<string, ClueSO>();

        /// <summary>
        /// 将一个新线索添加到向量数据库中。
        /// </summary>
        /// <param name="clue">要添加的线索对象</param>
        /// <param name="onComplete">成功添加时调用的回调</param>
        /// <param name="onError">发生错误时调用的回调</param>
        public void AddClue(ClueSO clue, System.Action onComplete = null, System.Action<string> onError = null)
        {
            if (clue == null || _vectorStore.ContainsKey(clue.clueID))
            {
                onComplete?.Invoke();
                return; // 如果线索为空或已存在，则不处理
            }

            string textToEmbed = clue.llmPromptHint;
            if (string.IsNullOrEmpty(textToEmbed))
            {
                onComplete?.Invoke();
                return;
            }

            LLMManager.Instance.GetEmbeddingAsync(textToEmbed, 
                vector =>
                {
                    if (vector != null)
                    {
                        _vectorStore[clue.clueID] = vector;
                        _clueStore[clue.clueID] = clue;
                        Debug.Log($"[VectorDB] 线索 '{clue.clueName}' 已成功添加到向量数据库。");
                    }
                    onComplete?.Invoke();
                },
                error =>
                {
                    Debug.LogError($"[VectorDB] 添加线索 '{clue.clueName}' 时发生错误: {error}");
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// 根据问题，从数据库中查找最相关的N个线索。
        /// </summary>
        /// <param name="question">玩家的问题</param>
        /// <param name="onComplete">成功查找后调用的回调，参数为相关线索列表</param>
        /// <param name="onError">发生错误时调用的回调</param>
        /// <param name="topN">需要返回的最相关线索的数量</param>
        public void FindMostRelevantClues(string question, System.Action<List<ClueSO>> onComplete, System.Action<string> onError, int topN = 3)
        {
            if (string.IsNullOrEmpty(question) || _vectorStore.Count == 0)
            {
                onComplete?.Invoke(new List<ClueSO>());
                return;
            }

            LLMManager.Instance.GetEmbeddingAsync(question,
                questionVector =>
                {
                    if (questionVector == null)
                    {
                        onComplete?.Invoke(new List<ClueSO>());
                        return;
                    }
                    
                    // 遍历数据库，计算所有线索与问题的相似度
                    var scoredClues = _vectorStore.Select(entry => new
                    {
                        ClueId = entry.Key,
                        Similarity = CalculateCosineSimilarity(questionVector, entry.Value)
                    }).ToList();
            
                    // 根据相似度从高到低排序，并取出前N个
                    var relevantClueIds = scoredClues
                        .OrderByDescending(item => item.Similarity)
                        .Take(topN)
                        .Select(item => item.ClueId);

                    // 从缓存中获取完整的ClueSO对象并返回
                    var result = relevantClueIds.Select(id => _clueStore[id]).ToList();
                    onComplete?.Invoke(result);
                },
                error =>
                {
                    Debug.LogError($"[VectorDB] 查找相关线索时发生错误: {error}");
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// 计算两个向量的余弦相似度。
        /// </summary>
        private float CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA == null || vecB == null || vecA.Length != vecB.Length) return 0;

            float dotProduct = 0.0f;
            float normA = 0.0f;
            float normB = 0.0f;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                normA += Mathf.Pow(vecA[i], 2);
                normB += Mathf.Pow(vecB[i], 2);
            }
            
            if (normA == 0 || normB == 0) return 0;
            
            return dotProduct / (Mathf.Sqrt(normA) * Mathf.Sqrt(normB));
        }
    }