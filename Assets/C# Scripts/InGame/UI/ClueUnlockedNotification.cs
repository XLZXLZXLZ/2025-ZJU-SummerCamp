using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 一个短暂显示的UI提示，告知玩家解锁了新线索，并展示其核心内容。
/// </summary>
public class ClueUnlockedNotification : PopupWindowBase
{
    [Header("UI组件引用")]
    [SerializeField] private Image clueImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    
    [Header("通知设置")]
    [Tooltip("通知显示的时长（秒）")]
    [SerializeField] private float displayDuration = 4f;

    /// <summary>
    /// 显示解锁通知。
    /// </summary>
    /// <param name="unlockedClue">已解锁的线索数据对象。</param>
    public void ShowNotification(ClueSO unlockedClue)
    {
        if(unlockedClue == null || clueImage == null || titleText == null || descriptionText == null)
        {
            Debug.LogError("解锁通知UI的引用不完整或传入的ClueSO为空！", this);
            Destroy(gameObject); // 如果无法正确显示，直接销毁，避免空引用
            return;
        }

        // 更新UI内容
        clueImage.sprite = unlockedClue.clueSprite;
        titleText.text = unlockedClue.clueName;
        descriptionText.text = unlockedClue.clueDescriptionForPlayer;

        base.Show(); // 调用基类的飞入动画
        StartCoroutine(HideAfterDelay());
    }

    // 在指定延迟后自动隐藏
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        base.Hide(); // 调用基类的飞出动画
    }
} 