/// <summary>
/// 定义LLM对玩家提问的评估结果。
/// </summary>
public enum EvaluationResult
{
    /// <summary>
    /// 提问内容完全正确。
    /// </summary>
    CompletelyCorrect,
    
    /// <summary>
    /// 提问内容部分正确，但包含错误或不完整的推断。
    /// </summary>
    PartiallyCorrect,
    
    /// <summary>
    /// 提问内容完全错误。
    /// </summary>
    Incorrect,
    
    /// <summary>
    /// 根据已知信息，无法判断对错。
    /// </summary>
    Noncommittal,
    
    /// <summary>
    /// 提问内容与当前聚焦的线索无关。
    /// </summary>
    Irrelevant,
    
    /// <summary>
    /// 发生了意外情况，例如LLM返回了无效格式。
    /// </summary>
    Unexpected
}

/// <summary>
/// 用于从LLM返回的JSON中解析数据的临时类。
/// </summary>
[System.Serializable]
public class InferenceResponse
{
    public string evaluation;
    public string explanation;
}

/// <summary>
/// 最终传递给游戏逻辑的、经过处理和验证的强类型结果。
/// </summary>
public struct InferenceResult
{
    public EvaluationResult evaluation;
    public string explanation;
}

/// <summary>
/// 用于从LLM返回的Quiz判断JSON中解析数据的临时类。
/// </summary>
[System.Serializable]
public class QuizJudgeResponse
{
    public bool is_correct;
    public string explanation;
}

/// <summary>
/// 用于从LLM返回的最终陈述评分JSON中解析数据的临时类。
/// </summary>
[System.Serializable]
public class FinalVerdictResponse
{
    [System.Serializable]
    public struct MatchedPoint
    {
        public string point;
        public bool matched;
    }
    public MatchedPoint[] matches;
}

/// <summary>
/// 代表一条完整的、已记录的问答历史。
/// </summary>
[System.Serializable]
public class DialogueRecord
{
    public string clueID; // 这条记录属于哪个线索
    public string question; // 玩家的问题
    public InferenceResult result; // 最终的推理结果 (包含评估和解释)

    public DialogueRecord(string clueId, string question, InferenceResult result)
    {
        this.clueID = clueId;
        this.question = question;
        this.result = result;
    }
} 