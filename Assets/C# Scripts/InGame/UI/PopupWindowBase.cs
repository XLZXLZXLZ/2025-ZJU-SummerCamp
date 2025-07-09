using UnityEngine;
using DG.Tweening;

/// <summary>
/// 所有可飞入/飞出式UI窗口的基类。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public abstract class PopupWindowBase : MonoBehaviour
{
    [Header("动画基类设置")]
    [SerializeField] protected float animationDuration = 0.4f;
    [SerializeField] protected Ease openEase = Ease.OutBack;
    [SerializeField] protected Ease closeEase = Ease.InBack;

    protected RectTransform panelRectTransform;
    protected Vector2 onScreenPosition;
    protected Vector2 offScreenPosition;
    
    /// <summary>
    /// 窗口当前是否可见。
    /// </summary>
    public bool IsVisible => gameObject.activeSelf;

    protected virtual void Awake()
    {
        panelRectTransform = GetComponent<RectTransform>();
        
        // 记录在编辑器里设计好的、显示在屏幕上的位置
        onScreenPosition = panelRectTransform.anchoredPosition;
        
        // 计算出屏幕外的位置（这里假设从下方飞入）
        offScreenPosition = new Vector2(onScreenPosition.x, -Screen.height / 2 - panelRectTransform.rect.height);

        // 启动时，将自己放置在屏幕外并隐藏
        panelRectTransform.anchoredPosition = offScreenPosition;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示窗口（飞入）。
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
        panelRectTransform.DOAnchorPos(onScreenPosition, animationDuration).SetEase(openEase);
    }

    /// <summary>
    /// 隐藏窗口（飞出）。
    /// </summary>
    public virtual void Hide()
    {
        panelRectTransform.DOAnchorPos(offScreenPosition, animationDuration).SetEase(closeEase)
            .OnComplete(() => gameObject.SetActive(false));
    }
} 