using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleBreaker : MonoBehaviour
{
    private static PuzzleBreaker instance;
    // 파괴 대기 중인 큐
    private Queue<Slot> readyToBreakBlocks;
    private List<Slot> rebuildSlotLines;

    private List<Slot> alreadyCheck;
    private Queue<Slot> waitingCheck;
    private bool isBreaking;

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
        rebuildSlotLines = new List<Slot>();
        alreadyCheck = new List<Slot>();
        waitingCheck = new Queue<Slot>();
    }

    public int TryBreakBlock(Slot slot1, Slot slot2)
    {
        if (slot1 == null && slot2 == null) return 0;

        readyToBreakBlocks.Clear();

        if(slot1 != null) InsertExistBreakBlocks(slot1);
        if (slot2 != null) InsertExistBreakBlocks(slot2);

        int breakCount = BreakBlocks();
        if(breakCount > 0) StartCoroutine(AfterBreak());
        return breakCount;
    }

    private void InsertExistBreakBlocks(Slot startSlot)
    {
        if (startSlot.haveBlock is Obstacle) return;

        alreadyCheck.Clear();
        waitingCheck.Clear();
        waitingCheck.Enqueue(startSlot);

        while(waitingCheck.Count != 0)
        {
            Slot baseSlot = waitingCheck.Dequeue();

            if (alreadyCheck.Contains(baseSlot)) continue;
            alreadyCheck.Add(baseSlot);

            List<Slot> breakableSlots = PuzzleSearch.Instance.GetBreakableSlots(baseSlot);
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

    private int BreakBlocks()
    {
        if (readyToBreakBlocks.Count == 0) return 0;

        rebuildSlotLines.Clear();
        List<Slot> replaceLineForFirstSlots = new List<Slot>();
        List<Slot> obstacles = new List<Slot>();

        int breakCount = 0;
        while (readyToBreakBlocks.Count != 0)
        {
            Slot slot_Cluster = readyToBreakBlocks.Dequeue();
            if (slot_Cluster != null)
            {
                breakCount++;
                slot_Cluster.BreakBlock();


                Slot downSlot = null;
                slot_Cluster.ForeachNearSlot((nearSlot, dir) =>
                {
                    if (dir == Slot.Direction.Down) downSlot = nearSlot;

                    Obstacle obstacle = null;
                    if(nearSlot != null && !nearSlot.IsReadyBreak() && nearSlot.haveBlock != null && nearSlot.haveBlock is Obstacle)
                    {
                        obstacles.Add(nearSlot);
                        nearSlot.ReadyBreakBlock();
                    }
                });
                if (downSlot == null || downSlot.haveBlock != null) replaceLineForFirstSlots.Add(slot_Cluster);
            }
        }

        obstacles.ForEach((slot) => {
            if (slot.IsReadyBreak())
            {
                slot.BreakBlock();
                if (slot.haveBlock == null)
                {
                    BlockAnimation.Instance.MoveToEmptySlotForUpLine(slot);
                    breakCount++;
                }
            }
        });
        replaceLineForFirstSlots.Sort((slot1, slot2) => slot1.transform.localPosition.y.CompareTo(slot2.transform.localPosition.y));
        replaceLineForFirstSlots.ForEach((slot) => BlockAnimation.Instance.MoveToEmptySlotForUpLine(slot));

        return breakCount;
    }

    IEnumerator AfterBreak()
    {
        if (isBreaking) yield break;
        isBreaking = true;

        while (true)
        {
            yield return new WaitWhile(() => BlockAnimation.Instance.IsPlayAnim);

            int breakCount = 0;
            PuzzleSearch.Instance.CheckAllSlots((slot) =>
            {
                breakCount += TryBreakBlock(slot, null);
            });

            yield return null;
            if (breakCount == 0) break;

            PuzzleCreator.Instance.CreateBlocks(breakCount);

            yield return new WaitWhile(() => PuzzleCreator.Instance.IsCreating);
        }

        isBreaking = false;
    }
}
