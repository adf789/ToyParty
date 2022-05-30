using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class PuzzleBreaker : MonoBehaviour
{
    private static PuzzleBreaker instance;
    // 파괴 대기 중인 큐
    private Queue<Slot> readyToBreakBlocks;
    private Dictionary<int, Slot> rebuildLineLowestSlots;

    private List<Slot> alreadyCheck;
    private Queue<Slot> waitingCheck;
    private bool isBreaking;
    public bool IsBreaking { get => isBreaking; }

    public static PuzzleBreaker Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        readyToBreakBlocks = new Queue<Slot>();
        rebuildLineLowestSlots = new Dictionary<int, Slot>();
        alreadyCheck = new List<Slot>();
        waitingCheck = new Queue<Slot>();
    }

    public void AddBreakBlock(Slot slot)
    {
        if (slot == null) return;

        InsertExistBreakBlocks(slot);
        List<Slot> lineSlots = PuzzleSearch.Instance.GetBreakableSlots_Line(slot);
        lineSlots.ForEach((tempSlot) => { readyToBreakBlocks.Enqueue(tempSlot); });
    }

    public int StartBreakBlocks()
    {
        if (readyToBreakBlocks.Count == 0) return 0;

        rebuildLineLowestSlots.Clear();
        Queue<Slot> obstacles = new Queue<Slot>();

        int breakCount = 0;
        while (readyToBreakBlocks.Count != 0)
        {
            Slot slot_Cluster = readyToBreakBlocks.Dequeue();

            breakCount++;
            slot_Cluster.BreakBlock();
            InsertSlotToRebuildLine(slot_Cluster);

            slot_Cluster.ForeachNearSlot((nearSlot, dir) =>
            {
                if (nearSlot != null && !nearSlot.IsReadyBreak() && nearSlot.haveBlock != null && nearSlot.haveBlock is Obstacle)
                {
                    obstacles.Enqueue(nearSlot);
                    nearSlot.ReadyBreakBlock();
                }
            });
        }

        while(obstacles.Count != 0)
        {
            Slot slot = obstacles.Dequeue();
            slot.BreakBlock();
            if (slot.haveBlock == null)
            {
                InsertSlotToRebuildLine(slot);
                breakCount++;
            }
        }

        if(!isBreaking) StartCoroutine(AfterBreak(breakCount));

        return breakCount;
    }

    private void InsertExistBreakBlocks(Slot startSlot)
    {
        if (startSlot.haveBlock is Obstacle) return;

        alreadyCheck.Clear();
        waitingCheck.Clear();
        waitingCheck.Enqueue(startSlot);

        while (waitingCheck.Count != 0)
        {
            Slot baseSlot = waitingCheck.Dequeue();

            if (alreadyCheck.Contains(baseSlot)) continue;
            alreadyCheck.Add(baseSlot);

            List<Slot> breakableSlots = PuzzleSearch.Instance.GetBreakableSlots_Cluster(baseSlot);
            List<Slot> checkableSlots = PuzzleSearch.Instance.GetMaybeClusterSlots();
            breakableSlots.ForEach((slot) =>
            {
                readyToBreakBlocks.Enqueue(slot);
            });

            checkableSlots.ForEach((slot) =>
            {
                waitingCheck.Enqueue(slot);
            });
        }
    }

    private void InsertSlotToRebuildLine(Slot slot)
    {
        if (slot == null) return;

        int lineIndex = slot.lineIndex;
        if (!rebuildLineLowestSlots.ContainsKey(slot.lineIndex)) rebuildLineLowestSlots.Add(lineIndex, slot);
        else if (rebuildLineLowestSlots[lineIndex].index > slot.index) rebuildLineLowestSlots[lineIndex] = slot;
    }

    IEnumerator AfterBreak(int breakCount)
    {
        if (isBreaking) yield break;
        isBreaking = true;

        while (breakCount > 0)
        {
            foreach (KeyValuePair<int, Slot> pair in rebuildLineLowestSlots)
            {
                BlockMover.Instance.AddMoveBlockForUpLine(pair.Value);
            }
            BlockMover.Instance.StartMoveBlocks();
            rebuildLineLowestSlots.Clear();
            PuzzleCreator.Instance.CreateBlocks(breakCount);

            yield return new WaitWhile(() => BlockMover.Instance.IsMoving);
            yield return new WaitWhile(() => PuzzleCreator.Instance.IsCreating);

            PuzzleSearch.Instance.CheckAllSlots((slot) =>
            {
                AddBreakBlock(slot);
            });
            breakCount = StartBreakBlocks();

            yield return null;
        }

        isBreaking = false;
    }
}
