using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 一个用于测试InferenceService核心功能的测试脚本。
/// </summary>
public class LLMTest : MonoBehaviour
{
    [Header("故事数据")]
    [Tooltip("游戏的主背景故事")]
    [SerializeField] private StoryBlueprintSO storyBlueprint;
    [Tooltip("当前要测试的线索")]
    [SerializeField] private ClueSO currentClue;

    [Header("UI Elements")]
    [Tooltip("用户输入消息的输入框")]
    [SerializeField] private InputField messageInputField;
    [Tooltip("用于显示LLM回复的文本框")]
    [SerializeField] private Text responseText;
    [Tooltip("用于发送消息的按钮")]
    [SerializeField] private Button sendButton;

    // 推理服务的实例
    private InferenceService _inferenceService;

    void Start()
    {
        // 初始化服务
        _inferenceService = new InferenceService();

        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClick);
        }
        else
        {
            Debug.LogError("UI Button 未在 Inspector 中分配。");
        }

        if (storyBlueprint == null || currentClue == null)
        {
            Debug.LogError("Story Blueprint 或 Current Clue 未在 Inspector 中分配！");
        }

        if (messageInputField == null)
        {
            Debug.LogError("Message Input Field 未在 Inspector 中分配。");
        }
        
        if (responseText == null)
        {
            Debug.LogWarning("Response Text 未在 Inspector 中分配，响应将只打印在控制台。");
        }
    }

    /// <summary>
    /// 当发送按钮被点击时调用
    /// </summary>
    private void OnSendButtonClick()
    {
        string message = messageInputField.text;

        // 确保所有必需的数据都已在Inspector中设置
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("输入框为空，请输入消息。");
            return;
        }
        if (storyBlueprint == null || currentClue == null)
        {
            Debug.LogError("Story Blueprint 或 Current Clue 未在 Inspector 中分配！");
            return;
        }

        Debug.Log($"正在针对线索 '{currentClue.clueName}' 提问: {message}");
        if (responseText != null) responseText.text = "思考中...";
        
        // 调用InferenceService
        _inferenceService.AskQuestionAboutClue(storyBlueprint, currentClue, message, 
            (result) => 
            {
                // 将强类型的结果格式化为字符串
                string formattedResponse = $"评估结果: 【{result.evaluation}】\n解释: {result.explanation}";
                Debug.Log(formattedResponse);

                if (responseText != null)
                {
                    responseText.text = formattedResponse;
                }
            }
        );
        
        // 发送后清空输入框
        messageInputField.text = "";
    }
}
