using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.GameFlow
{
    public class OutroAnimation : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Text animationText;
        [SerializeField] private CanvasGroup blackScreenCanvasGroup;

        [Header("Animation Settings")]
        [TextArea(3, 5)]
        [SerializeField] private string[] textSegments;
        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float displayDuration = 3.0f;
        [SerializeField] private float fadeOutDuration = 1.5f;
        [Tooltip("动画结束后要返回的场景名称")]
        [SerializeField] private string sceneToReturn = "SampleScene";
        
        private void Start()
        {
            // 当场景加载时自动开始播放动画
            StartCoroutine(PlayAnimation());
        }

        private IEnumerator PlayAnimation()
        {
            // Start with a black screen
            blackScreenCanvasGroup.alpha = 1f;
            animationText.text = "";
            yield return new WaitForSeconds(4f);

            foreach (var segment in textSegments)
            {
                // Fade in text
                animationText.text = segment;
                yield return FadeText(1f, fadeInDuration);
                
                yield return new WaitForSeconds(displayDuration);

                // Fade out text
                yield return FadeText(0f, fadeOutDuration);
            }
            
            yield return new WaitForSeconds(1f);
            
            Debug.Log($"结局动画播放完毕，返回场景: {sceneToReturn}");
            SceneManager.LoadScene(sceneToReturn);
        }
        
        private IEnumerator FadeText(float targetAlpha, float duration)
        {
            float startAlpha = animationText.color.a;
            Color startColor = animationText.color;
            float time = 0;
            while (time < duration)
            {
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                animationText.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
                time += Time.deltaTime;
                yield return null;
            }
            animationText.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        }
    }
} 