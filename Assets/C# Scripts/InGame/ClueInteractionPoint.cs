using UnityEngine;

/// <summary>
/// 挂载在2D游戏场景中的可交互物体上。
/// 点击后会解锁一个线索，并播放特效后自我销毁。
/// </summary>
[RequireComponent(typeof(Collider2D))] // 确保物体可以被点击 (2D)
public class ClueInteractionPoint : MonoBehaviour
{
    [Header("线索配置")]
    [Tooltip("点击此物体后要解锁的线索ID。必须与ClueUIPanel中列表里的Clue ID一致。")]
    [SerializeField] private string clueIDToUnlock;

    [Header("效果配置")]
    [Tooltip("发现线索时要实例化的粒子效果预制件")]
    [SerializeField] private GameObject discoveryParticlePrefab;

    private bool _isInteracted = false;

    // 当鼠标点击该物体的Collider时，此方法会被自动调用
    private void OnMouseDown()
    {
        // 防止重复点击触发
        if (_isInteracted) return;
        _isInteracted = true;

        // 1. 在场景中找到UI面板的实例，并调用其解锁方法
        ClueUIPanel cluePanel = FindObjectOfType<ClueUIPanel>();
        if (cluePanel != null)
        {
            if (!string.IsNullOrEmpty(clueIDToUnlock))
            {
                cluePanel.UnlockClue(clueIDToUnlock);
                Debug.Log($"交互点被点击，请求解锁线索: {clueIDToUnlock}");
            }
            else
            {
                Debug.LogWarning("这个交互点没有配置要解锁的Clue ID！", this);
            }
        }
        else
        {
            Debug.LogError("场景中找不到有效的 ClueUIPanel 实例！", this);
        }

        // 2. 如果配置了粒子效果，就在当前位置实例化一个
        if (discoveryParticlePrefab != null)
        {
            Instantiate(discoveryParticlePrefab, transform.position, Quaternion.identity);
        }

        // 3. 完成使命，摧毁自身
        Destroy(gameObject);
    }
} 