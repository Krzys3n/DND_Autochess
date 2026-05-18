using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "GlovesOfThieveryEffect", menuName = "Items/Effects/Gloves of Thievery")]
public class GlovesOfThieveryEffect : ItemEffect
{
    [Header("Item Pool (Auto-Loaded)")]
    public List<ItemData> possibleItemsToSteal = new List<ItemData>();

    // Lista nazw do zablokowania (uproszczona, skrypt sam usunie znaki specjalne)
    private readonly string[] blackList = new string[] 
    { 
        "GlovesOfThievery", "TomeOfUltimateMastery", "FightersMedallion", "RangersQuiver",
        "PaladinsVow", "BardsMantle", "RoguesWraps", "ClericsRelic",
        "WizardsSpellbook", "DruidsTotem"
    };

    public override void Apply(Unit target) { }

    public override void ApplyPassives(Unit owner)
    {
        // 1. Jeśli postać ma już tymczasowe itemy, nie losuj (blokada przed spamem)
        if (owner.tempItemCount > 0) return;

        if (possibleItemsToSteal == null || possibleItemsToSteal.Count == 0) LoadPool();
        if (possibleItemsToSteal.Count < 2) return;

        // 2. SZUKAMY INSTANCJI PRZEDMIOTU (nie pliku assetu, nie jednostki)
        // Szukamy w ekwipunku jednostki przedmiotu, który wywołał ten efekt
        ItemData myItemInstance = owner.equippedItems.FirstOrDefault(i => i != null && i.specialEffect == this);
        
        // Jeśli z jakiegoś powodu nie znajdzie (np. testy), używamy 0, 
        // ale przy poprawnym Instantiate w EquipItem zawsze znajdzie unikalne ID
        int itemID = (myItemInstance != null) ? myItemInstance.GetInstanceID() : 0;

        // 3. SEED: Sesja + Etap + Unikalne ID przedmiotu
        // Teraz wynik zależy tylko od tego, który to "egzemplarz" rękawic i który jest etap
        int finalSeed = GameManager.Instance.sessionSeed + 
                        GameManager.Instance.currentStage + 
                        itemID;

        Random.InitState(finalSeed);
        var stolenItems = possibleItemsToSteal.OrderBy(x => Random.value).Take(2).ToList();
        
        // Resetujemy losowość dla reszty silnika (np. szansy na uniki w walce)
        Random.InitState(System.Guid.NewGuid().GetHashCode());

        // 4. Nakładanie
        foreach (var item in stolenItems)
        {
            owner.AddTemporaryItem(item);
            if (item.specialEffect != null)
            {
                item.specialEffect.ApplyPassives(owner);
            }
        }

        Debug.Log($"<color=yellow>[Gloves]</color> {owner.unitName} (ItemID: {itemID}) Stage: {GameManager.Instance.currentStage} | Wylosowano: {stolenItems[0].itemName}, {stolenItems[1].itemName}");
    }

    private void LoadPool()
    {
        ItemData[] loadedItems = Resources.LoadAll<ItemData>("Items/FinishedItems");
        
        possibleItemsToSteal = loadedItems.Where(item => 
        {
            if (item == null || !item.isFinishedItem) return false;

            // FUNKCJA CZYSZCZĄCA NAZWĘ: Usuwa spacje, podłogi i apostrofy
            string cleanName = item.itemName.Replace(" ", "").Replace("_", "").Replace("'", "");
            
            // Sprawdzamy czy wyczyszczona nazwa jest na czarnej liście (również wyczyszczonej)
            bool isExcluded = blackList.Any(excluded => 
                string.Equals(excluded, cleanName, System.StringComparison.OrdinalIgnoreCase));

            return !isExcluded;
        }).ToList();

        // LOGI - teraz na pewno się pojawią przy ładowaniu
        Debug.Log($"<color=green><b>[Gloves Pool]</b></color> Załadowano <b>{possibleItemsToSteal.Count}</b> przedmiotów.");
        
        string names = string.Join(", ", possibleItemsToSteal.Select(i => i.itemName));
        Debug.Log("<color=cyan>[Pula zawiera]:</color> " + names);
    }

    // Dodajmy tę metodę, żebyś mógł zresetować pulę w Inspektorze (klikając prawym na komponent)
    [ContextMenu("Force Reload Pool")]
    public void ForceReload()
    {
        possibleItemsToSteal.Clear();
        LoadPool();
    }
}