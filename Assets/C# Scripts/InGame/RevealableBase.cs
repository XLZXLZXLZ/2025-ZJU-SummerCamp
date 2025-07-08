using UnityEngine;

namespace Game.InGame
{
    /// <summary>
    /// 所有可被动态揭示的游戏对象的基类。
    /// 提供了统一的初始可见性控制和显示接口。
    /// </summary>
    public abstract class RevealableBase : MonoBehaviour
    {
        [Header("可揭示对象 基类设置")]
        [Tooltip("如果勾选，此对象将在游戏开始时就可见。")]
        [SerializeField] protected bool initiallyVisible = false;

        [Tooltip("对象出现时使用的动画时长。")]
        [SerializeField] protected float appearDuration = 0.5f;
        [Tooltip("对象出现时使用的动画缓动类型。")]
        [SerializeField] protected DG.Tweening.Ease appearEase = DG.Tweening.Ease.OutBack;

        protected virtual void Awake()
        {
            if (!initiallyVisible)
            {
                // 如果不是初始可见，则调用子类实现的准备方法
                PrepareForReveal();
            }
        }

        /// <summary>
        /// 以动画形式显示此对象。子类必须实现此方法。
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// 为对象的“揭示”（显示）做准备。
        /// 子类必须实现此方法，以定义它们是如何被“隐藏”的（例如，缩放到0，或设置为透明）。
        /// </summary>
        protected abstract void PrepareForReveal();
    }
} 