using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image rarityBorder; 
    public Image unitImage;    
    public Image factionIconImage; 
    public Image classIconImage; 
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;

    private int unitCost;
    private UnitData currentUnitData;
    private int currentTier;

    public void Setup(UnitData data, int tier, Color rarityColor)
    {
        currentUnitData = data;
        currentTier = data.rarity;
        unitCost = data.cost; 

        nameText.text = data.unitName;
        priceText.text = unitCost.ToString() + " G";
        rarityBorder.color = rarityColor;
        
        // 1. GŁÓWNY SPRITE JEDNOSTKI
        if (unitImage != null && data.unitSprite != null)
        {
            unitImage.sprite = data.unitSprite;
            unitImage.preserveAspect = true;
        }

        // 2. IKONA FRAKCJI (Faction)
        if (factionIconImage != null)
        {
            // Sprawdzamy czy w danych UnitData przypisano ikonę frakcji
            if (data.factionIcon != null)
            {
                factionIconImage.sprite = data.factionIcon;
                factionIconImage.preserveAspect = true;
                factionIconImage.gameObject.SetActive(true);
            }
            else
            {
                factionIconImage.gameObject.SetActive(false);
            }
        }

        
        // 3. IKONA KLASY (Class) - Obsługa Listy klas
        if (classIconImage != null)
        {
            // Sprawdzamy, czy lista klas istnieje i czy ma przynajmniej jeden element
            if (data.unitClasses != null && data.unitClasses.Count > 0) 
            {
                // Wybieramy pierwszą klasę z listy jako główną do wyświetlenia ikony w sklepie
                SynergyData primaryClass = data.unitClasses[0];

                if (primaryClass != null && primaryClass.icon != null)
                {
                    classIconImage.sprite = primaryClass.icon;
                    classIconImage.preserveAspect = true;
                    classIconImage.gameObject.SetActive(true);
                }
                else
                {
                    classIconImage.gameObject.SetActive(false);
                }
            }
            else
            {
                // Jeśli jednostka nie ma żadnej klasy
                classIconImage.gameObject.SetActive(false);
            }
        }
    }

    // Dodaj tę metodę pomocniczą
    public UnitData GetCurrentUnitData()
    {
        return currentUnitData;
    }

    public void OnBuyClicked()
    {
        // Wywołujemy zakup w ShopManagerze
        bool bought = ShopManager.Instance.BuyUnit(currentUnitData, unitCost, currentTier); 
        
        if (bought) 
        {
            // ZAMIAST: gameObject.SetActive(false);
            // ROBIMY TO:
            HideSlot();
        }
    }

    private void HideSlot()
    {
        // 1. Wyłączamy przycisk, żeby nie dało się kupić "powietrza"
        if (TryGetComponent<Button>(out Button btn)) btn.interactable = false;

        // 2. Wyłączamy wszystkie grafiki i teksty wewnątrz tego slotu
        // Dzięki temu obiekt transform nadal istnieje i trzyma miejsce w Layout Group
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 3. Opcjonalnie wyłączamy tło samego slotu (jeśli ma komponent Image)
        if (TryGetComponent<Image>(out Image mainImg)) mainImg.enabled = false;
    }
}