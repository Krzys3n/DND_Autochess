using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance;

    [Header("Settings")]
    public SynergyPanelUI uiPanel;         // Referencja do panelu UI
    public List<SynergyData> allSynergies; // Lista wszystkich plików SynergyData (przydatne do bonusów)

    // Słownik do przechowywania obliczonych bonusów (np. Frakcja -> Wartość Ataku)
    private Dictionary<SynergyData, int> activeBonuses = new Dictionary<SynergyData, int>();

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);
    }

    // Główna funkcja wywoływana, gdy jednostka zmienia pozycję
    public void RecalculateSynergies()
    {
        Dictionary<SynergyData, int> counts = new Dictionary<SynergyData, int>();
        HashSet<string> uniqueUnits = new HashSet<string>();

        // 1. Znajdujemy wszystkie jednostki na scenie
        Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        
        foreach (Unit u in units)
        {
            if (!u.isEnemy && u.currentTile != null && !u.currentTile.isBenchSlot)
            {
                if (!uniqueUnits.Contains(u.unitName))
                {
                    uniqueUnits.Add(u.unitName);
                    
                    // 1. Zliczanie frakcji (zostaje bez zmian)
                    if (u.unitData != null && u.unitData.faction != null)
                    {
                        SynergyData f = u.unitData.faction;
                        counts[f] = counts.ContainsKey(f) ? counts[f] + 1 : 1;
                    }
                    
                    // 2. NOWA LOGIKA: Zliczanie wszystkich klas z listy
                    if (u.unitData != null && u.unitData.unitClasses != null)
                    {
                        // Przechodzimy pętlą przez każdą klasę na liście
                        foreach (SynergyData c in u.activeClasses) // Zmień z u.unitData.unitClasses
                        {
                            if (c == null) continue;
                            counts[c] = counts.ContainsKey(c) ? counts[c] + 1 : 1;
                        }
                    }
                }
            }
        }

        // 2. Obliczamy wartości bonusów statystyk (opcjonalne, jeśli masz to w SynergyData)
        activeBonuses.Clear();
        foreach (var entry in counts)
        {
            // Pobieramy bonus z pliku SynergyData na podstawie liczby jednostek
            // Zakładam, że masz metodę GetTotalBonus w SynergyData
            activeBonuses[entry.Key] = entry.Key.GetTotalBonus(entry.Value);
        }

        // 3. Aplikujemy bonusy do jednostek na planszy
        ApplyBonusesToUnits(units);

        // 4. Przekazujemy przeliczone dane do UI (Sortowanie odbywa się wewnątrz RefreshUI lub tutaj)
        if (uiPanel != null)
        {
            // Dodajemy drugi argument: allSynergies
            uiPanel.RefreshUI(counts, allSynergies); 
        }

        Debug.Log("Synergie przeliczone.");
    }

    void ApplyBonusesToUnits(Unit[] units)
    {
        foreach (Unit u in units)
        {
            if (u.isEnemy || (u.unitData == null && u.enemyData == null)) continue;

            // ZAMIAST ręcznego przypisywania u.attack, wywołujemy naszą nową logikę
            u.RecalculateStats(); 
        }
    }
}