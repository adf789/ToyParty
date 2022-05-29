using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCreator : MonoBehaviour
{
    private static PuzzleCreator instance;

    public static PuzzleCreator Instance
    {
        get
        {
            return instance;
        }
    }
    public PuzzleSearch puzzleSearch;
    public Transform blockSpawnPoint;
    private bool isCreating;
    public bool IsCreating { get => isCreating; }

    [SerializeField] private GameObject gemPrefab;
    public GameObject GemPrefab { get => gemPrefab; }
    [SerializeField] private GameObject obstaclePrefab;
    public GameObject ObstaclePrefab { get => obstaclePrefab; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(PuzzleStart());
    }

    public void AllSlotsRandomColor()
    {
        int index = 0;
        puzzleSearch.CheckAllSlots((slot) =>
        {
            if(slot.haveBlock == null) slot.haveBlock = slot.GetComponentInChildren<Block>();
            if(slot.haveBlock is Gem) slot.haveBlock.SetColor((Block.BlockColor)Random.Range(0, 6));
            slot.index = index++;
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
                    createBlock.Reset();
                    slot.SetBlock(createBlock);
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
                    createBlock.Reset();
                    slot.SetBlock(createBlock);
                }
            }
        });
    }

    public void CreateBlocks(int count)
    {
        StartCoroutine(PlayCreateBlocksAnim(count));
    }

    IEnumerator PuzzleStart()
    {
        isCreating = true;
        bool isComplete = false;

        while (!isComplete)
        {
            PuzzleSearch.Instance.CheckAllSlots((slot) =>
            {
                slot.transform.localScale = Vector3.Lerp(slot.transform.localScale, Vector3.one, 2 * Time.deltaTime);

                if (slot.transform.localScale == Vector3.one) isComplete = true;
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
            Block createdBlock = BlockPooling.Instance.GetUnuseGem();
            createdBlock.transform.position = blockSpawnPoint.position;
            createdBlock.SetColor((Block.BlockColor)Random.Range(0, 6));

            BlockAnimation.Instance.MoveToEmptySlot(createdBlock);
            yield return new WaitUntil(() => Mathf.Abs(createdBlock.transform.position.y - blockSpawnPoint.position.y) > 2f);
        }
        isCreating = false;
    }
}
