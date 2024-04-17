using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    public static SlotController instance;
    public Slot slot;
    public Transform slot_container_ui;
    public GridLayoutGroup group;
    public TextMeshProUGUI pointTxt;
    public TextMeshProUGUI levelTxt;
    int point = 0;

    private readonly Dictionary<Vector2Int, Slot> totalSlots = new();
    private readonly Dictionary<Vector2Int, Slot> slots = new();
    private readonly Dictionary<string, Item> itemStores = new();
    private readonly List<Slot> remainSlots = new();

    [SerializeField] private float waitTimer = 1f;
    [SerializeField] private float timeBwt = 0.5f;
    int current = 1;

    [SerializeField] private float waitCheckTimer = 1f;
    [SerializeField] private float waitCheckTimerPerTime = 0.2f;

    [SerializeField] private Sprite touchSprite;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite emptySprite;

    [SerializeField]
    private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;

    private List<Item> items = new();

    private Slot currentSlot = null;

    [SerializeField] private List<LevelScriptable> levels = new();
    int currentLevel = 0;

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
        ChangeLevel();
    }
    public void GetListItems(List<Sprite> sprites)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            Item item = new(sprites[i], i.ToString());
            items.Add(item);
            itemStores[i.ToString()] = item;
        }
    }
    public void ChangeLevel()
    {
        GetListItems(levels[currentLevel].sprites);
        SpawnSlot(levels[currentLevel].size, levels[currentLevel].blocksList);
        SpawnItem();

        pointTxt.text = "Point: " + point.ToString();
        levelTxt.text = "Level: " + currentLevel.ToString();
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
    public void CheckAllSlots()
    {
        CheckAllSlots(false);
    }
    public void CheckAllSlots(bool countPoint)
    {
        Vector2Int size = levels[currentLevel].size;
        int x = size.x;
        int y = size.y;
        List<Vector2Int> hitList = new();
        do
        {
            hitList?.Clear();
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    Vector2Int pos = new(j, i);
                    Slot tempSlot = slots[pos];
                    List<Vector2Int> hits = tempSlot.CheckSlotHit();
                    if (hits.Count > 0)
                    {
                        hitList.AddRange(hits);
                    }
                }
            }

            if (hitList.Count > 0)
            {
                hitList.Sort((a, b) =>
                {
                    int compareY = a.y.CompareTo(b.y);
                    if (compareY != 0)
                    {
                        return compareY;
                    }
                    return a.x.CompareTo(b.x);
                });

                for (int i = 0; i < hitList.Count; i++)
                {
                    Slot checkSlot = slots[hitList[i]];
                    float timer = waitCheckTimer + waitCheckTimerPerTime * i;
                    checkSlot.SwitchSlotItem(timer);
                }
                if (countPoint)
                {
                    //point += hitList.Count;
                    CheckPoint();
                }
            }
        } while (hitList.Count > 0);
    }
    public void CheckPoint()
    {
        pointTxt.text = "Point: " + point.ToString();
        if (point >= levels[currentLevel].requirePoint && !IsMaxLevel())
        {
            LevelUp();
        }
    }
    public bool IsMaxLevel()
    {
        return currentLevel == levels.Count - 1;
    }
    public void LevelUp()
    {
        int nextLevel = Mathf.Min(currentLevel + 1, levels.Count - 1);
        if (currentLevel != nextLevel)
        {
            currentLevel = nextLevel;
            ChangeLevel();
        }
    }
    public void CheckSlotItem(Slot nextSlot)
    {
        if (nextSlot.IsBlock())
        {
            return;
        }
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
            List<Vector2Int> checkList = new();
            if (currentCheck.Count > 0)
            {
                for (int i = 0; i < currentCheck.Count; i++)
                {
                    Slot slotCheck = slots[currentCheck[i]];
                    slotCheck.ChangeItemSprite(emptySprite);
                }
                checkList.AddRange(currentCheck);
            }
            if (nextCheck.Count > 0)
            {
                for (int i = 0; i < nextCheck.Count; i++)
                {
                    Slot slotCheck = slots[nextCheck[i]];
                    slotCheck.ChangeItemSprite(emptySprite);
                }
                checkList.AddRange(nextCheck);
            }
            if (checkList.Count > 0)
            {
                point += checkList.Count;
                CheckPoint();
                checkList.Sort((a, b) =>
                {
                    int compareY = a.y.CompareTo(b.y);
                    if (compareY != 0)
                    {
                        return compareY;
                    }
                    return a.x.CompareTo(b.x);
                });
                for (int i = 0; i < checkList.Count; i++)
                {
                    Slot checkSlot = slots[checkList[i]];
                    float timer = waitCheckTimer + waitCheckTimerPerTime * i;
                    checkSlot.SwitchSlotItem(timer);
                }

                CheckAllSlots(true);
            }
        }
    }
    public Slot GetSlot(Vector2Int pos)
    {
        return slots.ContainsKey(pos) ? slots[pos] : null;
    }
    public void SpawnSlot(Vector2Int size, List<Vector2Int> blocks)
    {
        foreach (Transform child in slot_container_ui.transform)
        {
            Destroy(child.gameObject);
        }
        int x = size.x;
        int y = size.y;
        group.constraintCount = x;
        slots?.Clear();
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                Vector2Int pos = new(j, i);
                Slot tempSlot;
                if (!totalSlots.ContainsKey(pos))
                {
                    tempSlot = Instantiate(slot, slot_container_ui.transform);
                }
                else
                {
                    tempSlot = totalSlots[pos];
                }
                tempSlot.SlotInit(pos, blocks.Contains(pos));
                totalSlots[pos] = tempSlot;
                slots[pos] = tempSlot;
                remainSlots.Add(tempSlot);
            }
        }
    }
    public void SpawnItem()
    {
        Vector2Int size = levels[currentLevel].size;
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
        float totalWaiter = waitTimer + timeBwt * current;
        Invoke(nameof(CheckAllSlots), totalWaiter);
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