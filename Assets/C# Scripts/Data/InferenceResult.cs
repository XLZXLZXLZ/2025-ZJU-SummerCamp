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