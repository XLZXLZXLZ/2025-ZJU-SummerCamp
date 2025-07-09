using System;
using System.Text;
using UnityEngine;
using BigBata.Data;

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
        // 2. 构建核心Prompt
        var prompt = $@"
# 角色与任务
你是一个侦探游戏中的AI助手“答案之书”。你的任务是根据我提供的【故事最终真相】和【当前线索的核心事实】，来评估【玩家的提问】。

# 核心规则
1.  **扮演严格的逻辑裁判**：你的唯一任务是判断【玩家的提问】作为一句**陈述句**，其事实真伪。你不是一个乐于助人的对话助手。
2.  **严格基于事实进行真伪判断**：你的评估（CompletelyCorrect, PartiallyCorrect, Incorrect）必须**唯一**依据【当前线索的核心事实】进行逻辑判断。
3.  **明确处理否定情况**：如果【当前线索的核心事实】是一个否定事实（例如，“画架没有在使用”），而【玩家的提问】是一个肯定陈述（例如，“画架还在使用吗？”），那么这个提问的逻辑判断结果**必须**是 `Incorrect`。反之亦然。
4.  **参考最终真相**：你可以参考【故事最终真相】来理解一个问题的背景，如果一个问题与**当前线索**完全无关，则应判定为 `Irrelevant` (无关)。。
5.  **尽量给出判断**：谨慎给出 `Irrelevant`的判断，对于用户提出的比较宽泛的问题，尽量做出一个
6.  **JSON格式输出**：你的回答必须是严格的JSON格式，不包含任何Markdown标记。

---
# 背景信息

## 故事最终真相 (汤底，最高层级的全局参考)
{story.fullStorySolution}

---
# 当前检视的线索

## A. 线索核心事实 (你进行评估的主要依据)
{clue.llmPromptHint}

## B. 玩家看到的描述 (仅供你参考，以了解玩家的视角)
{clue.clueDescriptionForPlayer}

---
# 玩家的提问
""{playerQuestion}""

