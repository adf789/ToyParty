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
    public Transform gemSpawnPoint;

    private void Awake()
    {
        instance = this;
    }

    public void AllSlotsRandomColor()
    {
        int index = 0;
        puzzleSearch.CheckAllSlots((slot) =>
        {
            slot.haveGem.SetColor((Gem.GemColor)Random.Range(0, 6));
            slot.index = index++;
        });
    }

    public void CreateGems(int count)
    {
        StartCoroutine(PlayCreateGemsAnim(count));
    }

    IEnumerator PlayCreateGemsAnim(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Gem createdGem = GemPooling.Instance.GetUnuseGem();
            createdGem.transform.position = gemSpawnPoint.position;
            createdGem.SetColor((Gem.GemColor)Random.Range(0, 6));
            createdGem.gameObject.SetActive(true);

            GemAnimation.Instance.MoveToEmptySlot(createdGem);
            yield return new WaitUntil(() => Mathf.Abs(createdGem.transform.position.y - gemSpawnPoint.position.y) > 2f);
        }
    }
}
