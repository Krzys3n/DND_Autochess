using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SynergyUIItem : MonoBehaviour
{
    [Header("UI References")]
    public Image factionIcon;
    public TextMeshProUGUI factionNameText; // <-- NOWA ZMIENNA NA NAZWĘ
    public TextMeshProUGUI countText;
    public Image background;

    [Header("Colors")]
    public Color activeColor = new Color(1f, 0.8f, 0f, 0.6f); 
    public Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.6f); 

    public void Setup(SynergyData data, int currentCount)
    {
        // Zabezpieczenie przed brakiem danych
        if (data == null) return;

        // 1. Ustawienie ikonki (jeśli przypisana w Inspektorze)
        if (factionIcon != null) 
            factionIcon.sprite = data.icon;
        
        // 2. --- NOWA LOGIKA: NADPISUJEMY NAZWĘ FRAKCJI ---
        if (factionNameText != null)
        {
            // Pobieraj nazwę z pola synergyName, które dodaliśmy do SynergyData
            factionNameText.text = data.synergyName; 
        }

        // 3. Ustawienie cyfry licznika
        if (countText != null) 
            countText.text = currentCount.ToString(); 

        // 4. Sprawdzanie aktywności bonusu (dla zmiany koloru tła)
        bool isAnyActive = false;
        if (data.thresholds != null && data.thresholds.Length > 0)
        {
            if (currentCount >= data.thresholds[0]) isAnyActive = true;
        }

        // 5. Ustawienie koloru tła
        if (background != null)
        {
            background.color = isAnyActive ? activeColor : inactiveColor;
        }
    }
}