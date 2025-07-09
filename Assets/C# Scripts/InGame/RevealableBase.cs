using UnityEngine;
using Game; // 引入Game命名空间以访问GameManager

namespace Game.InGame
{
    /// <summary>
    /// 所有可被动态揭示的游戏对象的基类。
    /// 提供了统一的初始可见性控制和显示接口。
    /// </summary>
    public abstract class RevealableBase : MonoBehaviour
    {
        [Header("可揭示对象 基类设置")]
        [Tooltip("如果勾选，此对象将在游戏正式开始时（入场动画结束后）自动显示。")]
        [SerializeField] protected bool initiallyVisible = false;

        [Tooltip("对象出现时使用的动画时长。")]
        [SerializeField] protected float appearDuration = 0.5f;
        [Tooltip("对象出现时使用的动画缓动类型。")]
        [SerializeField] protected DG.Tweening.Ease appearEase = DG.Tweening.Ease.OutBack;

        protected virtual void Awake()
        {
            // 所有可揭示对象，在唤醒时都先将自己隐藏
            PrepareForReveal();
        }
        
        protected virtual void Start()
        {
            // 如果被标记为“初始可见”，则订阅GameManager的游戏开始事件
            if (initiallyVisible)
            {
                GameManager.OnGameStarted += ShowOnGameStart;
            }
        }

        private void ShowOnGameStart()
        {
            // 收到游戏开始事件后，调用自身的Show方法
            Show();
            // 调用后即可取消订阅，避免不必要的麻烦
            GameManager.OnGameStarted -= ShowOnGameStart;
        }

        protected virtual void OnDestroy()
        {
            // 在对象被销毁时，确保取消订阅，防止内存泄漏
            if (initiallyVisible)
            {
                GameManager.OnGameStarted -= ShowOnGameStart;
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