using Game.UI;
using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace Game.InGame
{
    /// <summary>
    /// 代表一个可交互的线索点。
    /// </summary>
    public class ClueInteractionPoint : InteractionPointBase
    {
        [Header("线索点设置")]
        [SerializeField] private string clueIDToUnlock;
        
        [Header("效果配置")]
        [Tooltip("发现线索时要实例化的粒子效果预制件")]
        [SerializeField] private GameObject discoveryParticlePrefab;

        [SerializeField] private float durationToOpenUI = 0.5f;

        private bool isTriggered = false;

        private void OnMouseDown()
        {
            if (isTriggered) return;
            isTriggered = true; // 立即设置标志位，防止重复进入协程

            StartCoroutine(InteractionRoutine());
        }

        private IEnumerator InteractionRoutine()
        {
            // 1. 立即触发下一个关联的对象
            TriggerNextObjects();
            
            // 2. 播放粒子效果
            if (discoveryParticlePrefab != null)
            {
                Instantiate(discoveryParticlePrefab, transform.position, Quaternion.identity);
            }
            
            // 3. 禁用碰撞体并播放消失动画
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            // 使用基类定义的动画参数，实现平滑缩小至消失
            transform.DOScale(0, appearDuration).SetEase(appearEase)
                .OnComplete(() => gameObject.SetActive(false)); // 动画结束后隐藏自己

            // 4. 等待一小段时间，让UI有时间打开
            yield return new WaitForSeconds(0.3f); 
            
            // 5. 找到UI管理器并打开线索面板到指定页面
            ClueUIPanel cluePanel = ClueUIPanel.Instance;
            if (cluePanel != null && !string.IsNullOrEmpty(clueIDToUnlock))
            {
                cluePanel.OpenAndShowClue(clueIDToUnlock);
            }
            else
            {
                Debug.LogError("场景中找不到ClueUIPanel或此交互点未配置ClueID！", this);
            }
        }
    }
} 