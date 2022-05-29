using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Block
{
    protected new Vector3 originScale = new Vector3(1.8f, 1.8f, 1.8f);
    [SerializeField] private Sprite[] numberSprite;
    [SerializeField] private int curIndex;
    [SerializeField] private SpriteRenderer numberRenderer;
    private bool isDisappearing;


    public override void Reset()
    {
        base.Reset();
        transform.localScale = originScale;
        curIndex = 0;
        UpdateState();
        isDisappearing = false;
    }

    public override bool Break()
    {
        curIndex++;
        if (!UpdateState())
        {
            StartCoroutine(Disappear());
            return true;
        }
        else return false;
    }

    private bool UpdateState()
    {
        if (curIndex >= numberSprite.Length) return false;

        numberRenderer.sprite = numberSprite[curIndex];
        return true;
    }

    protected virtual void InitColor()
    {
        return;
    }

    IEnumerator Disappear()
    {
        if (isDisappearing) yield break;
        isDisappearing = true;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            Vector3 scale = originScale;
            while (renderer.color.a > 0.01f)
            {
                color.a = Mathf.Lerp(color.a, 0, 5 * Time.deltaTime);
                scale.x += 5f * Time.deltaTime;
                scale.y += 5f * Time.deltaTime;
                transform.localScale = scale;
                renderer.color = color;
                yield return null;
            }
        };

        transform.localScale = originScale;
        isDisappearing = false;
        BlockPooling.Instance.ReturnBlock(this);
    }
}
