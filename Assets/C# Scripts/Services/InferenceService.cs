using System;
using System.Text;
using UnityEngine;

/// <summary>
/// 负责处理与LLM推理相关的核心逻辑。
/// 构建Prompt，并解析返回结果。
/// </summary>
public class InferenceService
{
    /// <summary>
    /// 针对一个线索进行提问。
    /// </summary>
    /// <param name="story">完整的故事背景</param>
    /// <param name="clue">当前聚焦的线索</param>
    /// <param name="playerQuestion">玩家的提问</param>
    /// <param name="onComplete">完成时的回调，返回一个强类型的InferenceResult</param>
    public void AskQuestionAboutClue(StoryBlueprintSO story, ClueSO clue, string playerQuestion, Action<InferenceResult> onComplete)
    {
        string prompt = BuildInferencePrompt(story.fullStorySolution, clue.clueDescription, playerQuestion);
        
        // 通过单例调用LLMManager
        LLMManager.Instance.PostRequest(prompt, 
            (responseJson) => {
                // 成功回调：解析并验证返回的JSON
                InferenceResult result = ParseAndValidateResponse(responseJson);
                onComplete?.Invoke(result);
            }, 
            (error) => {
                // 失败回调：直接返回Unexpected结果
                onComplete?.Invoke(new InferenceResult { evaluation = EvaluationResult.Unexpected, explanation = error });
            }
        );
    }

    private string BuildInferencePrompt(string fullStory, string clueInfo, string question)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("你是一个侦探游戏中的“真相之书”。你的任务是根据背景真相和当前线索，评估玩家的提问。");
        sb.AppendLine("你必须严格遵守以下规则：");
        sb.AppendLine("1. **相关性判断**：首先，判断【玩家提问】是否只与【当前线索信息】直接相关。如果提问超出了线索信息的范围，即使内容在【完整故事真相】中是真的，也必须判定为“无关”。");
        sb.AppendLine("2. **准确性评估**：只有当问题与线索相关时，你才需要根据【完整故事真相】来评估其准确性。");
        sb.AppendLine("3. **输出格式**：你的回答必须是一个JSON对象，且只包含一个JSON对象。格式为：{\"evaluation\": \"结果枚举\", \"explanation\": \"一句话解释\"}。解释内容必须简洁，不超过20个字。");
        
        sb.AppendLine("\n--- 输入信息 ---");
        sb.AppendLine($"【完整故事真相】: {fullStory}");
        sb.AppendLine($"【当前线索信息】: {clueInfo}");
        sb.AppendLine($"【玩家提问】: {question}");
        
        sb.AppendLine("\n--- 结果枚举选项 ---");
        sb.AppendLine("CompletelyCorrect: 提问完全正确。");
        sb.AppendLine("PartiallyCorrect: 提问部分正确，但包含错误或不完整的推断。");
        sb.AppendLine("Incorrect: 提问完全错误。");
        sb.AppendLine("Noncommittal: 根据已知信息，无法判断对错。");
        sb.AppendLine("Irrelevant: 提问与当前线索无关。");
        
        sb.AppendLine("\n请根据以上规则，输出你的JSON判断结果：");

        return sb.ToString();
    }
    
    private InferenceResult ParseAndValidateResponse(string jsonResponse)
    {
        try
        {
            // JsonUtility对最外层是数组或简单值的JSON支持不佳，但对我们的简单对象是完美的。
            InferenceResponse response = JsonUtility.FromJson<InferenceResponse>(jsonResponse);
            
            if (response == null || string.IsNullOrEmpty(response.evaluation))
            {
                // 如果JSON本身合法但内容为空或缺少关键字段
                 return new InferenceResult { evaluation = EvaluationResult.Unexpected, explanation = "Model returned a valid but empty response." };
            }

            // 将字符串安全地转换为枚举
            EvaluationResult evalResult;
            if (Enum.TryParse(response.evaluation, true, out evalResult))
            {
                return new InferenceResult { evaluation = evalResult, explanation = response.explanation };
            }
            else
            {
                // 如果evaluation字段的值不是我们定义的任何一个枚举
                return new InferenceResult { evaluation = EvaluationResult.Unexpected, explanation = $"Invalid evaluation value '{response.evaluation}' from model." };
            }
        }
        catch (Exception ex)
        {
            // 如果JSON解析本身就失败了（模型被破甲，返回了非JSON内容）
            Debug.LogError($"[InferenceService] JSON Parse Error: {ex.Message}. Raw response: {jsonResponse}");
            return new InferenceResult { evaluation = EvaluationResult.Unexpected, explanation = "Failed to parse model response." };
        }
    }
} 