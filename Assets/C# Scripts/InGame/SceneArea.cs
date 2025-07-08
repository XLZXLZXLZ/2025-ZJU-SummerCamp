using UnityEngine;
using DG.Tweening;

namespace Game.InGame
{
    /// <summary>
    /// 代表一个可以被动态揭示的场景区域，通常用于地图背景等。
    /// 使用SpriteRenderer来控制整体的淡入淡出。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SceneArea : RevealableBase
    {
        private SpriteRenderer spriteRenderer;
        private bool isShown = false;

        protected override void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            base.Awake(); // 调用基类Awake来处理initiallyVisible逻辑
        }
        /// 定义此场景区域如何被隐藏：通过将其完全透明化。
        /// </summary>
        protected override void PrepareForReveal()
        {
            // 保存原始颜色，只将alpha设为0
            Color color = spriteRenderer.color;
            color.a = 0;
            spriteRenderer.color = color;
        }

        /// <summary>
        /// 以淡入动画的形式显示此场景区域。
        /// </summary>
        public override void Show()
        {
            if (isShown) return;
            isShown = true;
            
            gameObject.SetActive(true);
            // 对SpriteRenderer执行Fade动画
            spriteRenderer.DOFade(1, appearDuration).SetEase(appearEase);
        }
    }
} 