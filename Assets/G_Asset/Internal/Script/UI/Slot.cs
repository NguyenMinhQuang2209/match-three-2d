using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private Vector2Int pos;
    public Image img;
    public Image icon;
    private Item item = null;
    private bool isBlock = false;
    public void SlotInit(Vector2Int pos, Item item, bool isBlock)
    {
        this.isBlock = isBlock;
        this.pos = pos;
        this.item = item;
        if (item != null)
        {
            icon.sprite = item.sprite;
        }
        img.gameObject.SetActive(!isBlock);
        icon.gameObject.SetActive(!isBlock);
    }
    public Vector2Int GetPosition()
    {
        return pos;
    }
    public void ChangeSlotSprite(Sprite sprite)
    {
        img.sprite = sprite;
    }
}