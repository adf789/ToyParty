using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInteraction : MonoBehaviour
{
    private Vector2 mouseStartPos;
    [SerializeField] private Slot slot;

    private void OnMouseDrag()
    {
        if (BlockMover.Instance.IsMoving) return;
        else if (PuzzleCreator.Instance.IsCreating) return;
        else if (PuzzleBreaker.Instance.IsBreaking) return;

        Slot.pickedSlot = slot;
    }

    private void OnMouseUp()
    {
        Slot.pickedSlot = null;
    }

    private void OnMouseEnter()
    {
        if (BlockMover.Instance.IsMoving) return;
        else if (PuzzleCreator.Instance.IsCreating) return;
        else if (PuzzleBreaker.Instance.IsBreaking) return;
        else if (Slot.pickedSlot == null) return;
        
        slot.ExchangeBlock(Slot.pickedSlot);
        Slot.pickedSlot = null;
    }
}
