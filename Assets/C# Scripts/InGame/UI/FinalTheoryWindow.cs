using Game.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

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
        
        private InferenceService _inferenceService;
        private bool _isChecking = false;

        protected override void Awake()
        {
            base.Awake();
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            _inferenceService = new InferenceService();
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

            _inferenceService.EvaluateFinalTheory(playerTheory, (result) =>
            {
                if (result.similarity >= successThreshold)
                {
                    feedbackText.text = $"分析完成：相似度 {result.similarity:P0} 推理正确！";
                    // 直接调用GameManager的通关函数
                    GameManager.Instance.StartEndingSequence();
                }
                else
                {
                    feedbackText.text = $"分析完成：相似度 {result.similarity:P0} 请再试一次。";
                    _isChecking = false;
                    submitButton.interactable = true;
                }
            });
        }
    }
} 