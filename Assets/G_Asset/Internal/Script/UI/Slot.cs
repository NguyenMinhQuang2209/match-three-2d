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
        if (timer > 0)
        {
            Invoke(nameof(ChangeSprite), timer);
        }
        else
        {
            ChangeSprite();
        }
    }
    public void ChangeSprite()
    {
        if (item != null)
        {
            icon.sprite = item.sprite;
        }
    }
    public bool IsBlock()
    {
        return isBlock;
    }
    public Vector2Int GetPosition()
    {
        return pos;
    }
    public void ChangeSlotSprite(Sprite sprite)
    {
        img.sprite = sprite;
    }
    public void ChangeItemSprite(Sprite newSprite = null)
    {
        icon.sprite = newSprite;
    }
    public string GetItemName()
    {
        return item.itemName;
    }
    public Item GetItem()
    {
        return item;
    }
    public bool IsEqual(Item newItem)
    {
        return newItem.itemName == item?.itemName;
    }

    public List<Vector2Int> CheckSlotHit()
    {
        List<Vector2Int> verticalSlots = new();
        List<Vector2Int> horizontalSlots = new();

        int countUp = TotalCount(verticalSlots, new(0, -1));
        int countDown = TotalCount(verticalSlots, new(0, 1));
        int countLeft = TotalCount(horizontalSlots, new(-1, 0));
        int countRight = TotalCount(horizontalSlots, new(1, 0));

        int countTopDown = countUp + countDown;
        int countLeftRight = countLeft + countRight;

        List<Vector2Int> finalList = new();

        bool wasAdd = false;

        if (countTopDown >= 2)
        {
            finalList.AddRange(verticalSlots);
            finalList.Add(GetPosition());
            wasAdd = true;
        }
        if (countLeftRight >= 2)
        {
            finalList.AddRange(horizontalSlots);
            if (!wasAdd)
            {
                finalList.Add(GetPosition());
            }
        }

        return finalList;
    }
    public int TotalCount(List<Vector2Int> list, Vector2Int change)
    {
        int total = 0;
        Vector2Int currentPos = pos;
        while (true)
        {
            Vector2Int nextDir = new(currentPos.x + change.x, currentPos.y + change.y);
            Slot checkSlot = SlotController.instance.GetSlot(nextDir);
            if (checkSlot == null)
            {
                return total;
            }

            if (checkSlot.IsBlock())
            {
                return total;
            }

            if (IsEqual(checkSlot.GetItem()))
            {
                total++;
                list.Add(nextDir);
            }
            else
            {
                return total;
            }
            currentPos = nextDir;
        }
    }
    public void SwitchSlotItem(float timer)
    {
        Invoke(nameof(SwitchSlotItem), timer);
    }
    public void SwitchSlotItem()
    {
        Vector2Int currentPos = new(pos.x, pos.y);
        while (true)
        {
            Vector2Int nextPos = new(currentPos.x, currentPos.y - 1);
            Slot nextSlot = SlotController.instance.GetSlot(nextPos);
            if (nextSlot == null)
            {
                Item ranItem = SlotController.instance.GetRandomItem();
                ChangeItem(ranItem, 0.02f);
                return;
            }
            else
            {
                if (!nextSlot.IsBlock())
                {
                    Item nextItem = nextSlot.GetItem();
                    ChangeItem(nextItem, 0.02f);
                    nextSlot.SwitchSlotItem();
                    return;
                }
                else
                {
                    currentPos = nextPos;
                }
            }
        }
    }
}
