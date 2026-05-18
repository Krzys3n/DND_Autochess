using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Potrzebne dla Coroutine

public class BenchManager : MonoBehaviour
{
    public static BenchManager Instance;

    [Header("Settings")]
    public GameObject slotPrefab;
    public int maxSlots = 16;

    [Header("Debug / Testing")]
    public List<ItemData> startingItems; // Przeciągnij tu swoje SO przedmiotów w Inspektorze
    public List<ItemData> allItemsDatabase; // Przeciągnij tu wszystkie SO przedmiotów w inspektorze
    public List<ItemSlot> spawnedSlots = new List<ItemSlot>();

    void Awake()
    {
        Instance = this;

        // KLONUJEMY TUTAJ (Przed InitializeBench!)
        for (int i = 0; i < startingItems.Count; i++)
        {
            if (startingItems[i] != null)
            {
                startingItems[i] = Instantiate(startingItems[i]);
            }
        }

        InitializeBench();
    }


    void Start()
    {

    }

void InitializeBench()
{
    foreach (Transform child in transform) Destroy(child.gameObject);
    spawnedSlots.Clear();

    for (int i = 0; i < maxSlots; i++)
    {
        GameObject newSlot = Instantiate(slotPrefab, transform);
        ItemSlot slotScript = newSlot.GetComponent<ItemSlot>();
        
        // USUNIĘTO: TooltipTrigger tooltip = ...
        // Manager nie musi już dotykać tooltipa!

        if (i < startingItems.Count && startingItems[i] != null)
        {
            slotScript.SetupSlot(startingItems[i]); 
        }
        else
        {
            slotScript.SetupSlot(null);
        }

        newSlot.SetActive(true);
        spawnedSlots.Add(slotScript);
    }
}

    public bool AddItem(ItemData data)
    {
        foreach (var slot in spawnedSlots)
        {
            if (slot.currentItem == null)
            {
                slot.gameObject.SetActive(true);
                slot.SetupSlot(data);
                Debug.Log($"Dodano przedmiot do ławki: {data.itemName}");
                return true;
            }
        }
        Debug.LogWarning("Brak miejsca na ławce!");
        return false;
    }

    public void RemoveItem(ItemSlot slot)
    {
        slot.SetupSlot(null);
    }

    public ItemData GetCraftResult(ItemData ingredientA, ItemData ingredientB)
    {
        // Przeszukujemy naszą bazę wszystkich dostępnych w grze przedmiotów
        foreach (ItemData potentialResult in allItemsDatabase)
        {
            // Pytamy dany przedmiot: "Czy powstajesz z tych dwóch składników?"
            if (potentialResult.IsCraftableFrom(ingredientA, ingredientB))
            {
                return potentialResult;
            }
        }
        return null; // Jeśli żadna recepta nie pasuje
    }
}