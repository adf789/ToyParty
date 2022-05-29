using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GemAnimation : MonoBehaviour
{
    private static GemAnimation instance;
    [SerializeField] private bool isPlayAnim;
    private Dictionary<Gem, Slot> movingGems = new Dictionary<Gem, Slot>();

    public static GemAnimation Instance
    {
        get
        {
            return instance;
        }
    }
    public bool IsPlayAnim { get => isPlayAnim || movingGems.Count != 0; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
    }

    public void PlayExchangeGem(Gem fromGem, Gem toGem, UnityAction resultAction = null)
    {
        StartCoroutine(GemExchangingAnim(fromGem, toGem, resultAction));
    }

    IEnumerator GemExchangingAnim(Gem fromGem, Gem toGem, UnityAction resultAction = null)
    {
        isPlayAnim = true;
        Transform fromTransform = fromGem.transform;
        Transform toTransform = toGem.transform;

        while (true)
        {
            fromTransform.localPosition = Vector3.MoveTowards(fromTransform.localPosition, Vector3.zero, 10 * Time.deltaTime);
            toTransform.localPosition = Vector3.MoveTowards(toTransform.localPosition, Vector3.zero, 10 * Time.deltaTime);

            if (fromTransform.localPosition.Equals(Vector3.zero) && toTransform.localPosition.Equals(Vector3.zero)) break;

            yield return null;
        }

        resultAction?.Invoke();
        isPlayAnim = false;
    }

    public void MoveToEmptySlotForUpLine(Slot slot)
    {
        Slot upSlot = slot.GetNearSlot(Slot.Direction.Up);

        while (upSlot != null)
        {
            if(upSlot.haveGem != null)
            {
                MoveGemToEmptySlot(upSlot);
            }

            Slot prevSlot = upSlot;
            upSlot = upSlot.GetNearSlot(Slot.Direction.Up);
            upSlot = upSlot?? prevSlot.GetNearSlot(Slot.Direction.Up_Left);
            upSlot = upSlot ?? prevSlot.GetNearSlot(Slot.Direction.Up_Right);
        }
    }

    public void MoveToEmptySlot(Gem gem)
    {
        if (movingGems.ContainsKey(gem)) return;
        movingGems.Add(gem, null);

        StartCoroutine(GemMovingAnim(gem));
    }

    public void MoveGemToEmptySlot(Slot fromSlot)
    {
        Gem gem = fromSlot.ReleaseGem();
        if (gem == null) return;

        if (movingGems.ContainsKey(gem)) return;
        movingGems.Add(gem, fromSlot);

        StartCoroutine(GemMovingAnim(gem, fromSlot));
    }

    IEnumerator GemMovingAnim(Gem gem, Slot startSlot = null)
    {
        Slot nextSlot = null;
        Slot curSlot = startSlot;

        Transform gemTransform = gem.transform;

        while (true)
        {
            if (nextSlot == null || gemTransform.position == nextSlot.transform.position)
            {
                if(nextSlot != null) curSlot = nextSlot;

                do
                {
                    nextSlot = GetNextMovingSlot(curSlot);
                    yield return null;
                } while (nextSlot != null && !nextSlot.TryReservedForGem());

                if (nextSlot == null)
                {
                    curSlot.SetGem(gem);
                    curSlot.ReleaseReserved();
                    break;
                }
                else
                {
                    nextSlot.TryReservedForGem();
                    if (curSlot != null) curSlot.ReleaseReserved();
                }
            }

            gemTransform.position = Vector3.MoveTowards(gemTransform.position, nextSlot.transform.position, 10 * Time.deltaTime);
            yield return null;
        }

        movingGems.Remove(gem);
    }

    private Slot GetNextMovingSlot(Slot slot)
    {
        if (slot == null) return PuzzleSearch.Instance.RootSlot;

        Slot downSlot = slot.GetNearSlot(Slot.Direction.Down);
        if(downSlot != null)
        {
            if (downSlot.haveGem == null) return downSlot;
        }

        Slot leftSlot = slot.GetNearSlot(Slot.Direction.Down_Left);
        Slot rightSlot = slot.GetNearSlot(Slot.Direction.Down_Right);
        if ((leftSlot == null || leftSlot.haveGem != null) && (rightSlot == null || rightSlot.haveGem != null)) return null;
        else if (leftSlot == null || leftSlot.haveGem != null) return rightSlot;
        else if (rightSlot == null || rightSlot.haveGem != null) return leftSlot;

        return Random.Range(0, 2) == 0 ? leftSlot : rightSlot;
    }
}
