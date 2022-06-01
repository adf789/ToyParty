using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPooling : Singleton<BlockPooling>
{
    [SerializeField] private List<Gem> gems = new List<Gem>();
    [SerializeField] private List<Obstacle> obstacles = new List<Obstacle>();

    public void Init()
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
            obj.name = "Gem";
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
            obj.name = "Obstacle";
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
