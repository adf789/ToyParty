using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

public class PuzzleBreaker : Singleton<PuzzleBreaker>
{
    private Queue<Slot> readyToBreakBlocks; // 파괴 대기 중인 큐
    private Dictionary<int, Slot> rebuildLineLowestSlots;
    private Queue<(Block, int)> scoreSpawns; 

    private List<Slot> alreadyCheck;
    private Queue<Slot> waitingCheck;
    private bool isBreaking;
    public bool IsBreaking { get => isBreaking; }

    private void Awake()
    {
        readyToBreakBlocks = new Queue<Slot>();
        rebuildLineLowestSlots = new Dictionary<int, Slot>();
        scoreSpawns = new Queue<(Block, int)>();
        alreadyCheck = new List<Slot>();
        waitingCheck = new Queue<Slot>();
    }

    public void AddBreakBlock(Slot slot)
    {
        if (slot == null) return;

        InsertExistBreakBlocks(slot);
    }

    public int StartBreakBlocks()
    {
        if (readyToBreakBlocks.Count == 0) return 0;

        rebuildLineLowestSlots.Clear();
        Queue<Slot> obstacles = new Queue<Slot>();

        int breakCount = 0;
        while (readyToBreakBlocks.Count != 0)
        {
            Slot slot = readyToBreakBlocks.Dequeue();

            breakCount++;
            slot.BreakBlock();
            InsertSlotToRebuildLine(slot);

            slot.ForeachNearSlot((nearSlot, dir) =>
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

        InstantiateScoreText();

        return breakCount;
    }

    private void InsertExistBreakBlocks(Slot startSlot)
    {
        if (startSlot.haveBlock is Obstacle) return;

        alreadyCheck.Clear();
        waitingCheck.Clear();
        waitingCheck.Enqueue(startSlot);

        int originCount = readyToBreakBlocks.Count;

        while (waitingCheck.Count != 0)
        {
            Slot baseSlot = waitingCheck.Dequeue();

            if (alreadyCheck.Contains(baseSlot)) continue;
            alreadyCheck.Add(baseSlot);

            InsertBreakableSlots_Cluster(baseSlot);
        }

        InsertBreakableSlots_Line(startSlot);
        AddScoreInQueue(startSlot.haveBlock, readyToBreakBlocks.Count - originCount);
    }

    private void AddScoreInQueue(Block block, int blockCount)
    {
        if (blockCount < 1) return;
        else if (block == null) return;

        int score = blockCount * 30;

        (Block, int) tuple = (block, score);
        scoreSpawns.Enqueue(tuple);
    }

    private void InsertSlotToRebuildLine(Slot slot)
    {
        if (slot == null) return;

        int lineIndex = slot.lineIndex;
        if (!rebuildLineLowestSlots.ContainsKey(slot.lineIndex)) rebuildLineLowestSlots.Add(lineIndex, slot);
        else if (rebuildLineLowestSlots[lineIndex].index > slot.index) rebuildLineLowestSlots[lineIndex] = slot;
    }

    /// <summary>
    /// 주위 파괴 가능한 슬롯을 찾아 리스트를 만듦
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private void InsertBreakableSlots_Cluster(Slot slot)
    {
        Slot.Direction dir = FindNotSameBlockDirection(slot);

        // 주위 모든 슬롯의 젬이 같은 색일 경우
        if (dir == Slot.Direction.None)
        {
            slot.ForeachNearSlot((nearSlot, dir) =>
            {
                waitingCheck.Enqueue(nearSlot);

                if (!nearSlot.IsReadyBreak())
                {
                    readyToBreakBlocks.Enqueue(nearSlot);
                    nearSlot.ReadyBreakBlock();
                }
            });

            if (!slot.IsReadyBreak())
            {
                readyToBreakBlocks.Enqueue(slot);
            }
        }
        else
        {
            FindCluster(slot, dir);
        }
    }

    /// <summary>
    /// 주위 파괴 가능한 슬롯을 찾아 리스트를 만듦
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private void InsertBreakableSlots_Line(Slot slot)
    {
        slot.ForeachNearSlot(Slot.Direction.Up, Slot.Direction.Down_Right, (nearSlot, dir) =>
        {
            TryGetSlotLine(slot, dir);
        });
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
    private void FindCluster(Slot slot, Slot.Direction startDir)
    {
        Queue<Slot> clusterSlots = new Queue<Slot>();

        Slot.Direction endDir = Slot.RotateCounterClockWise(startDir, 1);
        slot.ForeachNearSlot(startDir, endDir, (nearSlot, dir) =>
        {
            if (slot.IsSameBlock(nearSlot))
            {
                clusterSlots.Enqueue(nearSlot);
                return;
            }

            if (clusterSlots.Count > 2 && !slot.IsReadyBreak())
            {
                readyToBreakBlocks.Enqueue(slot);
                slot.ReadyBreakBlock();
            }
            AddClusterSlots(clusterSlots);

            clusterSlots.Clear();
        });

        if (clusterSlots.Count > 2 && !slot.IsReadyBreak())
        {
            readyToBreakBlocks.Enqueue(slot);
            slot.ReadyBreakBlock();
        }
        AddClusterSlots(clusterSlots);
    }

    private void TryGetSlotLine(Slot slot, Slot.Direction dir)
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
            for (int i = 0; i < lineCount; i++)
            {
                if (!calculateSlot.IsReadyBreak())
                {
                    readyToBreakBlocks.Enqueue(calculateSlot);
                    calculateSlot.ReadyBreakBlock();
                }
                calculateSlot = calculateSlot.GetNearSlot(crossDir);
            }
        }
    }

    private void AddClusterSlots(Queue<Slot> addableSlots)
    {
        if (addableSlots.Count > 1)
        {
            int clusterSize = addableSlots.Count;
            foreach (Slot clusterSlot in addableSlots)
            {
                waitingCheck.Enqueue(clusterSlot);

                if (clusterSize > 2 && !clusterSlot.IsReadyBreak())
                {
                    readyToBreakBlocks.Enqueue(clusterSlot);
                    clusterSlot.ReadyBreakBlock();
                }
            }
        }
    }

    private void InstantiateScoreText()
    {
        while(scoreSpawns.Count != 0)
        {
            (Block, int) scoreTuple = scoreSpawns.Dequeue();
            ScoreText scoreText = ExtraPooling.Instance.GetUnUseScoreText();
            scoreText.transform.position = scoreTuple.Item1.transform.position;
            scoreText.Show(scoreTuple.Item2, scoreTuple.Item1.Block_Color);
            ScreenUI.Instance.AddScore(scoreTuple.Item2);
        }
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

        ScreenUI.Instance.CheckEndForStage();
        isBreaking = false;
    }
}
