using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.UI
{
    /// <summary>
    /// 一个用于开放式问题问答的UI窗口。
    /// 玩家输入文字答案，通过InferenceService判断答案是否正确。
    /// </summary>
    public class QuizWindow : PasswordDoorWindow
    {
        [Header("问答设置")]
        [Tooltip("需要向玩家提出的问题")]
        [SerializeField] private string question;

        [Tooltip("用于提示LLM如何判断对错的关键标准")]
        [TextArea(3, 5)]
        [SerializeField] private string successConditionHint;
        
        private bool _isChecking = false;
        private InferenceService _inferenceService;

        protected override void Awake()
        {
            base.Awake();

            _inferenceService = new InferenceService();
        }

        /// <summary>
        /// 配置问答窗口
        /// </summary>
        /// <param name="newQuestion">要显示的问题</param>
        /// <param name="newHint">给玩家的提示文本</param>
        /// <param name="newSuccessCondition">给LLM的评判标准</param>
        public void Configure(string newQuestion, string newHint, string newSuccessCondition)
        {
            question = newQuestion;
            successConditionHint = newSuccessCondition;
            
            // 使用基类中的 hintText 来显示给玩家的提示
            if (hintText != null)
            {
                hintText.text = newHint;
            }
        }

        /// <summary>
        /// 重写基类的密码检查逻辑，改为调用LLM服务进行判断
        /// </summary>
        protected override void CheckPassword()
        {
            if (_isChecking) return;

            string playerAnswer = passwordInputField.text;
            if (string.IsNullOrWhiteSpace(playerAnswer))
            {
                // 如果玩家没有输入，则轻微晃动提示
                panelRectTransform.DOShakeAnchorPos(animationDuration / 2, new Vector2(10, 0), 5, 90, false, true);
                return;
            }

            _isChecking = true;
            confirmButton.interactable = false; // 检查期间禁用按钮，防止重复提交

            // 调用推理服务来判断玩家的答案
            _inferenceService.JudgePlayerAnswer(
                question,
                playerAnswer,
                successConditionHint,
                (result) => {
                    _isChecking = false;
                    confirmButton.interactable = true; // 恢复按钮

                    if (result == EvaluationResult.CompletelyCorrect)
                    {
                        // 答案正确，触发事件并关闭窗口
                        OnPasswordCorrect?.Invoke();
                        Hide();
                    }
                    else
                    {
                        // 答案错误，晃动窗口并清空输入框
                        passwordInputField.text = "";
                        panelRectTransform.DOShakeAnchorPos(animationDuration, new Vector2(15, 0), 10, 90, false, true);
                    }
                }
            );
        }
    }
} 