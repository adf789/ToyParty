using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInteraction : MonoBehaviour
{
    private Vector2 mouseStartPos;
    [SerializeField] private Slot slot;
    [SerializeField] private Gem gem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (GemAnimation.Instance.IsPlayAnim) return;

        Slot.pickedSlot = slot;
    }

    private void OnMouseUp()
    {
        Slot.pickedSlot = null;
    }

    private void OnMouseEnter()
    {
        if (GemAnimation.Instance.IsPlayAnim) return;
        else if (Slot.pickedSlot == null) return;
        
        slot.ExchangeGem(Slot.pickedSlot);
        Slot.pickedSlot = null;
    }
}
