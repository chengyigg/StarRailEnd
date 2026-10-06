using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AffectionPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public Transform container;
    public GameObject slotPrefab;
    public Button closeButton;

    private List<AffectionSlotUI> slots = new List<AffectionSlotUI>();

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void Update()
    {
        if (panelRoot != null && panelRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        if (panelRoot == null) return;

        var unlocked = AffectionManager.GetUnlockedCharacters();
        EnsureSlotCount(unlocked.Count);

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < unlocked.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(unlocked[i]);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        if (PanelManager.Instance != null) PanelManager.Instance.OpenOnly(panelRoot);
        else panelRoot.SetActive(true);
    }

    void EnsureSlotCount(int count)
    {
        while (slots.Count < count)
        {
            GameObject obj = Instantiate(slotPrefab, container);
            var slot = obj.GetComponent<AffectionSlotUI>();
            if (slot == null)
            {
                Destroy(obj);
                GameLog.Error("[AffectionPanel] slotPrefab 上没有 AffectionSlotUI 组件");
                break;
            }
            slots.Add(slot);
        }
    }

    public void Close()
    {
        if (panelRoot == null) return;
        if (PanelManager.Instance != null) PanelManager.Instance.Close(panelRoot);
        else panelRoot.SetActive(false);
    }
}