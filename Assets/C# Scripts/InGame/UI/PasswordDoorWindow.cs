using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.UI
{
    public class PasswordDoorWindow : PopupWindowBase
    {
        [Header("密码门 UI 组件")]
        [SerializeField] private InputField passwordInputField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text hintText; // 用于显示诗歌等提示

        [Header("密码设置")]
        [SerializeField] private string correctPassword;

        /// <summary>
        /// 当输入正确密码时触发的事件
        /// </summary>
        public event Action OnPasswordCorrect;

        protected override void Awake()
        {
            base.Awake(); // 调用基类的Awake，确保位置被正确初始化

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(CheckPassword);
            }

            if (passwordInputField != null)
            {
                // 允许按回车键提交
                passwordInputField.onEndEdit.AddListener(OnInputEndEdit);
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(CheckPassword);
            }
            if (passwordInputField != null)
            {
                passwordInputField.onEndEdit.RemoveListener(OnInputEndEdit);
            }
        }
        
        /// <summary>
        /// 配置密码门
        /// </summary>
        /// <param name="password">正确的密码</param>
        /// <param name="hint">显示的提示文本（如诗歌）</param>
        public void Configure(string password, string hint = "")
        {
            correctPassword = password;
            if (hintText != null)
            {
                hintText.text = hint;
            }
        }

        private void OnInputEndEdit(string input)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckPassword();
            }
        }

        private void CheckPassword()
        {
            if (passwordInputField.text == correctPassword)
            {
                OnPasswordCorrect?.Invoke();
                Hide(); // 密码正确，调用基类的Hide()飞出
            }
            else
            {
                // 密码错误，清空输入框并晃动窗口提示
                passwordInputField.text = "";
                panelRectTransform.DOShakeAnchorPos(animationDuration, new Vector2(15, 0), 10, 90, false, true);
            }
        }
        
        /// <summary>
        /// 打开密码窗口
        /// </summary>
        public void Open()
        {
            // 每次打开时清空输入框
            if (passwordInputField != null)
            {
                passwordInputField.text = "";
            }
            Show(); // 调用基类的Show()飞入
        }
    }
} 