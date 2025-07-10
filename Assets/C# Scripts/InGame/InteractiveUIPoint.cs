using Game.UI;
using UnityEngine;

namespace Game.InGame
{
    /// <summary>
    /// 一个通用的交互点，用于打开一个可交互的UI窗口（如密码门）。
    /// </summary>
    public class InteractiveUIPoint : InteractionPointBase
    {
        [Header("通用交互点设置")]
        [Tooltip("点击后要打开的UI窗口。这个UI窗口需要挂载一个继承自PopupWindowBase的脚本。")]
        [SerializeField] private PopupWindowBase interactiveUI;
        
        [Tooltip("【可选】当交互成功完成后，在原地生成的粒子效果预制件。")]
        [SerializeField] private GameObject completionParticlePrefab;

        private bool isCompleted = false;

        private void OnMouseDown()
        {
            if (interactiveUI == null || isCompleted) return;

            // 打开UI窗口
            interactiveUI.Show();
            
            // 特殊处理：如果这个UI是密码门，我们需要订阅它的成功事件
            if (interactiveUI is PasswordDoorWindow passwordWindow)
            {
                // 使用 += 来订阅事件，但为了防止重复订阅，最好先-=
                passwordWindow.OnPasswordCorrect -= HandlePasswordCorrect;
                passwordWindow.OnPasswordCorrect += HandlePasswordCorrect;
            }
            else
            {
                // 如果不是密码门这种有“完成”状态的窗口，
                // 那么点击后就直接触发后续对象，并且禁用自己。
                TriggerNextObjectsAndDisable();
            }
        }

        private void HandlePasswordCorrect()
        {
            if (isCompleted) return;
            isCompleted = true;

            // 1. 触发后续对象
            TriggerNextObjects();

            // 2. 实例化粒子效果
            if (completionParticlePrefab != null)
            {
                Instantiate(completionParticlePrefab, transform.position, Quaternion.identity);
            }
            
            // 3. 清理工作：取消订阅事件
            if (interactiveUI is PasswordDoorWindow passwordWindow)
            {
                passwordWindow.OnPasswordCorrect -= HandlePasswordCorrect;
            }

            // 4. 摧毁自身
            Destroy(gameObject);
        }

        private void TriggerNextObjectsAndDisable()
        {
            if (isCompleted) return;
            isCompleted = true;
            
            // 触发后续对象
            TriggerNextObjects();

            // 禁用自身，防止重复交互。这里不销毁，因为UI可能还需要它
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }
} 