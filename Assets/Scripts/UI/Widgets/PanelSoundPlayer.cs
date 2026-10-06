using UnityEngine;

public class PanelSoundPlayer : MonoBehaviour
{
    void OnEnable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("panel_open");
    }

    void OnDisable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("panel_close");
    }
}