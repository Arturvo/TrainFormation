using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownButtonScript : MonoBehaviour, IPointerClickHandler
{
    private SoundSystem soundSystem;

    private void Awake()
    {
        GameObject soundSystemObject = GameObject.Find("SoundSystem");
        if (soundSystemObject != null)
        {
            soundSystem = soundSystemObject.GetComponent<SoundSystem>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (soundSystem != null) soundSystem.PlaySound("ButtonClick");
    }
}
