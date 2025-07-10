using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

// --- API Data Structures ---
[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class RequestData
{
    public string model;
    public List<ChatMessage> messages;
}

[System.Serializable]
public class ResponseChoice
{
    public ChatMessage message;
}

[System.Serializable]
public class ResponseData
{
    public List<ResponseChoice> choices;
}

[System.Serializable]
public class EmbeddingRequestData
{
    public string model;
    public string input;
}

[System.Serializable]
public class EmbeddingData
{
    public List<float> embedding;
    public int index;
}

[System.Serializable]
public class EmbeddingResponseData
{
    public List<EmbeddingData> data;
}


public class LLMManager : Singleton<LLMManager>
{
    private const string ChatApiUrl = "https://api.chatanywhere.tech/v1/chat/completions";
    private const string EmbeddingApiUrl = "https://api.chatanywhere.tech/v1/embeddings";
    private string apiKey = "sk-P4Fvkft8iS5EycEUm2QJdLLDK2pKCd6mSWZMQAnhMunKQ6wd";

    [Header("LLM Settings")]
    [Tooltip("API请求的超时时间（秒）")]
    [SerializeField] private int requestTimeout = 15;
    [Tooltip("要使用的聊天模型 ID")]
    [SerializeField] private string chatModel = "gpt-3.5-turbo";
    [Tooltip("要使用的嵌入模型 ID")]
    [SerializeField] private string embeddingModel = "text-embedding-ada-002";

    [Tooltip("给大语言模型的系统级指令")]
    [SerializeField] private string systemPrompt = "You are a helpful assistant.";

    // Start is called before the first frame update
    void Start()
    {
        // 这是一个使用示例，您可以在其他脚本中调用 PostRequest
        // StartCoroutine(PostRequest("你好！"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// 向大语言模型发送消息并获取回复的公共入口点。
    /// </summary>
    /// <param name="userMessage">要发送给用户的消息</param>
    /// <param name="onComplete">成功获取回复时调用的回调，参数为回复内容</param>
    /// <param name="onError">发生错误时调用的回调，参数为错误信息</param>
    public void SendRequest(string userMessage, System.Action<string> onComplete, System.Action<string> onError)
    {
        StartCoroutine(PostRequest(userMessage, onComplete, onError));
    }

    /// <summary>
    /// 向大语言模型发送消息并获取回复的私有协程。
    /// </summary>
    private IEnumerator PostRequest(string userMessage, System.Action<string> onComplete, System.Action<string> onError)
    {
        // 1. 构建请求体
        RequestData requestData = new RequestData
        {
            model = chatModel,
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "system", content = systemPrompt },
                new ChatMessage { role = "user", content = userMessage }
            }
        };
        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

        // 2. 创建并配置 UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(ChatApiUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.timeout = requestTimeout;
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // 3. 发送请求并等待响应
            yield return request.SendWebRequest();

            // 4. 处理响应
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;

                // 在解析前打印原始响应，用于调试
                Debug.Log($"[LLMManager] Raw response received: {jsonResponse}");

                ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);
                if (responseData != null && responseData.choices != null && responseData.choices.Count > 0)
                {
                    string assistantMessage = responseData.choices[0].message.content;
                    Debug.Log("LLM Response: " + assistantMessage);
                    // 调用成功回调
                    onComplete?.Invoke(assistantMessage);
                }
                else
                {
                    string errorMsg = "API Error: Invalid response format.";
                    Debug.LogError(errorMsg);
                    Debug.LogError("API Response: " + jsonResponse);
                    // 调用错误回调
                    onError?.Invoke(errorMsg);
                }
            }
            else
            {
                string errorMsg = "API Error: " + request.error;
                Debug.LogError(errorMsg);
                Debug.LogError("API Response: " + request.downloadHandler.text);
                // 调用错误回调
                onError?.Invoke(errorMsg);
            }
        }
    }
    
    /// <summary>
    /// 获取文本的向量表示（Embedding）。
    /// </summary>
    /// <param name="text">要进行向量化的文本</param>
    /// <param name="onComplete">成功获取向量时调用的回调，参数为向量数组</param>
    /// <param name="onError">发生错误时调用的回调，参数为错误信息</param>
    public void GetEmbeddingAsync(string text, System.Action<float[]> onComplete, System.Action<string> onError)
    {
        StartCoroutine(GetEmbeddingCoroutine(text, onComplete, onError));
    }

    private IEnumerator GetEmbeddingCoroutine(string text, System.Action<float[]> onComplete, System.Action<string> onError)
    {
        // 1. 构建请求体
        EmbeddingRequestData requestData = new EmbeddingRequestData
        {
            model = embeddingModel,
            input = text
        };
        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

        // 2. 创建并配置 UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(EmbeddingApiUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.timeout = requestTimeout;
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // 3. 发送请求并等待响应
            yield return request.SendWebRequest();

            // 4. 处理响应
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"[LLMManager] Embedding Raw response received: {jsonResponse}");

                EmbeddingResponseData responseData = JsonUtility.FromJson<EmbeddingResponseData>(jsonResponse);
                if (responseData != null && responseData.data != null && responseData.data.Count > 0)
                {
                    float[] embedding = responseData.data[0].embedding.ToArray();
                    onComplete?.Invoke(embedding);
                }
                else
                {
                    string errorMsg = "API Error: Invalid embedding response format.";
                    Debug.LogError(errorMsg);
                    Debug.LogError("API Response: " + jsonResponse);
                    onError?.Invoke(errorMsg);
                }
            }
            else
            {
                string errorMsg = "API Error: " + request.error;
                Debug.LogError(errorMsg);
                Debug.LogError("API Response: " + request.downloadHandler.text);
                onError?.Invoke(errorMsg);
            }
        }
    }
}
