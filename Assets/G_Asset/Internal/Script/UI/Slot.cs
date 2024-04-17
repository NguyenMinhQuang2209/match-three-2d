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
    public void SlotInit(Vector2Int pos, bool isBlock)
    {
        this.isBlock = isBlock;
        this.pos = pos;
        img.gameObject.SetActive(!isBlock);
        icon.gameObject.SetActive(!isBlock);
    }
    public void ChangeItem(Item newItem, float timer)
    {
        if (isBlock)
        {
            return;
        }
        item = newItem;
        Invoke(nameof(ChangeSprite), timer);
    }
    public void ChangeSprite()
    {
        if (item != null)
        {
            icon.sprite = item.sprite;
        }
    }
    public Vector2Int GetPosition()
    {
        return pos;
    }
    public void ChangeSlotSprite(Sprite sprite)
    {
        img.sprite = sprite;
    }
    public string GetItemName()
    {
        return item.itemName;
    }
    public Item GetItem()
    {
        return item;
    }

    public bool CheckSlotHit(Item newItem)
    {

        return true;
    }
}
