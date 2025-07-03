using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LLMTest : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("对 LLMManager 实例的引用")]
    [SerializeField] private LLMManager llmManager;

    [Header("UI Elements")]
    [Tooltip("用户输入消息的输入框")]
    [SerializeField] private InputField messageInputField;
    [Tooltip("用于发送消息的按钮")]
    [SerializeField] private Button sendButton;
    [Tooltip("用于显示LLM回复的文本框")]
    [SerializeField] private Text responseText;

    void Start()
    {
        if (sendButton != null)
        {
            // 为按钮添加点击事件监听器
            sendButton.onClick.AddListener(OnSendButtonClick);
        }
        else
        {
            Debug.LogError("UI Button 未在 Inspector 中分配。");
        }

        if (llmManager == null)
        {
            Debug.LogError("LLMManager 未在 Inspector 中分配。");
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
        if (!string.IsNullOrEmpty(message) && llmManager != null)
        {
            Debug.Log("正在发送消息: " + message);
            if (responseText != null) responseText.text = "思考中...";
            
            // 调用 LLMManager 中的 PostRequest 协程，并传入回调函数
            StartCoroutine(llmManager.PostRequest(message, 
                // 成功时的回调
                (response) => {
                    Debug.Log("收到回复: " + response);
                    if (responseText != null)
                    {
                        responseText.text = response;
                    }
                },
                // 失败时的回调
                (error) => {
                    Debug.LogError("请求失败: " + error);
                    if (responseText != null)
                    {
                        responseText.text = "出现错误: " + error;
                    }
                }
            ));
            
            // 发送后清空输入框
            messageInputField.text = "";
        }
        else
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("输入框为空，请输入消息。");
            }
            if (llmManager == null)
            {
                 Debug.LogError("LLMManager 未分配，无法发送消息。");
            }
        }
    }
}
