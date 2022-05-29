using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public enum GemColor
    {
        Red,
        Green,
        Yellow,
        Orange,
        Purple,
        Blue
    }
    [SerializeField] private GemColor color;

    public GemColor Color { get => color; }

    public void SetColor(GemColor color)
    {
        this.color = color;
        InitColor();
    }

    public void Break()
    {
        //GemPooling.Instance.ReturnGem(this);
        StartCoroutine(Disappear());
    }

    private void InitColor()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            Debug.Log("Renderer ã�� ����");
            return;
        };

        switch (color)
        {
            case GemColor.Red:
                renderer.color = new UnityEngine.Color(176 / 255f, 0, 0);
                break;
            case GemColor.Green:
                renderer.color = UnityEngine.Color.green;
                break;
            case GemColor.Yellow:
                renderer.color = UnityEngine.Color.yellow;
                break;
            case GemColor.Orange:
                renderer.color = new UnityEngine.Color(1, 94 / 255f, 0);
                break;
            case GemColor.Purple:
                renderer.color = new UnityEngine.Color(68 / 255f, 0, 183 / 255f);
                break;
            case GemColor.Blue:
                renderer.color = UnityEngine.Color.blue;
                break;
        }
    }

    IEnumerator Disappear()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            while (renderer.color.a != 0)
            {
                color.a = Mathf.Lerp(color.a, 0, 3 * Time.deltaTime);
                renderer.color = color;
                yield return null;
            }
        };

        GemPooling.Instance.ReturnGem(this);
    }
}