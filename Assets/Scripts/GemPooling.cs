using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemPooling : MonoBehaviour
{
    private static GemPooling instance;
    public static GemPooling Instance { get => instance; }

    [SerializeField] private List<Gem> gems;

    private void Awake()
    {
        instance = this;
    }

    public Gem GetUnuseGem()
    {
        Gem gem = gems.Find((x) => !x.gameObject.activeSelf);
        if(gem == null)
        {
            GameObject createdObj = Instantiate<GameObject>(gems[0].gameObject);
            createdObj.SetActive(true);
            gem = createdObj.GetComponent<Gem>();
            gems.Add(gem);
        }

        return gem;
    }

    public void ReturnGem(Gem gem)
    {
        gem.gameObject.SetActive(false);
        gem.transform.SetParent(transform);
    }
}
