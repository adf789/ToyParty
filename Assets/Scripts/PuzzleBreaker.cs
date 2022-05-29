using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleBreaker : MonoBehaviour
{
    private static PuzzleBreaker instance;
    // 파괴 대기 중인 큐
    private Queue<Slot> readyToBreakGems;

    private List<Slot> alreadyCheck_Cluster;
    private Queue<Slot> waitingCheck_Cluster;

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
        readyToBreakGems = new Queue<Slot>();
        alreadyCheck_Cluster = new List<Slot>();
        waitingCheck_Cluster = new Queue<Slot>();
    }

    public bool TryBreakGem(Slot slot)
    {
        readyToBreakGems.Clear();

        InsertExistBreakGems(slot);
        //InsertExistBreakGems_Line(slot);

        int breakCount = BreakGems();
        StartCoroutine(AfterBreak(breakCount));
        return breakCount > 0;
    }

    private void InsertExistBreakGems(Slot startSlot)
    {
        alreadyCheck_Cluster.Clear();
        waitingCheck_Cluster.Clear();
        waitingCheck_Cluster.Enqueue(startSlot);

        while(waitingCheck_Cluster.Count != 0)
        {
            Slot baseSlot = waitingCheck_Cluster.Dequeue();

            if (alreadyCheck_Cluster.Contains(baseSlot)) continue;
            alreadyCheck_Cluster.Add(baseSlot);

            List<Slot> breakableSlots = PuzzleSearch.Instance.GetBreakableSlots(baseSlot);
            List<Slot> checkableSlots = PuzzleSearch.Instance.GetMaybeClusterSlots();
            breakableSlots.ForEach((slot) =>
            {
                readyToBreakGems.Enqueue(slot);
            });

            checkableSlots.ForEach((slot) =>
            {
                waitingCheck_Cluster.Enqueue(slot);
            });
            //int clusterSize = 0;
            //baseSlot.ForeachCheckCluster((slot, size) =>
            //{
            //    clusterSize = size;
            //    waitingCheck_Cluster.Enqueue(slot);
            //    if (size > 2)
            //    {
            //        readyToBreakGem(slot);
            //    }
            //});

            //if(clusterSize > 2)
            //{
            //    readyToBreakGem(baseSlot);
            //}
        }
    }

    private void InsertExistBreakGems_Line(Slot curSlot)
    {
        List<Slot> slotsInLine = new List<Slot>();
        curSlot.ForeachNearSlot(Slot.Direction.Up, Slot.Direction.Down_Right, (nearSlot, dir) =>
        {
            slotsInLine.Clear();
            curSlot.TryGetSlotLine(dir, ref slotsInLine);
            if(slotsInLine.Count > 0)
            {
                slotsInLine.ForEach((slot) => readyToBreakGem(slot));
            }
        });
    }

    private int BreakGems()
    {
        if (readyToBreakGems.Count == 0) return 0;

        List<Slot> replaceLineForFirstSlots = new List<Slot>();
        int breakCount = 0;
        while (readyToBreakGems.Count != 0)
        {
            Slot slot_Cluster = readyToBreakGems.Dequeue();
            if (slot_Cluster != null)
            {
                breakCount++;
                slot_Cluster.BreakGem();

                Slot downSlot = slot_Cluster.GetNearSlot(Slot.Direction.Down);
                if (downSlot == null || downSlot.haveGem != null) replaceLineForFirstSlots.Add(slot_Cluster);
            }
        }

        replaceLineForFirstSlots.Sort((slot1, slot2) => slot1.transform.localPosition.y.CompareTo(slot2.transform.localPosition.y));
        replaceLineForFirstSlots.ForEach((slot) => GemAnimation.Instance.MoveToEmptySlotForUpLine(slot));

        return breakCount;
    }

    private void readyToBreakGem(Slot slot)
    {
        if (!slot.IsReadyBreak()) readyToBreakGems.Enqueue(slot);
        slot.ReadyBreakGem();
    }

    IEnumerator AfterBreak(int breakCount)
    {
        if (breakCount < 1) yield break;
        yield return null;
        yield return new WaitWhile(() => GemAnimation.Instance.IsPlayAnim);

        GemAnimation.Instance.MoveGemToEmptySlot(PuzzleSearch.Instance.RootSlot);
        PuzzleCreator.Instance.CreateGems(breakCount);

        yield return null;
        yield return new WaitWhile(() => GemAnimation.Instance.IsPlayAnim);

        PuzzleSearch.Instance.CheckAllSlots((slot) =>
        {
            TryBreakGem(slot);
        });
    }
}
