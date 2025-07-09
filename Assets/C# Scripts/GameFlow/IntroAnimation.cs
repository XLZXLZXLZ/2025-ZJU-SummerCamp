using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.GameFlow
{
    /// <summary>
    /// 负责播放游戏开场的逐段文字动画。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class IntroAnimation : MonoBehaviour
    {
        [Header("文本内容")]
        [Tooltip("需要逐段展示的文字内容。")]
        [SerializeField, TextArea(3, 5)] private string[] textSegments;

        [Header("UI组件")]
        [Tooltip("用于显示文字的UI Text组件。")]
        [SerializeField] private Text displayText;

        [Header("动画参数")]
        [Tooltip("每段文字淡入所需时间。")]
        [SerializeField] private float fadeInDuration = 1.0f;
        [Tooltip("每段文字完全显示后停留的时间。")]
        [SerializeField] private float holdDuration = 2.0f;
        [Tooltip("每段文字淡出所需时间。")]
        [SerializeField] private float fadeOutDuration = 1.0f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (displayText == null)
            {
                displayText = GetComponentInChildren<Text>();
            }
        }

        private void Start()
        {
            // 自动开始播放
            StartCoroutine(PlayAnimationRoutine());
        }

        /// <summary>
        /// 播放整个文字动画序列的协程。
        /// </summary>
        private IEnumerator PlayAnimationRoutine()
        {
            // 初始状态为完全透明
            canvasGroup.alpha = 0;
            displayText.text = "";

            // 等待一小帧确保所有初始化完成
            yield return null;

            foreach (var segment in textSegments)
            {
                // 设置文本并开始淡入
                displayText.text = segment;
                canvasGroup.DOFade(1, fadeInDuration);
                yield return new WaitForSeconds(fadeInDuration);

                // 停留
                yield return new WaitForSeconds(holdDuration);

                // 淡出
                canvasGroup.DOFade(0, fadeOutDuration);
                yield return new WaitForSeconds(fadeOutDuration);
            }
            
            // 所有动画已完成
            Debug.Log("入场动画播放完毕。");
            GameManager.Instance.GameStart();
            
            // 可以选择在此处禁用或销毁自身
            gameObject.SetActive(false);
        }
    }
} 