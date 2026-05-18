using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleStatsTrackerUI : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI totalValueText; 
    public TextMeshProUGUI val1Text; // Phys / Heal
    public TextMeshProUGUI val2Text; // Mag / Shields Gen
    public TextMeshProUGUI val3Text; // Shields Absorb / True Damage

    [Header("Bar Components (flexibleWidth)")]
    public LayoutElement fill1Layout;      
    public LayoutElement fill2Layout;      
    public LayoutElement fillTrueLayout;   
    
    public LayoutElement barContainerLayout; 
    public LayoutElement spacerLayout;       

    [Header("Visuals (colors)")]
    public Image fill1Image;
    public Image fill2Image;
    public Image fillTrueImage; 

    public void SetupUniversal(string uName, float v1, float v2, float v3, float maxVal, TrackerMode mode)
    {
        nameText.text = uName;

        float totalToDisplay = 0;
        float totalVisualWidth = v1 + v2 + v3; 

        // Definiujemy kolory bazowe
        Color c1 = Color.red; 
        Color c2 = Color.blue; 
        Color c3 = Color.white; 

        if (val3Text != null) val3Text.gameObject.SetActive(false);

        switch (mode)
        {
            case TrackerMode.Damage:
                totalToDisplay = v1 + v2 + v3;
                c1 = new Color(0.9f, 0.2f, 0.2f); // Czerwony (Phys)
                c2 = new Color(0.2f, 0.5f, 1f);   // Niebieski (Mag)
                c3 = Color.white;                  // Biały (True)
                
                val1Text.text = Mathf.RoundToInt(v1).ToString();
                val2Text.text = Mathf.RoundToInt(v2).ToString();
                if (val3Text != null && v3 > 0) 
                {
                    val3Text.gameObject.SetActive(true);
                    val3Text.text = Mathf.RoundToInt(v3).ToString();
                }
                break;

            case TrackerMode.Tanking:
                totalToDisplay = v1 + v2;
                c1 = new Color(0.6f, 0.4f, 0.2f); // Brązowy (Armor)
                c2 = new Color(0.6f, 0.2f, 0.8f); // Fioletowy (MR)
                c3 = Color.clear;
                
                val1Text.text = Mathf.RoundToInt(v1).ToString();
                val2Text.text = Mathf.RoundToInt(v2).ToString();
                break;

            case TrackerMode.Healing:
                totalToDisplay = v1 + v3; // Heal + Absorbed

                c1 = new Color(0.2f, 0.8f, 0.2f); // Zielony (Heal)
                c2 = new Color(0.6f, 0.8f, 1f);   // Jasny Niebieski (Gen)
                c3 = new Color(0.1f, 0.3f, 0.8f); // Ciemny Niebieski (Abs)

                val1Text.text = "H:" + Mathf.RoundToInt(v1);
                val2Text.text = "G:" + Mathf.RoundToInt(v2);
                if (val3Text != null) 
                {
                    val3Text.gameObject.SetActive(true);
                    val3Text.text = "A:" + Mathf.RoundToInt(v3);
                }
                break;
        }

        // --- KLUCZOWA ZMIANA: Dopasowanie kolorów tekstów do kolorów pasków ---
        if (val1Text != null) val1Text.color = c1;
        if (val2Text != null) val2Text.color = c2;
        if (val3Text != null) val3Text.color = c3;

        // Ustawienie głównej liczby
        totalValueText.text = Mathf.RoundToInt(totalToDisplay).ToString();

        // Aplikacja kolorów do obrazków pasków
        if(fill1Image != null) fill1Image.color = c1;
        if(fill2Image != null) fill2Image.color = c2;
        if(fillTrueImage != null) fillTrueImage.color = c3;

        // Logika rozmiarów pasków
        fill1Layout.flexibleWidth = Mathf.Max(0.0001f, v1);
        fill2Layout.flexibleWidth = Mathf.Max(0.0001f, v2);
        fillTrueLayout.flexibleWidth = Mathf.Max(0.0001f, v3);

        if (maxVal > 0)
        {
            barContainerLayout.flexibleWidth = totalVisualWidth;
            spacerLayout.flexibleWidth = maxVal - totalVisualWidth;
        }
        else
        {
            barContainerLayout.flexibleWidth = 0;
            spacerLayout.flexibleWidth = 1;
        }
    }
}