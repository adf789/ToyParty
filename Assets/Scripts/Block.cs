using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public enum BlockColor
    {
        Red,
        Green,
        Yellow,
        Orange,
        Purple,
        Blue
    }
    [SerializeField] protected BlockColor blockColor;
    public BlockColor Block_Color { get => blockColor; }
    public Vector3 originScale;

    private void Awake()
    {
        //originScale = transform.localScale;
    }

    public virtual void SetColor(BlockColor color)
    {
        this.blockColor = color;
        InitColor();
    }

    public virtual void Reset() { }

    public virtual bool Break() { return true; }
    
    public virtual bool IsSame(Block block) {
        if (this is Obstacle || block is Obstacle) return false;
        return blockColor == block.Block_Color;
    }

    protected virtual void InitColor()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            Debug.Log("Renderer 찾지 못함");
            return;
        };

        switch (blockColor)
        {
            case BlockColor.Red:
                renderer.color = new UnityEngine.Color(176 / 255f, 0, 0);
                break;
            case BlockColor.Green:
                renderer.color = UnityEngine.Color.green;
                break;
            case BlockColor.Yellow:
                renderer.color = UnityEngine.Color.yellow;
                break;
            case BlockColor.Orange:
                renderer.color = new UnityEngine.Color(1, 94 / 255f, 0);
                break;
            case BlockColor.Purple:
                renderer.color = new UnityEngine.Color(68 / 255f, 0, 183 / 255f);
                break;
            case BlockColor.Blue:
                renderer.color = UnityEngine.Color.blue;
                break;
        }
    }
}
