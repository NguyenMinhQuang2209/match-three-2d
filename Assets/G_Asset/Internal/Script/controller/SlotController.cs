using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    public static SlotController instance;
    public Slot slot;
    public Transform slot_container_ui;

    public Vector2Int size = new();
    private Dictionary<Vector2Int, Slot> slots = new();
    private Dictionary<string, Item> itemStores = new();
    private List<Slot> remainSlots = new();

    [SerializeField] private float waitTimer = 1f;
    [SerializeField] private float timeBwt = 0.5f;
    int current = 1;

    [SerializeField] private Sprite touchSprite;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite emptySprite;

    [SerializeField]
    private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;


    [Space(5)]
    [SerializeField] private List<Sprite> sprites = new();
    private List<Item> items = new();

    private Slot currentSlot = null;
    [Space(5)]
    [SerializeField] private List<Vector2Int> blocks = new();

    Vector2 firstTouch = new();
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    private void Start()
    {
        GetListItems();
        SpawnSlot();
        Invoke(nameof(SpawnItem), 1f);
    }
    public void GetListItems()
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            Item item = new(sprites[i], i.ToString());
            items.Add(item);
            itemStores[i.ToString()] = item;
        }
    }
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                firstTouch = touch.position;
                PointerEventData pointerEventData = new(eventSystem)
                {
                    position = touch.position
                };

                List<RaycastResult> results = new();
                raycaster.Raycast(pointerEventData, results);
                if (results.Count > 0)
                {
                    Slot touchSlot;
                    if (results[0].gameObject.TryGetComponent<Slot>(out touchSlot))
                    {

                    }
                    else
                    {
                        if (results[0].gameObject.transform.parent.TryGetComponent<Slot>(out touchSlot))
                        {

                        }
                    }
                    if (touchSlot != null)
                    {
                        TouchSlot(touchSlot);
                    }
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                CheckTouchEnd(touch.position);
            }
        }
    }
    public void TouchSlot(Slot nextSlot)
    {
        if (currentSlot != null)
        {
            currentSlot.ChangeSlotSprite(defaultSprite);
        }
        currentSlot = nextSlot;
        if (currentSlot != null)
        {
            currentSlot.ChangeSlotSprite(touchSprite);
        }
    }
    public Item GetRandomItem()
    {
        int ran = Random.Range(0, items.Count);
        return items[ran];
    }
    public void CheckTouchEnd(Vector2 endTouch)
    {
        Vector2 target = endTouch - firstTouch;
        if (Mathf.Abs(target.x) > Mathf.Abs(target.y))
        {
            // Left Right
            if (target.x < 0)
            {
                if (currentSlot != null)
                {
                    Vector2Int currentPos = currentSlot.GetPosition();
                    Vector2Int leftPos = new(currentPos.x - 1, currentPos.y);
                    if (slots.ContainsKey(leftPos))
                    {
                        CheckSlotItem(slots[leftPos]);
                    }
                }
            }
            else if (target.x > 0)
            {
                if (currentSlot != null)
                {
                    Vector2Int currentPos = currentSlot.GetPosition();
                    Vector2Int leftPos = new(currentPos.x + 1, currentPos.y);
                    if (slots.ContainsKey(leftPos))
                    {
                        CheckSlotItem(slots[leftPos]);
                    }
                }
            }
        }
        else
        {
            if (target.y > 0)
            {
                if (currentSlot != null)
                {
                    Vector2Int currentPos = currentSlot.GetPosition();
                    Vector2Int leftPos = new(currentPos.x, currentPos.y - 1);
                    if (slots.ContainsKey(leftPos))
                    {
                        CheckSlotItem(slots[leftPos]);
                    }
                }
            }
            else if (target.y < 0)
            {
                if (currentSlot != null)
                {
                    Vector2Int currentPos = currentSlot.GetPosition();
                    Vector2Int leftPos = new(currentPos.x, currentPos.y + 1);
                    if (slots.ContainsKey(leftPos))
                    {
                        CheckSlotItem(slots[leftPos]);
                    }
                }
            }
        }
    }
    public void CheckSlotItem(Slot nextSlot)
    {
        Item currentItem = currentSlot.GetItem();
        Item nextItem = nextSlot.GetItem();

        currentSlot.ChangeItem(nextItem, 0f);
        nextSlot.ChangeItem(currentItem, 0f);

        List<Vector2Int> currentCheck = currentSlot.CheckSlotHit();
        List<Vector2Int> nextCheck = nextSlot.CheckSlotHit();

        if (currentCheck.Count == 0 && nextCheck.Count == 0)
        {
            currentSlot.ChangeItem(currentItem, 0.1f);
            nextSlot.ChangeItem(nextItem, 0.1f);
        }
        else
        {
            if (currentCheck.Count > 0)
            {
                for (int i = 0; i < currentCheck.Count; i++)
                {
                    Slot slotCheck = slots[currentCheck[i]];
                    slotCheck.ChangeItemSprite(emptySprite);
                }
            }
            if (nextCheck.Count > 0)
            {
                for (int i = 0; i < nextCheck.Count; i++)
                {
                    Slot slotCheck = slots[nextCheck[i]];
                    slotCheck.ChangeItemSprite(emptySprite);
                }
            }
        }
    }
    public Slot GetSlot(Vector2Int pos)
    {
        return slots.ContainsKey(pos) ? slots[pos] : null;
    }
    public void SpawnSlot()
    {
        foreach (Transform child in slot_container_ui.transform)
        {
            Destroy(child.gameObject);
        }
        int x = size.x;
        int y = size.y;
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                Vector2Int pos = new(j, i);
                Slot tempSlot = Instantiate(slot, slot_container_ui.transform);
                tempSlot.SlotInit(pos, blocks.Contains(pos));
                slots[pos] = tempSlot;
                remainSlots.Add(tempSlot);
            }
        }
    }
    public void SpawnItem()
    {
        int x = size.x;
        int y = size.y;
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                Vector2Int pos = new(j, i);
                Slot tempSlot = slots[pos];
                Item item = GetRandomItem();
                tempSlot.ChangeItem(item, waitTimer + timeBwt * current);
                current++;
            }
        }
    }

}
public class Item
{
    public Sprite sprite;
    public string itemName = "";
    public Item(Sprite sprite, string itemName)
    {
        this.sprite = sprite;
        this.itemName = itemName;
    }
}