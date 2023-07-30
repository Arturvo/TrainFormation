using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Animator animator;
    private SoundSystem soundSystem;

    private void Awake()
    {
        GameObject soundSystemObject = GameObject.Find("SoundSystem");
        if (soundSystemObject != null)
        {
            soundSystem = soundSystemObject.GetComponent<SoundSystem>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetTrigger("MakeBig");
        if (soundSystem != null) soundSystem.PlaySound("ButtonHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetTrigger("MakeSmall");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (soundSystem != null) soundSystem.PlaySound("ButtonClick");
    }
}
