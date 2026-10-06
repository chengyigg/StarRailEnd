using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责显示对话分支选项按钮，并在被点击时把选择抛给 DialogueManager 处理。
/// 不关心剧情、好感度、跳转——那些都是 DialogueManager 的事。
/// </summary>
public class DialogueChoiceView : MonoBehaviour
{
    [Header("引用")]
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;

    /// <summary>某个选项被点击时触发。DialogueManager 订阅它来处理后果。</summary>
    public event Action<DialogueChoice> OnChoiceClicked;

    /// <summary>当前是否有可见选项（含灰掉不可点的）。</summary>
    public bool HasVisibleChoices { get; private set; }

    private Func<string, string> placeholderReplacer;

    public void Initialize(Func<string, string> replacer)
    {
        placeholderReplacer = replacer;
    }

    public void Show(List<DialogueChoice> choices)
    {
        Hide();
        if (choices == null || choices.Count == 0) return;
        if (choiceButtonPrefab == null || choiceContainer == null)
        {
            GameLog.Warning("[DialogueChoiceView] prefab 或 container 未挂");
            return;
        }

        bool anyShown = false;

        foreach (DialogueChoice choice in choices)
        {
            bool met = choice.CheckCondition();
            if (!met && choice.hideWhenUnmet) continue;

            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);

            var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = placeholderReplacer != null
                    ? placeholderReplacer(choice.choiceText)
                    : choice.choiceText;

            var captured = choice;
            var btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnChoiceClicked?.Invoke(captured));
                if (!met) btn.interactable = false;
            }

            anyShown = true;
        }

        HasVisibleChoices = anyShown;
    }

    public void Hide()
    {
        HasVisibleChoices = false;
        if (choiceContainer == null) return;
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }
}