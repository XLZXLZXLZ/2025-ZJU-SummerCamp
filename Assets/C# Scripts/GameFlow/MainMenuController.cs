
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameFlow
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject introAnimationObject;

        [Header("Animation Settings")]
        [SerializeField] private float initialDelay = 2.0f;
        [SerializeField] private float fadeOutDuration = 1.0f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            // 绑定按钮事件
            startButton.onClick.AddListener(OnStartButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        private void Start()
        {
            // 确保IntroAnimation在开始时是禁用的
            if (introAnimationObject != null)
            {
                introAnimationObject.SetActive(false);
            }

            // 开始时禁止交互，并启动延迟协程
            canvasGroup.interactable = false;
            StartCoroutine(EnableInteractionAfterDelay());
        }

        private IEnumerator EnableInteractionAfterDelay()
        {
            yield return new WaitForSeconds(initialDelay);
            canvasGroup.interactable = true;
            Debug.Log("主菜单按钮已启用。");
        }

        private void OnStartButtonClicked()
        {
            // 防止重复点击
            canvasGroup.interactable = false;
            StartCoroutine(StartGameSequence());
        }

        private IEnumerator StartGameSequence()
        {
            Debug.Log("开始游戏... 主菜单淡出。");
            float time = 0;
            while (time < fadeOutDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, time / fadeOutDuration);
                time += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0;

            // 激活入场动画
            if (introAnimationObject != null)
            {
                introAnimationObject.SetActive(true);
                Debug.Log("入场动画已激活。");
            }

            // 禁用主菜单
            gameObject.SetActive(false);
        }

        private void OnExitButtonClicked()
        {
            Debug.Log("退出游戏。");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
} 