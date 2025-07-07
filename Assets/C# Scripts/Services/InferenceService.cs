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
        // 2. 构建核心Prompt
        var prompt = $@"
# 角色与任务
你是一个侦探游戏中的AI助手“答案之书”。你的任务是根据我提供的【故事最终真相】和【当前线索的核心事实】，来评估【玩家的提问】。

# 核心规则
1.  **判断严格基于当前线索**：你的评估（CompletelyCorrect, PartiallyCorrect, Incorrect）必须**主要**依据【当前线索的核心事实】。
2.  **参考最终真相**：你可以参考【故事最终真相】来理解一个问题的背景，但如果一个问题的事实正确，而与**当前线索**无关，则应判定为 `Irrelevant` (无关)。这是为了引导玩家专注于当前线索。
3.  **拒绝开放性问题**：严格拒绝回答任何需要主观解释或叙事的开放性问题（例如“为什么会这样？”、“后来怎么样了？”），这类问题应被判定为 `Irrelevant`。
4.  **JSON格式输出**：你的回答必须是严格的JSON格式，不包含任何Markdown标记。

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

    #region Truth Assessment Methods

    /// <summary>
    /// 判断玩家对一个具体问题的回答是否正确（小测验模式）。
    /// </summary>
    public void JudgePlayerAnswer(string question, string correctAnswer, string playerAnswer, System.Action<bool, string> onComplete)
    {
        string prompt = BuildQuizJudgePrompt(question, correctAnswer, playerAnswer);
        LLMManager.Instance.SendRequest(prompt, 
            (responseJson) => {
                try
                {
                    string cleanedJson = CleanLLMResponse(responseJson);
                    QuizJudgeResponse response = JsonUtility.FromJson<QuizJudgeResponse>(cleanedJson);
                    onComplete?.Invoke(response.is_correct, response.explanation);
                }
                catch(System.Exception ex)
                {
                    Debug.LogError($"[InferenceService] JudgePlayerAnswer Error: {ex.Message}. Raw response: {responseJson}");
                    onComplete?.Invoke(false, "判断时发生错误。");
                }
            }, 
            (error) => {
                onComplete?.Invoke(false, "判断时发生错误。");
            }
        );
    }

    /// <summary>
    /// 对玩家的最终真相陈述进行打分。
    /// </summary>
    public void ScoreFinalVerdict(System.Collections.Generic.List<string> scoringPoints, string playerVerdict, System.Action<int, System.Collections.Generic.List<string>> onComplete)
    {
        string prompt = BuildFinalVerdictPrompt(scoringPoints, playerVerdict);
        LLMManager.Instance.SendRequest(prompt,
            (responseJson) => {
                try
                {
                    string cleanedJson = CleanLLMResponse(responseJson);
                    FinalVerdictResponse response = JsonUtility.FromJson<FinalVerdictResponse>(cleanedJson);
                    int score = 0;
                    var matchedPoints = new System.Collections.Generic.List<string>();
                    foreach(var item in response.matches)
                    {
                        if(item.matched)
                        {
                            score++;
                            matchedPoints.Add(item.point);
                        }
                    }
                    onComplete?.Invoke(score, matchedPoints);
                }
                catch(System.Exception ex)
                {
                     Debug.LogError($"[InferenceService] ScoreFinalVerdict Error: {ex.Message}. Raw response: {responseJson}");
                    onComplete?.Invoke(0, null);
                }
            },
            (error) => {
                onComplete?.Invoke(0, null);
            }
        );
    }

    private string BuildQuizJudgePrompt(string question, string correctAnswer, string playerAnswer)
    {
        return $"你是一个游戏裁判。请判断【玩家的回答】在语义上是否回答了【问题】，并且意思上是否与【标准答案】一致。你的回答必须是一个JSON，格式为：{{\"is_correct\": true/false, \"explanation\": \"一句话解释\"}}。\n\n【问题】:{question}\n【标准答案】:{correctAnswer}\n【玩家的回答】:{playerAnswer}";
    }

    private string BuildFinalVerdictPrompt(System.Collections.Generic.List<string> scoringPoints, string playerVerdict)
    {
        string pointsStr = string.Join("\", \"", scoringPoints);
        return $"你是一个游戏裁判。请仔细阅读【玩家的最终陈述】，并判断该陈述在语义上是否清晰地覆盖了【得分点列表】中的每一个要点。你的回答必须是一个JSON对象，其格式为：{{\"matches\": [ {{\"point\": \"得分点内容\", \"matched\": true/false}}, ... ]}}。\n\n【得分点列表】:[\"{pointsStr}\"]\n【玩家的最终陈述】:{playerVerdict}";
    }

    #endregion
} 