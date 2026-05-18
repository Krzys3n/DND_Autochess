using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SynergyPanelUI : MonoBehaviour
{
    public GameObject itemPrefab; // Prefab SynergyUIItem
    public Transform container;   // Obiekt z Vertical Layout Group (Content w Scroll View)

    public void RefreshUI(Dictionary<SynergyData, int> counts, List<SynergyData> allData)
    {
        Debug.Log("Próbuję odświeżyć UI. Ilość synergii do pokazania: " + counts.Count);
        // 1. CZYŚCIMY LISTĘ
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 2. SORTOWANIE (Twoje wymagania: Aktywne > Frakcje > Ilość)
        var sortedList = counts.Keys.ToList();

        sortedList.Sort((a, b) => 
        {
            int countA = counts[a];
            int countB = counts[b];

            // Czy synergia A i B są aktywne (czy przekroczyły pierwszy próg)?
            bool aActive = a.thresholds != null && a.thresholds.Length > 0 && countA >= a.thresholds[0];
            bool bActive = b.thresholds != null && b.thresholds.Length > 0 && countB >= b.thresholds[0];

            // Zasada 1: Aktywne (złote) na górę
            if (aActive != bActive) return bActive.CompareTo(aActive);

            // Zasada 2: Frakcje nad Klasami (wymaga pola isFaction w SynergyData)
            if (a.isFaction != b.isFaction) return b.isFaction.CompareTo(a.isFaction);

            // Zasada 3: Większa ilość jednostek wyżej
            return countB.CompareTo(countA);
        });

        // 3. TWORZENIE ELEMENTÓW UI
        foreach (SynergyData data in sortedList)
        {
            if (counts[data] <= 0) continue; 

            GameObject obj = Instantiate(itemPrefab, container);
            
            // Pobieramy komponent i ustawiamy dane
            if (obj.TryGetComponent<SynergyUIItem>(out SynergyUIItem uiItem))
            {
                uiItem.Setup(data, counts[data]);
            }
        }
    }
}