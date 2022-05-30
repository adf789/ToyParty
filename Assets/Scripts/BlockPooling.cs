using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPooling : MonoBehaviour
{
    private static BlockPooling instance;
    public static BlockPooling Instance { get => instance; }

    [SerializeField] private List<Gem> gems;
    [SerializeField] private List<Obstacle> obstacles;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PuzzleSearch.Instance.CheckAllSlots((slot) =>
        {
            if (slot.haveBlock == null) return;
            else if (slot.haveBlock is Gem) gems.Add((Gem)slot.haveBlock);
            else if (slot.haveBlock is Obstacle) obstacles.Add((Obstacle)slot.haveBlock);
        });
    }

    public Block GetUnuseGem()
    {
        Gem gem = gems.Find((x) => !x.gameObject.activeSelf);
        if(gem == null)
        {
            GameObject obj = Instantiate(PuzzleCreator.Instance.GemPrefab, transform);
            obj.name = string.Format("Gem{0}", ++PuzzleCreator.Instance.blockCount);
            gem = obj.GetComponent<Gem>();
            gems.Add(gem);
        }

        gem.gameObject.SetActive(true);
        gem.Reset();
        return gem;
    }

    public Block GetUnuseObstacle()
    {
        Obstacle obstacle = obstacles.Find((x) => !x.gameObject.activeSelf);
        if (obstacle == null)
        {
            GameObject obj = Instantiate(PuzzleCreator.Instance.ObstaclePrefab, transform);
            obj.name = string.Format("Obstacle{0}", ++PuzzleCreator.Instance.blockCount);
            obstacle = obj.GetComponent<Obstacle>();
            obstacles.Add(obstacle);
        }

        obstacle.gameObject.SetActive(true);
        obstacle.Reset();
        return obstacle;
    }

    public void ReturnBlock(Block block)
    {
        block.gameObject.SetActive(false);
        block.transform.SetParent(transform);
    }
}
