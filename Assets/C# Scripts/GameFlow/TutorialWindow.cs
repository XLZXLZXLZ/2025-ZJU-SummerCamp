using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameFlow
{
    /// <summary>
    /// 负责展示游戏玩法介绍的UI窗口，支持翻页。
    /// </summary>
    public class TutorialWindow : PopupWindowBase
    {
        [Header("教程页面")]
        [Tooltip("将所有教程页面的GameObject拖到这里。")]
        [SerializeField] private List<GameObject> pages;

        [Header("UI按钮")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;

        private int currentPageIndex = 0;

        protected override void Awake()
        {
            base.Awake(); // 调用基类的Awake
            
            if(nextButton) nextButton.onClick.AddListener(ShowNextPage);
            if(prevButton) prevButton.onClick.AddListener(ShowPreviousPage);
            if(closeButton) closeButton.onClick.AddListener(Hide); // 关闭按钮直接调用基类的Hide方法

            // 订阅游戏开始事件，当游戏开始时自动显示本窗口
            GameManager.OnGameStarted += Show;
        }

        protected void OnDestroy()
        {
            // 在对象销毁时，确保取消订阅，防止内存泄漏
            GameManager.OnGameStarted -= Show;
            
            if(nextButton) nextButton.onClick.RemoveListener(ShowNextPage);
            if(prevButton) prevButton.onClick.RemoveListener(ShowPreviousPage);
            if(closeButton) closeButton.onClick.RemoveListener(Hide);
        }

        public override void Show()
        {
            base.Show();
            currentPageIndex = 0;
            UpdatePages();
        }

        private void ShowNextPage()
        {
            if (currentPageIndex < pages.Count - 1)
            {
                currentPageIndex++;
                UpdatePages();
            }
        }

        private void ShowPreviousPage()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdatePages();
            }
        }

        /// <summary>
        /// 根据当前页面索引，更新所有页面的显示状态和按钮的交互状态。
        /// </summary>
        private void UpdatePages()
        {
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].SetActive(i == currentPageIndex);
            }

            if (prevButton)
            {
                prevButton.interactable = (currentPageIndex > 0);
            }
            if (nextButton)
            {
                nextButton.interactable = (currentPageIndex < pages.Count - 1);
            }
        }
    }
} 