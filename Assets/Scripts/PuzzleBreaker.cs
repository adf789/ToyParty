using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleBreaker : MonoBehaviour
{
    private static PuzzleBreaker instance;
    // 파괴 대기 중인 큐
    private Queue<Slot> readyToBreakGems;

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
        readyToBreakGems = new Queue<Slot>();
        alreadyCheck = new List<Slot>();
        waitingCheck = new Queue<Slot>();
    }

    public int TryBreakGem(Slot slot1)
    {
        if (slot1 == null) return 0;

        readyToBreakGems.Clear();

        InsertExistBreakGems(slot1);

        int breakCount = BreakGems();
        StartCoroutine(AfterBreak());
        return breakCount;
    }

    private void InsertExistBreakGems(Slot startSlot)
    {
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
                readyToBreakGems.Enqueue(slot);
            });

            checkableSlots.ForEach((slot) =>
            {
                waitingCheck.Enqueue(slot);
            });
        }
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

    IEnumerator AfterBreak()
    {
        if (isBreaking) yield break;
        isBreaking = true;

        while (true)
        {
            yield return new WaitWhile(() => GemAnimation.Instance.IsPlayAnim);

            int breakCount = 0;
            PuzzleSearch.Instance.CheckAllSlots((slot) =>
            {
                breakCount += TryBreakGem(slot);
            });

            yield return null;
            if (breakCount == 0) break;

            PuzzleCreator.Instance.CreateGems(breakCount);

            yield return new WaitWhile(() => PuzzleCreator.Instance.IsCreating);
        }

        isBreaking = false;
    }
}