# 你的评估 (请严格按照下面的JSON格式输出)
```json
{{
  ""evaluation"": ""(CompletelyCorrect | PartiallyCorrect | Incorrect | Noncommittal | Irrelevant)"",
  ""explanation"": ""(对此评估的简短中文解释)""
}}
```";

        // 3. 发送请求
        LLMManager.Instance.SendRequest(prompt, 
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
        
        sb.AppendLine("你是一个侦探游戏中的“真相之书”，绝对严谨和注重逻辑。你的任务是根据背景真相和当前线索，评估玩家的提问。");
        sb.AppendLine("你必须严格遵守以下规则：");
        sb.AppendLine("1. **回答准确性**：根据背景和当前所展示的事物信息，思考玩家的提问是否是一个正确的提问，并回答玩家正确/错误，或其它枚举选项。如果玩家的提问与当前线索严重不相关，则考虑回复Irrelevant");
        sb.AppendLine("2. **逻辑一致性**：你的 `evaluation`（评估）结果必须与你的 `explanation`（解释）严格对应。不允许出现解释为“否”但评估为“是”的逻辑矛盾。");
        sb.AppendLine("3. **输出格式**：你的回答必须是一个JSON对象，且只包含一个JSON对象。格式为：{\"evaluation\": \"结果枚举\", \"explanation\": \"一句话解释\"}。解释内容必须简洁，不超过20个字。");
        
        sb.AppendLine("\n--- 输入信息 ---");
        sb.AppendLine($"【完整故事真相】: {fullStory}");
        sb.AppendLine($"【当前线索信息】: {clueInfo}");
        sb.AppendLine($"【玩家提问】: {question}");
        
        sb.AppendLine("\n--- 结果枚举选项 ---");
        sb.AppendLine("CompletelyCorrect: 提问完全正确。");
        sb.AppendLine("PartiallyCorrect: 提问不包含任何错误，但正确得不够完整。");
        sb.AppendLine("Incorrect: 提问存在错误。");
        sb.AppendLine("Noncommittal: 根据已知信息，无法判断对错。");
        sb.AppendLine("Irrelevant: 提问与当前线索无关，或问题格式无效。");
        
        sb.AppendLine("\n请根据以上规则，输出你的JSON判断结果：");

        return sb.ToString();
    }
    
    private InferenceResult ParseAndValidateResponse(string jsonResponse)
    {
        try
        {
            // 步骤1: 清理字符串
            string cleanedJson = CleanLLMResponse(jsonResponse);

            // 步骤2: 解析清理后的JSON
            InferenceResponse response = JsonUtility.FromJson<InferenceResponse>(cleanedJson);
            
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
            Debug.LogError($"[InferenceService] JSON Parse Error: {ex.Message}. Raw response for debugging: {jsonResponse}");
            return new InferenceResult { evaluation = EvaluationResult.Unexpected, explanation = "Failed to parse model response." };
        }
    }

    /// <summary>
    /// 清理LLM可能返回的、包含Markdown标记的字符串。
    /// </summary>
    /// <param name="rawResponse">从LLM获取的原始响应</param>
    /// <returns>一个纯净的、可能为JSON的字符串</returns>
    private string CleanLLMResponse(string rawResponse)
    {
        string cleaned = rawResponse.Trim();
        if (cleaned.StartsWith("```json"))
        {
            cleaned = cleaned.Substring(7).TrimStart(); // 移除 "```json" 和之后可能紧跟的换行符
        }
        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3).TrimEnd(); // 移除结尾的 "```" 和之前可能紧跟的换行符
        }
        return cleaned;
    }

    public void JudgePlayerAnswer(string question, string playerAnswer, string successCondition, Action<EvaluationResult> callback)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("你是一个逻辑判断程序。你的任务是根据提供的“问题”、“玩家的回答”和“评判标准”，判断玩家的回答是否满足标准。");
        prompt.AppendLine("你的回答必须遵循以下规则：");
        prompt.AppendLine("1. 你的回答必须是一个JSON对象。");
        prompt.AppendLine("2. JSON对象必须包含两个字段：\"evaluation\" 和 \"reason\"。");
        prompt.AppendLine("3. \"evaluation\" 字段的值必须是以下五个字符串之一：\"CompletelyCorrect\", \"PartiallyCorrect\", \"Incorrect\", \"Irrelevant\", \"Noncommittal\"。");
        prompt.AppendLine("   - \"CompletelyCorrect\": 玩家的回答完全命中了评判标准。");
        prompt.AppendLine("   - \"Incorrect\": 玩家的回答未命中评判标准。");
        prompt.AppendLine("   - 在你的判断中，请优先使用 \"CompletelyCorrect\" 和 \"Incorrect\"。只在玩家的回答部分正确或与问题无关时，才使用其他选项。");
        prompt.AppendLine("4. \"reason\" 字段需要简单解释你做出判断的原因。");
        prompt.AppendLine("\n--- 输入信息 ---");
        prompt.AppendLine($"问题: \"{question}\"");
        prompt.AppendLine($"玩家的回答: \"{playerAnswer}\"");
        prompt.AppendLine($"评判标准: \"{successCondition}\"");
        prompt.AppendLine("\n--- 你的输出 ---");
        prompt.AppendLine("{\"evaluation\": \"...\", \"reason\": \"...\"}");

        LLMManager.Instance.SendRequest(prompt.ToString(), (response) =>
        {
            try
            {
                var result = JsonUtility.FromJson<InferenceResult>(CleanLLMResponse(response));
                callback?.Invoke(result.evaluation);
            }
            catch (Exception e)
            {
                Debug.LogError($"解析玩家答案评估结果失败: {e.Message}");
                callback?.Invoke(EvaluationResult.Noncommittal);
            }
        },
        (response) =>
        {
            Debug.LogError("响应失败");
        });
    }

    public void EvaluateFinalTheory(string playerTheory, Action<TheoryEvaluationResult> callback)
    {
        var storyBlueprint = GameManager.Instance.currentStory;
        if (storyBlueprint == null || string.IsNullOrEmpty(storyBlueprint.fullStorySolution))
        {
            Debug.LogError("未能获取完整故事背景。");
            callback?.Invoke(new TheoryEvaluationResult { similarity = 0, reason = "内部错误：无法加载故事。" });
            return;
        }

        string fullStory = storyBlueprint.fullStorySolution;

        var prompt = new StringBuilder();
        prompt.AppendLine("你是一位严格的、专业的侦探游戏裁判。你的任务是评估玩家对整个案件真相的猜测与标准答案之间的相似度。");
        prompt.AppendLine("你需要根据以下“标准答案”和“玩家的猜测”，给出一个0.0到1.0之间的浮点数“相似度”评分，并给出你评分的“理由”。");
        prompt.AppendLine("评分标准：");
        prompt.AppendLine("- 1.0: 玩家的猜测完全准确，所有关键情节、动机和人物关系都正确。");
        prompt.AppendLine("- 0.7-0.9: 玩家的猜测大致正确，抓住了核心真相，但可能遗漏了一些次要细节或在动机上有轻微偏差。");
        prompt.AppendLine("- 0.4-0.6: 玩家的猜测部分正确，理解了故事的某些片段，但对核心情节有重大误解。");
        prompt.AppendLine("- 0.0-0.3: 玩家的猜测基本上是错误的。");
        prompt.AppendLine("\n--- 标准答案 ---");
        prompt.AppendLine(fullStory);
        prompt.AppendLine("\n--- 玩家的猜测 ---");
        prompt.AppendLine(playerTheory);
        prompt.AppendLine("\n--- 你的输出 ---");
        prompt.AppendLine("请严格按照以下JSON格式输出你的评判结果，不要添加任何额外的解释：");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"similarity\": <你的评分, 一个浮点数>,");
        prompt.AppendLine("  \"reason\": \"<你做出该评分的简要理由>\"");
        prompt.AppendLine("}");

        LLMManager.Instance.SendRequest(prompt.ToString(),
            (response) =>
            {
                try
                {
                    var result = JsonUtility.FromJson<TheoryEvaluationResult>(CleanLLMResponse(response));
                    callback?.Invoke(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析最终理论评估结果失败: {e.Message}");
                    callback?.Invoke(new TheoryEvaluationResult { similarity = 0, reason = "无法解析裁判的反馈。" });
                }
            },
            (response) =>
            {
                Debug.LogError("最终理论评估请求响应失败");
                callback?.Invoke(new TheoryEvaluationResult { similarity = 0, reason = "与裁判的连接中断。" });
            });
    }
} 