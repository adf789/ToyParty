using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Slot : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Up_Right,
        Down_Right,
        Down,
        Down_Left,
        Up_Left,
        None
    }
    public static Slot pickedSlot;
    public Gem haveGem;
    public bool reservedForGem;
    public int index;

    [SerializeField] private Slot[] nearSlots;
    private bool readyBreak;

    /// <summary>
    /// 시계 방향으로 칸 수 만큼 회전
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="num">칸 수</param>
    /// <returns></returns>
    public static Direction RotateClockWise(Direction baseDirection, int num)
    {
        int tempNum = (int)baseDirection + num % 6;
        if (tempNum > (int)Direction.Up_Left) return tempNum - (Direction.Up_Left + 1);

        return (Direction)tempNum;
    }

    /// <summary>
    /// 반시계 방향으로 칸 수 만큼 회전
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="num">칸 수</param>
    /// <returns></returns>
    public static Direction RotateCounterClockWise(Direction baseDirection, int num)
    {
        int tempNum = (int)baseDirection - num % 6;
        if (tempNum < 0) return Direction.Up_Left + (tempNum + 1);

        return (Direction)tempNum;
    }

    public void SetGem(Gem gem)
    {
        if (haveGem != null) return;

        haveGem = gem;
        haveGem.transform.SetParent(transform);
        haveGem.transform.localPosition = Vector3.zero;
        readyBreak = false;
    }

    public bool TryReservedForGem()
    {
        if (haveGem != null || reservedForGem) return false;
        reservedForGem = true;
        return true;
    }

    public void ReleaseReserved()
    {
        reservedForGem = false;
    }

    public Gem ReleaseGem()
    {
        if (haveGem == null) return null;

        Gem gem = haveGem;
        gem.transform.SetParent(null);
        haveGem = null;

        return gem;
    }

    public void ClearNearSlot()
    {
        for(int dir = 0; dir < 6; dir++)
        {
            SetNearSlot((Direction)dir, null);
        }
    }

    public Slot GetNearSlot(Direction direction)
    {
        return nearSlots[(int)direction];
    }

    public void SetNearSlot(Direction direction, Slot slot)
    {
        nearSlots[(int)direction] = slot;
    }

    public void ForeachNearSlot(UnityAction<Slot, Direction> action)
    {
        ForeachNearSlot(Direction.Up, Direction.Up_Left, action);
    }

    public void ForeachNearSlot(Direction startDir, Direction endDir, UnityAction<Slot, Direction> action)
    {
        bool endReady = false;
        while (true)
        {
            if (startDir == endDir) endReady = true;

            action(GetNearSlot(startDir), startDir);

            if (endReady) return;

            startDir = RotateClockWise(startDir, 1);
        }
    }

    public void ForeachCheckCluster(UnityAction<Slot, int> action)
    {
        Direction endDir = Direction.None;
        Direction startDir = Direction.Up;

        // Cluster 체크하기위한 시작위치 탐색
        do
        {
            if (!IsSameGemWithNearSlot(startDir))
            {
                endDir = startDir;
                break;
            }
            startDir = RotateCounterClockWise(startDir, 1);
        } while (startDir != Direction.Up);

        // 군집된 개수만큼 
        if(endDir == Direction.None)
        {
            GatherCluster(Direction.Up, 6, action);
            return;
        }

        int clusterSize = 0;
        startDir = RotateClockWise(endDir, 1);
        ForeachNearSlot(startDir, endDir, (slot, dir) =>
        {
            if (this.IsSameGem(slot))
            {
                clusterSize++;
                return;
            }

            if (clusterSize > 1)
            {
                GatherCluster(RotateCounterClockWise(dir, clusterSize), clusterSize, action);
            }

            if(clusterSize > 0) clusterSize = 0;
        });

        if(clusterSize > 1) GatherCluster(RotateCounterClockWise(startDir, clusterSize), clusterSize, action);
    }

    public bool IsSameGemWithNearSlot(Direction direction)
    {
        return IsSameGem(nearSlots[(int)direction]);
    }

    public bool IsSameGem(Slot slot)
    {
        if (slot == null) return false;
        else if (slot.haveGem == null || haveGem == null) return false;

        bool isSame = false;

        try
        {
            isSame = slot.haveGem.Color == haveGem.Color;
        }
        catch (Exception)
        {
            Debug.Log(StackTraceUtility.ExtractStackTrace());
        }

        return isSame;
    }

    public void ExchangeGem(Slot slot, bool doingBreak = true)
    {
        if (IsSameGem(slot)) return;
        else if (haveGem == null || slot.haveGem == null) return;

        bool foundNearSlot = false;
        ForeachNearSlot((nearSlot, direction) =>
        {
            if (nearSlot == null) return;

            if (nearSlot.Equals(slot))
            {
                foundNearSlot = true;
            }
        });

        if (!foundNearSlot) return;

        ExchangeGemWithSlot(slot);

        GemAnimation.Instance.PlayExchangeGem(haveGem, slot.haveGem, (doingBreak ? Break : null));

        void Break()
        {
            int breakCount1 = PuzzleBreaker.Instance.TryBreakGem(slot);
            int breakCount2 = PuzzleBreaker.Instance.TryBreakGem(this);
            if (breakCount1 == 0 && breakCount2 == 0) ExchangeGem(slot, false);
            else PuzzleCreator.Instance.CreateGems(breakCount1 + breakCount2);
        }
    }

    public void ReadyBreakGem()
    {
        readyBreak = true;
    }

    public bool IsReadyBreak()
    {
        return readyBreak;
    }

    public void BreakGem()
    {
        Gem gem = ReleaseGem();
        if(gem != null) gem.Break();
    }

    public void TryGetSlotLine(Direction dir, ref List<Slot> slotsInLine)
    {
        int lineCount = 1;
        Direction crossDir = RotateClockWise(dir, 3);
        Slot nearSlot_Dir = GetNearSlot(dir);
        Slot nearSlot_CrossDir = GetNearSlot(crossDir);
        Slot calculateSlot = this;

        while (IsSameGem(nearSlot_Dir) || IsSameGem(nearSlot_CrossDir))
        {
            if (IsSameGem(nearSlot_Dir))
            {
                lineCount++;
                calculateSlot = nearSlot_Dir;
                nearSlot_Dir = nearSlot_Dir.GetNearSlot(dir);
            }

            if (IsSameGem(nearSlot_CrossDir))
            {
                lineCount++;
                nearSlot_CrossDir = nearSlot_CrossDir.GetNearSlot(crossDir);
            }
        }

        if(lineCount >= 3)
        {
            if (slotsInLine == null) slotsInLine = new List<Slot>();

            for(int i = 0; i < lineCount; i++)
            {
                slotsInLine.Add(calculateSlot);
                calculateSlot = calculateSlot.GetNearSlot(crossDir);
            }
        }
    }

    private void GatherCluster(Direction startDir, int size, UnityAction<Slot, int> action)
    {
        if (size < 2) return;

        Direction endDir = RotateClockWise(startDir, size - 1);
        ForeachNearSlot(startDir, endDir, (slot, dir) =>
        {
            action?.Invoke(GetNearSlot(dir), size);
        });
    }

    private void ExchangeGemWithSlot(Slot targetSlot)
    {
        Gem tempGem = haveGem;
        haveGem = targetSlot.haveGem;
        targetSlot.haveGem = tempGem;

        haveGem.transform.SetParent(transform);
        targetSlot.haveGem.transform.SetParent(targetSlot.transform);
    }
}
