using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleSearch : MonoBehaviour
{
    private static PuzzleSearch instance;
    [SerializeField] private Slot rootSlot;
    public Slot RootSlot { get => rootSlot; }
    private List<Slot> maybeClusterSlots;

    public static PuzzleSearch Instance
    {
        get
        {
            return instance;
        }
    }


    private void Awake()
    {
        instance = this;
        maybeClusterSlots = new List<Slot>();
    }

    public void CheckAllSlots(UnityAction<Slot> foreachAction)
    {
        CheckSlotRecursive(Slot.Direction.None, rootSlot, foreachAction);
    }

    /// <summary>
    /// 주위 파괴 가능한 슬롯을 찾아 리스트를 만듦
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public List<Slot> GetBreakableSlots(Slot slot)
    {
        List<Slot> breakableSlots = null;
        maybeClusterSlots.Clear();

        Slot.Direction dir = FindNotSameBlockDirection(slot);

        // 주위 모든 슬롯의 젬이 같은 색일 경우
        if(dir == Slot.Direction.None)
        {
            breakableSlots = new List<Slot>();
            slot.ForeachNearSlot((nearSlot, dir) =>
            {
                if(dir == Slot.Direction.Up || dir == Slot.Direction.Up_Right || dir == Slot.Direction.Down_Right)
                {
                    TryGetSlotLine(slot, dir, ref breakableSlots);
                }
                if (!nearSlot.IsReadyBreak())
                {
                    breakableSlots.Add(nearSlot);
                    nearSlot.ReadyBreakBlock();
                }
            });

            if (!slot.IsReadyBreak())
            {
                breakableSlots.Add(slot);
            }
        }
        else
        {
            breakableSlots = FindClusterAndLine(slot, dir);
        }

        return breakableSlots;
    }

    public List<Slot> GetMaybeClusterSlots()
    {
        return maybeClusterSlots;
    }

    private Slot.Direction FindNotSameBlockDirection(Slot slot)
    {
        Slot.Direction endDir = Slot.Direction.None;
        Slot.Direction startDir = Slot.Direction.Up;

        // Cluster 체크하기위한 시작위치 탐색
        do
        {
            if (!slot.IsSameBlockWithNearSlot(startDir))
            {
                endDir = startDir;
                break;
            }
            startDir = Slot.RotateCounterClockWise(startDir, 1);
        } while (startDir != Slot.Direction.Up);

        return endDir;
    }

    /// <summary>
    /// 주위 젬이 군집을 이루는지, 라인을 이루는지 탐색
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="startDir"></param>
    /// <returns></returns>
    private List<Slot> FindClusterAndLine(Slot slot, Slot.Direction startDir)
    {
        List<Slot> slots = new List<Slot>();
        Queue<Slot> clusterSlots = new Queue<Slot>();

        Slot.Direction endDir = Slot.RotateCounterClockWise(startDir, 1);
        slot.ForeachNearSlot(startDir, endDir, (nearSlot, dir) =>
        {
            if (dir == Slot.Direction.Up || dir == Slot.Direction.Up_Right || dir == Slot.Direction.Down_Right)
            {
                TryGetSlotLine(slot, dir, ref slots);
            }

            if (slot.IsSameBlock(nearSlot))
            {
                clusterSlots.Enqueue(nearSlot);
                return;
            }

            if (clusterSlots.Count > 2 && !slot.IsReadyBreak())
            {
                slots.Add(slot);
                slot.ReadyBreakBlock();
            }
            AddClusterSlots(clusterSlots, ref slots);

            clusterSlots.Clear();
        });

        if (clusterSlots.Count > 2 && !slot.IsReadyBreak())
        {
            slots.Add(slot);
            slot.ReadyBreakBlock();
        }
        AddClusterSlots(clusterSlots, ref slots);

        return slots;
    }

    private void TryGetSlotLine(Slot slot, Slot.Direction dir, ref List<Slot> slotsInLine)
    {
        if (slot == null) return;

        int lineCount = 1;
        Slot.Direction crossDir = Slot.RotateClockWise(dir, 3);
        Slot nearSlot_Dir = slot.GetNearSlot(dir);
        Slot nearSlot_CrossDir = slot.GetNearSlot(crossDir);
        Slot calculateSlot = slot;

        while (slot.IsSameBlock(nearSlot_Dir) || slot.IsSameBlock(nearSlot_CrossDir))
        {
            if (slot.IsSameBlock(nearSlot_Dir))
            {
                lineCount++;
                calculateSlot = nearSlot_Dir;
                nearSlot_Dir = nearSlot_Dir.GetNearSlot(dir);
            }

            if (slot.IsSameBlock(nearSlot_CrossDir))
            {
                lineCount++;
                nearSlot_CrossDir = nearSlot_CrossDir.GetNearSlot(crossDir);
            }
        }

        if (lineCount >= 3)
        {
            if (slotsInLine == null) slotsInLine = new List<Slot>();

            for (int i = 0; i < lineCount; i++)
            {
                if (!calculateSlot.IsReadyBreak())
                {
                    slotsInLine.Add(calculateSlot);
                    calculateSlot.ReadyBreakBlock();
                }
                calculateSlot = calculateSlot.GetNearSlot(crossDir);
            }
        }
    }

    private void AddClusterSlots(Queue<Slot> addableSlots, ref List<Slot> addedSlots)
    {
        if (addableSlots.Count > 1 || true)
        {
            int clusterSize = addableSlots.Count;
            foreach (Slot clusterSlot in addableSlots)
            {
                if (!maybeClusterSlots.Contains(clusterSlot)) maybeClusterSlots.Add(clusterSlot);
                
                if (clusterSize > 2 && !clusterSlot.IsReadyBreak())
                {
                    addedSlots.Add(clusterSlot);
                    clusterSlot.ReadyBreakBlock();
                }
            }
        }
    }

    private void CheckSlotRecursive(Slot.Direction dir, Slot curSlot, UnityAction<Slot> foreachAction)
    {
        if (curSlot == null) return;

        if (dir == Slot.Direction.Down_Left) CheckSlotRecursive(Slot.Direction.Down_Left, curSlot.GetNearSlot(Slot.Direction.Down_Left), foreachAction);
        else if (dir == Slot.Direction.Down_Right) CheckSlotRecursive(Slot.Direction.Down_Right, curSlot.GetNearSlot(Slot.Direction.Down_Right), foreachAction);
        else
        {
            CheckSlotRecursive(Slot.Direction.Down_Left, curSlot.GetNearSlot(Slot.Direction.Down_Left), foreachAction);
            CheckSlotRecursive(Slot.Direction.Down, curSlot.GetNearSlot(Slot.Direction.Down), foreachAction);
            CheckSlotRecursive(Slot.Direction.Down_Right, curSlot.GetNearSlot(Slot.Direction.Down_Right), foreachAction);
        }

        foreachAction?.Invoke(curSlot);
    }
}
