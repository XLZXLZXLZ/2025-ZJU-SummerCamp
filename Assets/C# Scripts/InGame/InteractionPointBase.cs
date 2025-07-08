using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Game.InGame
{
    /// <summary>
    /// 所有交互点的基类，现在继承自RevealableBase
    /// </summary>
    public abstract class InteractionPointBase : RevealableBase
    {
        [Header("交互点 触发设置")]
        [Tooltip("当此交互点被完成后，需要揭示的其他对象（可以是其他交互点，也可以是场景区域等）。")]
        [SerializeField] private List<RevealableBase> nextObjectsToReveal;
        
        /// <summary>
        /// 定义交互点如何被隐藏：通过将其缩放到0。
        /// </summary>
        protected override void PrepareForReveal()
        {
            transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 以放大动画形式显示此交互点。
        /// </summary>
        public override void Show()
        {
            gameObject.SetActive(true);
            transform.DOScale(1f, appearDuration).SetEase(appearEase);
        }

        /// <summary>
        /// 触发与之关联的下一个对象的出现。
        /// </summary>
        protected void TriggerNextObjects()
        {
            if (nextObjectsToReveal == null || nextObjectsToReveal.Count == 0) return;

            foreach (var obj in nextObjectsToReveal)
            {
                if (obj != null)
                {
                    obj.Show();
                }
            }
        }
        
        #if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (nextObjectsToReveal == null || nextObjectsToReveal.Count == 0) return;
            
            Gizmos.color = Color.cyan; // 使用新的颜色以作区分
            foreach (var obj in nextObjectsToReveal)
            {
                if (obj != null)
                {
                    // 为了让箭头更明显，稍微偏移一下终点
                    Vector3 endPosition = obj.transform.position;
                    Vector3 direction = (endPosition - transform.position).normalized;
                    Gizmos.DrawLine(transform.position, endPosition - direction * 0.3f);
                    // 在终点画一个小箭头
                    Gizmos.DrawLine(endPosition - direction * 0.3f, endPosition - direction * 0.5f + Quaternion.Euler(0, 0, 30) * direction * 0.2f);
                    Gizmos.DrawLine(endPosition - direction * 0.3f, endPosition - direction * 0.5f + Quaternion.Euler(0, 0, -30) * direction * 0.2f);
                }
            }
        }
        #endif
    }
} 