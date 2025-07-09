using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 负责在游戏开始时，将子UI元素依次飞入。
/// </summary>
public class IndexesAnimation : MonoBehaviour
{
    [Header("动画设置")]
    [Tooltip("每个子物体飞入的动画时长")]
    [SerializeField] private float animationDuration = 0.6f;
    [Tooltip("子物体之间飞入的间隔时间")]
    [SerializeField] private float delayBetweenItems = 0.15f;
    [Tooltip("动画使用的缓动效果")]
    [SerializeField] private Ease easeType = Ease.OutBack;

    // 存储子物体及其原始位置
    private readonly List<RectTransform> childRectTransforms = new List<RectTransform>();
    private readonly List<Vector2> originalPositions = new List<Vector2>();

    private void Awake()
    {
        // 找到所有子物体，记录它们的原始位置，然后将它们移到屏幕外
        // 我们假设UI的父级有一个Canvas来获取屏幕宽度，这是比较稳妥的做法
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("IndexesAnimation无法找到父级的Canvas组件!", this);
            return;
        }
        // 改为获取屏幕高度
        float moveDistance = (canvas.transform as RectTransform).rect.height;

        foreach (RectTransform child in transform)
        {
            // 只处理激活的子物体
            if (child.gameObject.activeSelf)
            {
                childRectTransforms.Add(child);
                originalPositions.Add(child.anchoredPosition);

                // 改为向上移动到屏幕外
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, child.anchoredPosition.y + moveDistance);
            }
        }
    }

    private void Start()
    {
        // 订阅游戏开始事件
        GameManager.OnGameStarted += PlayEnterAnimation;
    }

    private void OnDestroy()
    {
        // 在对象销毁时取消订阅，以防内存泄漏
        GameManager.OnGameStarted -= PlayEnterAnimation;
    }

    /// <summary>
    /// 播放子物体依次入场的动画
    /// </summary>
    private void PlayEnterAnimation()
    {
        if (childRectTransforms.Count == 0) return;

        // 使用DOTween的Sequence来创建队列动画
        Sequence sequence = DOTween.Sequence();
        
        // 依次为每个子物体创建入场动画，并插入到序列中
        for (int i = 0; i < childRectTransforms.Count; i++)
        {
            RectTransform child = childRectTransforms[i];
            Vector2 targetPosition = originalPositions[i];

            // 在序列的特定时间点插入动画，实现依次播放的效果
            // 第一个在0秒开始，第二个在delayBetweenItems秒开始，以此类推
            sequence.Insert(i * delayBetweenItems, 
                child.DOAnchorPos(targetPosition, animationDuration).SetEase(easeType)
            );
        }
        
        // 播放整个动画序列
        sequence.Play();
    }
}
