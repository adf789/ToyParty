using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlockAnimation : MonoBehaviour
{
    private static BlockAnimation instance;
    [SerializeField] private bool isPlayAnim;
    private Dictionary<Block, Slot> movingBlocks = new Dictionary<Block, Slot>();

    public static BlockAnimation Instance
    {
        get
        {
            return instance;
        }
    }
    public bool IsPlayAnim { get => isPlayAnim || movingBlocks.Count != 0; }

    private void Awake()
    {
        instance = this;
    }

    public void PlayExchangeBlock(Block fromBlock, Block toBlock, UnityAction resultAction = null)
    {
        StartCoroutine(BlockExchangingAnim(fromBlock, toBlock, resultAction));
    }

    IEnumerator BlockExchangingAnim(Block fromBlock, Block toBlock, UnityAction resultAction = null)
    {
        isPlayAnim = true;
        Transform fromTransform = fromBlock.transform;
        Transform toTransform = toBlock.transform;

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
            if(upSlot.haveBlock != null)
            {
                MoveBlockToEmptySlot(upSlot);
            }

            Slot prevSlot = upSlot;
            upSlot = upSlot.GetNearSlot(Slot.Direction.Up);
            upSlot = upSlot?? prevSlot.GetNearSlot(Slot.Direction.Up_Left);
            upSlot = upSlot ?? prevSlot.GetNearSlot(Slot.Direction.Up_Right);
        }
    }

    public void MoveToEmptySlot(Block block)
    {
        if (movingBlocks.ContainsKey(block)) return;
        movingBlocks.Add(block, null);

        StartCoroutine(BlockMovingAnim(block));
    }

    public void MoveBlockToEmptySlot(Slot fromSlot)
    {
        Block block = fromSlot.ReleaseBlock();
        if (block == null) return;

        if (movingBlocks.ContainsKey(block)) return;
        movingBlocks.Add(block, fromSlot);

        StartCoroutine(BlockMovingAnim(block, fromSlot));
    }

    IEnumerator BlockMovingAnim(Block block, Slot startSlot = null)
    {
        Slot nextSlot = null;
        Slot curSlot = startSlot;

        Transform blockTransform = block.transform;

        while (true)
        {
            if (nextSlot == null || blockTransform.position == nextSlot.transform.position)
            {
                if(nextSlot != null) curSlot = nextSlot;

                do
                {
                    nextSlot = GetNextMovingSlot(curSlot);
                    yield return null;
                } while (nextSlot != null && !nextSlot.TryReservedForBlock());

                if (nextSlot == null)
                {
                    curSlot.SetBlock(block);
                    curSlot.ReleaseReserved();
                    break;
                }
                else
                {
                    nextSlot.TryReservedForBlock();
                    if (curSlot != null) curSlot.ReleaseReserved();
                }
            }

            blockTransform.position = Vector3.MoveTowards(blockTransform.position, nextSlot.transform.position, 10 * Time.deltaTime);
            yield return null;
        }

        movingBlocks.Remove(block);
    }

    private Slot GetNextMovingSlot(Slot slot)
    {
        if (slot == null) return PuzzleSearch.Instance.RootSlot;

        Slot downSlot = slot.GetNearSlot(Slot.Direction.Down);
        if(downSlot != null)
        {
            if (downSlot.haveBlock == null) return downSlot;
        }

        Slot leftSlot = slot.GetNearSlot(Slot.Direction.Down_Left);
        Slot rightSlot = slot.GetNearSlot(Slot.Direction.Down_Right);
        if ((leftSlot == null || leftSlot.haveBlock != null) && (rightSlot == null || rightSlot.haveBlock != null)) return null;
        else if (leftSlot == null || leftSlot.haveBlock != null) return rightSlot;
        else if (rightSlot == null || rightSlot.haveBlock != null) return leftSlot;

        return Random.Range(0, 2) == 0 ? leftSlot : rightSlot;
    }
}
