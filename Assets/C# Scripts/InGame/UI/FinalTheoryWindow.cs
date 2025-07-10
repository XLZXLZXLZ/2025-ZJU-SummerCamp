using Game.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BigBata.InGame.UI
{
    public class FinalTheoryWindow : PasswordDoorWindow
    {
        [Header("UI References")]
        [SerializeField] private InputField theoryInputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private Text feedbackText;

        [Header("Settings")]
        [SerializeField] private float successThreshold = 0.8f;
        [Tooltip("【重要】设置最终理论需要回答正确的关键点，用于引导AI进行更准确的评估。")]
        [SerializeField] private List<string> keyPointsForTheory = new List<string>
        {
            "真正的画家利奥已死，现在的“利奥”是一个AI。",
            "伊芙琳是利奥的爱人，她策划了“凤凰计划”来延续利奥的艺术生命，但她也已去世。",
            "本次画展是AI的最后一次画展，核心作品《我，诞生》象征着AI自我意识的觉醒。"
        };
        
        private InferenceService _inferenceService;
        private bool _isChecking = false;

        int tryCounts = 0;

        protected override void Awake()
        {
            base.Awake();
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            _inferenceService = new InferenceService();
        }

        private void Update()
        {
            if (tryCounts > 3)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    GameManager.Instance.StartEndingSequence();
                }
            }
        }

        public void Show(string title)
        {
        }

        private void OnSubmitButtonClicked()
        {
            if (_isChecking) return;
            
            string playerTheory = theoryInputField.text;
            if (string.IsNullOrWhiteSpace(playerTheory))
            {
                feedbackText.text = "请输入你的最终猜测。";
                return;
            }

            _isChecking = true;
            submitButton.interactable = false;
            feedbackText.text = "正在分析你的理论...";

            _inferenceService.EvaluateFinalTheory(playerTheory, keyPointsForTheory, (result) =>
            {
                if (result.similarity >= successThreshold)
                {
                    feedbackText.text = $"分析完成：相似度 {result.similarity:P0}。推理正确！";
                    // 直接调用GameManager的通关函数
                    GameManager.Instance.StartEndingSequence();
                }
                else
                {
                    tryCounts++;
                    
                    // 如果相似度为0且有错误原因，则判定为请求出错
                    if (result.similarity == 0 && !string.IsNullOrEmpty(result.reason))
                    {
                        feedbackText.text = $"分析时出现错误";
                    }
                    else
                    {
                        feedbackText.text = $"分析完成：相似度 {result.similarity:P0}。请再试试?";
                        if (tryCounts > 3)
                        {
                            feedbackText.text += "\n(本段难度较高，你可以使用回车键直接跳过此段进入结局)";
                        }
                    }

                    
                    _isChecking = false;
                    submitButton.interactable = true;
                }
            });
        }
    }
} 