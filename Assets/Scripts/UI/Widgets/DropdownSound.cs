using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Dropdown))]
public class DropdownSound : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData e)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("dropdown_open");
    }
}