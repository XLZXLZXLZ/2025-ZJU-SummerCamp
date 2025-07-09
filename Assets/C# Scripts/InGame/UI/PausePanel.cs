
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BigBata.InGame.UI
{
    public class PausePanel : PopupWindowBase
    {
        [Header("UI References")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button exitButton;

        protected override void Awake()
        {
            base.Awake();
          
            // 绑定按钮事件
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        private void OnContinueButtonClicked()
        {
            // 调用基类的飞出方法
            Hide();
        }

        private void OnExitButtonClicked()
        {
            Debug.Log("从暂停菜单退出游戏。");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
} 