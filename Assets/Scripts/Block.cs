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
        Blue,
        Black
    }

    [SerializeField] protected BlockColor blockColor;
    public BlockColor Block_Color { get => blockColor; }
    public Vector3 originScale;
    public bool isMoving;
    public Slot.Direction moveDirection;

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

    public virtual bool Break() { Debug.Log(this + " 파괴");  return true; }
    
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

        renderer.color = CustomColor.GetColor(blockColor);
    }
}
