using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Text; // POTRZEBNE DLA TOOLTIPA

// DODANO: IPointerEnterHandler, IPointerExitHandler
public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon; // Przeciągnij tutaj dziecko "Icon_Image"
    public ItemData currentItem;
    
    private CanvasGroup iconCanvasGroup;
    private Vector3 iconStartLocalPos;
    private Transform originalIconParent;

    void Awake()
    {
        // Pobieramy CanvasGroup z ikonki, nie ze slotu!
        iconCanvasGroup = itemIcon.GetComponent<CanvasGroup>();
        if (iconCanvasGroup == null) iconCanvasGroup = itemIcon.gameObject.AddComponent<CanvasGroup>();
        
        iconStartLocalPos = itemIcon.transform.localPosition;
        UpdateUI();
    }

    public void SetupSlot(ItemData data)
    {
        currentItem = data;
        
        if (itemIcon == null) return;

        if (currentItem != null && currentItem.itemIcon != null)
        {
            itemIcon.sprite = currentItem.itemIcon;
            itemIcon.enabled = true;
            
            // Upewnij się, że przezroczystość jest na 100%
            if (iconCanvasGroup != null) iconCanvasGroup.alpha = 1f;
            
            Color c = itemIcon.color;
            c.a = 1f;
            itemIcon.color = c;
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            if (iconCanvasGroup != null) iconCanvasGroup.alpha = 0f;
        }
    }

    private void UpdateUI()
    {
        if (itemIcon == null) return;

        if (currentItem != null)
        {
            itemIcon.sprite = currentItem.itemIcon;
            itemIcon.enabled = true;
            iconCanvasGroup.alpha = 1f;
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            iconCanvasGroup.alpha = 0f;
        }
    }

    // --- INTEGRACJA TOOLTIPA ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Wyświetlamy tylko gdy jest przedmiot i NIE trwa przeciąganie
        if (currentItem != null && !Mouse.current.leftButton.isPressed)
        {
            string description = BuildDescription(currentItem);
            TooltipSystem.Show(description, currentItem.itemName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }

    private string BuildDescription(ItemData data)
    {
        if (data == null) return "";
        StringBuilder sb = new StringBuilder();

        // 1. Lore / Description
        if (!string.IsNullOrEmpty(data.description))
        {
            sb.AppendLine($"<i>{data.description}</i>");
            sb.AppendLine();
        }

        // 2. Flat Stat Bonuses (Stałe wartości)
        
        // HEALTH - Zielony
        if (data.healthBonus > 0) 
            sb.AppendLine($"<color=#44FF44>+{data.healthBonus} Health</color>");
        
       // ATTACK DMG - Czerwony
        if (data.attackBonus > 0) 
            sb.AppendLine($"<color=#FF4444>+{data.attackBonus} Attack Damage</color>");

        // % ATTACK DMG - Ten sam czerwony
        if (data.attackDamagePercent > 0) 
            sb.AppendLine($"<color=#FF4444>+{data.attackDamagePercent}% Attack Damage</color>");
            
        // ABILITY POWER - Fioletowy
        if (data.abilityPowerBonus > 0) 
            sb.AppendLine($"<color=#CC44FF>+{data.abilityPowerBonus} Ability Power</color>");

        // ARMOR - Granatowy (Deep Blue)
        if (data.armorBonus > 0) 
            sb.AppendLine($"<color=#3366FF>+{data.armorBonus} Armor</color>");

        // MAGIC RESIST - Różowy (Hot Pink)
        if (data.magicResistBonus > 0) 
            sb.AppendLine($"<color=#FF66CC>+{data.magicResistBonus} Magic Resist</color>");

        // MANA REGEN - Niebieski
        // Upewnij się, że masz pole 'manaRegenBonus' w ItemData.cs
        if (data.startingMana > 0) 
            sb.AppendLine($"<color=#00CCFF>+{data.startingMana} Starting Mana</color>");
        if (data.manaRegen > 0) 
            sb.AppendLine($"<color=#00CCFF>+{data.manaRegen} Mana Regen</color>");

        // 3. Percent Stat Bonuses (Wartości procentowe)
        
        // ATTACK SPEED - Żółty
        if (data.attackSpeedPercent > 0) 
            sb.AppendLine($"<color=#FFFF44>+{data.attackSpeedPercent}% Attack Speed</color>");

        // CRIT CHANCE - Pomarańczowy/Złoty
        if (data.critChancePercent > 0) 
            sb.AppendLine($"<color=#FF8800>+{data.critChancePercent}% Crit Chance</color>");

        // 4. Recipe / Crafting Info
        sb.AppendLine();
        if (data.isFinishedItem && data.recipeComponentA != null && data.recipeComponentB != null)
        {
            sb.AppendLine("<color=#FFA500>RECIPE:</color>");
            sb.AppendLine($"<size=85%>{data.recipeComponentA.itemName} + {data.recipeComponentB.itemName}</size>");
        }
        else
        {
            sb.AppendLine("<color=#888888><size=85%>Basic Component</size></color>");
        }

        return sb.ToString();
    }

    // --- DRAG AND DROP (Twoja oryginalna logika) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        TooltipSystem.Hide(); // Ukryj przy starcie przeciągania
        originalIconParent = itemIcon.transform.parent;
        originalIconParent = itemIcon.transform.parent;
        itemIcon.transform.SetParent(GetComponentInParent<Canvas>().transform);
        itemIcon.transform.SetAsLastSibling();
        iconCanvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        
        // Ikonka podąża za myszką (New Input System)
        itemIcon.transform.position = Mouse.current.position.ReadValue();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        iconCanvasGroup.blocksRaycasts = true;
        itemIcon.transform.SetParent(originalIconParent);
        itemIcon.transform.localPosition = iconStartLocalPos;

        // 1. Logika Dropu na inny SLOT (Ławka)
        GameObject overGO = eventData.pointerCurrentRaycast.gameObject;
        if (overGO != null)
        {
            ItemSlot targetSlot = overGO.GetComponent<ItemSlot>();
            if (targetSlot == null) targetSlot = overGO.GetComponentInParent<ItemSlot>();

            if (targetSlot != null && targetSlot != this)
            {
                HandleItemDrop(targetSlot);
                return;
            }
        }
        
        // 2. Logika Dropu na JEDNOSTKĘ (Raycast2D)
        Vector2 mousePos = Pointer.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit.collider != null && hit.collider.TryGetComponent<Unit>(out Unit unit))
        {
            if (!unit.isEnemy)
            {
                // Używamy nowej metody TryAddItem, która sama obsłuży crafting i limity
                if (unit.EquipItem(currentItem))
                {
                    // Jeśli udało się dodać lub skraftować na jednostce, usuwamy ze slotu
                    BenchManager.Instance.RemoveItem(this);
                }
            }
        }
    }

    void CheckDropTarget()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            ItemSlot targetSlot = result.gameObject.GetComponentInParent<ItemSlot>();
            if (targetSlot != null && targetSlot != this)
            {
                HandleItemDrop(targetSlot);
                return;
            }
        }
        
        // Logika Dropu na jednostkę (Raycast2D)
        Vector2 mousePos = Pointer.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit.collider != null && hit.collider.TryGetComponent<Unit>(out Unit unit))
        {
            if (!unit.isEnemy)
            {
                // NAJPIERW sprawdź, czy to Consumable. 
                // Jeśli tak, zawsze pozwól na EquipItem (remover zadziała i sam przerwie działanie).
                if (currentItem.isConsumable)
                {
                    unit.EquipItem(currentItem);
                    BenchManager.Instance.RemoveItem(this);
                    return; // Kończymy, nie sprawdzamy limitów
                }

                // DOPIERO TUTAJ logika dla zwykłych przedmiotów (Crafting + Limity)
                if (unit.EquipItem(currentItem))
                {
                    BenchManager.Instance.RemoveItem(this);
                }
                else
                {
                    // Ten komunikat zobaczysz tylko dla zwykłych itemów przy 3/3 slotach
                    Debug.Log("Brak miejsca na przedmiot!");
                }
            }
        }
    }

    void HandleItemDrop(ItemSlot targetSlot)
    {
        ItemData draggedItem = this.currentItem;
        ItemData targetItem = targetSlot.currentItem;

        // Logika Craftingu na ławce
        if (targetItem != null && !draggedItem.isFinishedItem && !targetItem.isFinishedItem)
        {
            // Pytamy nową bazę danych o wynik połączenia
            ItemData craftedResult = ItemCraftingManager.Instance.CheckRecipe(draggedItem, targetItem);
            
            if (craftedResult != null)
            {
                targetSlot.SetupSlot(craftedResult);
                this.SetupSlot(null);
                Debug.Log($"Crafting udany: {craftedResult.itemName}");
                return;
            }
        }

        // Zwykły SWAP (zamiana miejscami)
        ItemData tempItem = targetSlot.currentItem;
        targetSlot.SetupSlot(draggedItem);
        this.SetupSlot(tempItem);
    }

}