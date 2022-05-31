using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCreator : Singleton<PuzzleCreator>
{
    public PuzzleSearch puzzleSearch;
    public Transform blockSpawnPoint;
    public int blockCount;
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    private bool isCreating;
    private const float obstaclePercent = 0.1f;
    [Header("랜덤 블록 생성")]
    [SerializeField] private bool startRandomBlock;

    public bool IsCreating { get => isCreating; }
    public GameObject GemPrefab { get => gemPrefab; }
    public GameObject ObstaclePrefab { get => obstaclePrefab; }

    private void Awake()
    {
        PuzzleSearch.Instance.CheckAllSlots((slot) =>
        {
            blockCount++;
            slot.transform.localScale = Vector3.zero;
        });
    }

    private void Start()
    {
        StartCoroutine(CreatePuzzle());
    }

    public void AllSlotsRandomColor()
    {
        int index = 1;
        int lineIndex = 0;
        puzzleSearch.CheckAllSlots((slot) =>
        {
            slot.index = index++;
            if (slot.GetNearSlot(Slot.Direction.Down) == null) lineIndex++;
            slot.lineIndex = lineIndex;
            slot.gameObject.name = string.Format("Slot{0}", slot.index);
            slot.transform.SetAsLastSibling();
            if (slot.haveBlock == null) slot.haveBlock = slot.GetComponentInChildren<Block>();
            if(slot.haveBlock is Gem) slot.haveBlock.SetColor((Block.BlockColor)Random.Range(0, 6));
        });
    }

    public void PlaceRandomObstacle(float obstaclePercent)
    {
        int index = 0;
        puzzleSearch.CheckAllSlots((slot) =>
        {
            float gemPercent = Random.Range(0f, 1f);
            bool isGem = gemPercent > obstaclePercent;

            if (slot.haveBlock == null) slot.haveBlock = slot.GetComponentInChildren<Block>();
            slot.index = index++;

            if (isGem)
            {
                // 젬
                if (slot.haveBlock is Obstacle)
                {
                    Block block = slot.ReleaseBlock();
                    DestroyImmediate(block.gameObject);
                    Gem createBlock = Instantiate(gemPrefab).GetComponent<Gem>();
                    createBlock.gameObject.name = string.Format("Gem{0}", slot.index + 1);
                    slot.SetBlock(createBlock);
                    createBlock.Reset();
                    slot.haveBlock.SetColor((Block.BlockColor)Random.Range(0, 6));
                }
            }
            else
            {
                // 함정
                if (slot.haveBlock is Gem)
                {
                    Block block = slot.ReleaseBlock();
                    DestroyImmediate(block.gameObject);
                    Obstacle createBlock = Instantiate(obstaclePrefab).GetComponent<Obstacle>();
                    createBlock.gameObject.name = string.Format("Obstacle{0}", slot.index + 1);
                    slot.SetBlock(createBlock);
                    createBlock.Reset();
                }
            }
        });
    }

    public void CreateBlocks(int count)
    {
        StartCoroutine(PlayCreateBlocksAnim(count));
    }

    IEnumerator CreatePuzzle()
    {
        if (startRandomBlock)
        {
            AllSlotsRandomColor();
            PlaceRandomObstacle(obstaclePercent);
        }
        BlockPooling.Instance.Init();
        yield return StartCoroutine(PuzzleStart());
    }

    IEnumerator PuzzleStart()
    {
        isCreating = true;
        bool isComplete = false;

        while (!isComplete)
        {
            PuzzleSearch.Instance.CheckAllSlots((slot) =>
            {
                slot.transform.localScale = Vector3.MoveTowards(slot.transform.localScale, Vector3.one, 2 * Time.deltaTime);
                if (slot.transform.localScale == Vector3.one)
                {
                    isComplete = true;
                }
            });
            yield return null;
        }

        isCreating = false;
    }

    IEnumerator PlayCreateBlocksAnim(int count)
    {
        Debug.Log(count + "개 생성");
        isCreating = true;
        for (int i = 0; i < count; i++)
        {
            float createdObstaclePercent = Random.Range(0f, 1f);
            Block createdBlock = createdObstaclePercent > obstaclePercent ? BlockPooling.Instance.GetUnuseGem() : BlockPooling.Instance.GetUnuseObstacle();
            createdBlock.transform.position = blockSpawnPoint.position;
            createdBlock.SetColor((Block.BlockColor)Random.Range(0, 6));

            BlockMover.Instance.AddMoveBlock(createdBlock);
            BlockMover.Instance.StartMoveBlocks();
            yield return new WaitUntil(() => Mathf.Abs(createdBlock.transform.position.y - PuzzleSearch.Instance.RootSlot.transform.position.y) < 1f);
        }
        isCreating = false;
    }
}
