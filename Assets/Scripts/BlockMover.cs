using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class BlockMover : MonoBehaviour
{
    private static BlockMover instance;
    [SerializeField] private bool isMoving;
    private Dictionary<Block, Slot> movingBlocks;
    private int movingCount;
    public int MovingCount { get => movingCount; }

    public static BlockMover Instance
    {
        get
        {
            return instance;
        }
    }
    public bool IsMoving { get => isMoving || movingCount > 0; }

    private void Awake()
    {
        instance = this;
        movingBlocks = new Dictionary<Block, Slot>();
    }

    public void PlayExchangeBlock(Block fromBlock, Block toBlock, UnityAction resultAction = null)
    {
        StartCoroutine(BlockExchangingAnim(fromBlock, toBlock, resultAction));
    }

    IEnumerator BlockExchangingAnim(Block fromBlock, Block toBlock, UnityAction resultAction = null)
    {
        isMoving = true;
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
        isMoving = false;
    }

    public void AddMoveBlockForUpLine(Slot slot)
    {
        Slot upSlot = slot;

        while (upSlot != null)
        {
            if(upSlot.haveBlock != null && !upSlot.haveBlock.isMoving)
            {
                Debug.Log(upSlot + "½ÅÈ£ ¹ÞÀ½");
                AddMoveBlock(upSlot);
            }

            Slot prevSlot = upSlot;
            upSlot = upSlot.GetNearSlot(Slot.Direction.Up);
            upSlot = upSlot ?? prevSlot.GetNearSlot(Slot.Direction.Up_Left);
            upSlot = upSlot ?? prevSlot.GetNearSlot(Slot.Direction.Up_Right);
        }
    }

    public void AddMoveBlock(Block block)
    {
        if (block.isMoving) return;
        block.isMoving = true;
        block.direction = Random.Range(0, 2) == 0 ? Block.MoveDirection.Left : Block.MoveDirection.Right;
        movingBlocks.Add(block, null);
    }

    public void AddMoveBlock(Slot fromSlot)
    {
        if (fromSlot.haveBlock == null) return;
        Block block = fromSlot.ReleaseBlock();
        if (block.isMoving) return;
        block.isMoving = true;
        block.direction = Random.Range(0, 2) == 0 ? Block.MoveDirection.Left : Block.MoveDirection.Right;
        movingBlocks.Add(block, fromSlot);
    }

    public void StartMoveBlocks()
    {
        Dictionary<Block, Slot> testDic = movingBlocks.OrderBy(pair => pair.Key.transform.position.y).ToDictionary(x => x.Key, x => x.Value);
        foreach (KeyValuePair<Block, Slot> pair in movingBlocks.OrderBy(pair => pair.Key.transform.position.y).ToDictionary(x => x.Key, x => x.Value))
        {
            //Slot destinationSlot = FindDestinationSlot(pair.Key, pair.Value);
            Queue<Slot> destinationPath = FindDestinationSlotQ(pair.Key, pair.Value);
            Slot destinationSlot = destinationPath.Last<Slot>();
            destinationSlot.reservedForBlock = pair.Key;
            Debug.Log(pair.Key + " reserved: " + destinationSlot);
            
            StartCoroutine(BlockMoving(pair.Key, destinationPath));
        }

        movingBlocks.Clear();
    }

    IEnumerator BlockMoving(Block block, Queue<Slot> path)
    {
        movingCount++;


        Slot nextSlot = path.Dequeue();
        Transform blockTransform = block.transform;
        float speed = 10f;

        while (true)
        {
            blockTransform.position = Vector3.MoveTowards(blockTransform.position, nextSlot.transform.position, speed * Time.deltaTime);
            speed += 0.02f;
            if (speed > 12f) speed = 12f;

            if (blockTransform.position == nextSlot.transform.position)
            {
                if(path.Count == 0)
                {
                    Debug.Log(block + " µµÂø");
                    nextSlot.SetBlock(block);
                    nextSlot.reservedForBlock = null;
                    break;
                }
                nextSlot = path.Dequeue();
                Debug.Log(block + " -> " + nextSlot);
            }

            yield return null;
        }

        movingCount--;
    }

    IEnumerator BlockMoving(Block block, Slot startSlot = null)
    {
        movingCount++;
        Slot curSlot = startSlot;
        Slot nextSlot = startSlot == null ? PuzzleSearch.Instance.RootSlot : GetNextDestination(block, curSlot);
        Debug.Log(block + " -> " + nextSlot);

        Transform blockTransform = block.transform;
        float speed = 10f;

        while (nextSlot != null)
        {
            blockTransform.position = Vector3.MoveTowards(blockTransform.position, nextSlot.transform.position, speed * Time.deltaTime);
            speed += 0.02f;
            if (speed > 1.2f) speed = 1.2f;

            if (blockTransform.position == nextSlot.transform.position)
            {
                curSlot = nextSlot;
                nextSlot = GetNextDestination(block, curSlot);
                Debug.Log(block + " -> " + nextSlot);
                if (curSlot.reservedForBlock != null && curSlot.reservedForBlock.Equals(block))
                {
                    Debug.Log(block + " µµÂø");
                    curSlot.SetBlock(block);
                    curSlot.reservedForBlock = null;
                    break;
                }
            }

            yield return null;
        }

        movingCount--;
    }

    private Queue<Slot> FindDestinationSlotQ(Block block, Slot startSlot = null)
    {
        Queue<Slot> pathQueue = new Queue<Slot>();
        if (startSlot == null) pathQueue.Enqueue(PuzzleSearch.Instance.RootSlot);
        Slot destinationSlot = startSlot ?? PuzzleSearch.Instance.RootSlot;
        while (true)
        {
            Slot tempSlot = GetNextDestination(block, destinationSlot);
            if (tempSlot == null) break;

            destinationSlot = tempSlot;
            pathQueue.Enqueue(destinationSlot);
        }

        return pathQueue;
    }

    private Slot FindDestinationSlot(Block block, Slot startSlot = null)
    {
        Slot destinationSlot = startSlot ?? PuzzleSearch.Instance.RootSlot;
        while (true)
        {
            Slot tempSlot = GetNextDestination(block, destinationSlot);
            if (tempSlot == null) break;

            destinationSlot = tempSlot;
        }

        return destinationSlot;
    }

    private Slot GetNextDestination(Block block, Slot slot)
    {
        if (slot == null) return PuzzleSearch.Instance.RootSlot;

        Slot downSlot = slot.GetNearSlot(Slot.Direction.Down);
        if (PossibleDestination(block, downSlot)) return downSlot;

        Slot leftSlot = slot.GetNearSlot(Slot.Direction.Down_Left);
        Slot rightSlot = slot.GetNearSlot(Slot.Direction.Down_Right);

        if (PossibleDestination(block, leftSlot) && PossibleDestination(block, rightSlot))
        {
            if (leftSlot.reservedForBlock != null && leftSlot.reservedForBlock.Equals(block)) return leftSlot;
            else if (rightSlot.reservedForBlock != null && rightSlot.reservedForBlock.Equals(block)) return rightSlot;
            else return block.direction == Block.MoveDirection.Left ? leftSlot : rightSlot;
        }
        else if (PossibleDestination(block, leftSlot)) return leftSlot;
        else if (PossibleDestination(block, rightSlot)) return rightSlot;
        else return null;
    }

    private bool PossibleDestination(Block block, Slot slot)
    {
        return slot != null && slot.haveBlock == null && (slot.reservedForBlock == null || slot.reservedForBlock.Equals(block));
    }
}
