using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Resources")]
    public int gold = 10;
    public int currentLevel = 1;
    public int xp = 0;
    public int xpToNextLevel = 2;
    public int maxLevel = 8;

    [Header("UI Elements")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI levelText;
    public Transform cardsContainer;
    public GameObject shopSlotPrefab;

    [Header("Unit Pools")]
    public List<UnitData> allUnitData; // Lista Twoich Scriptable Objects
    public GameObject baseUnitPrefab; // Jeden wspólny prefab dla wszystkich postaci
    

    private int[][] shopChances = new int[][]
    {
        new int[] { 100, 0,  0,  0,  0 },  // Lvl 1
        new int[] { 100, 0,  0,  0,  0 },  // Lvl 2
        new int[] { 75,  25, 0,  0,  0 },  // Lvl 3
        new int[] { 55,  30, 15, 0,  0 },  // Lvl 4
        new int[] { 45,  33, 20, 2,  0 },  // Lvl 5
        new int[] { 25,  40, 30, 5,  0 },  // Lvl 6
        new int[] { 19,  30, 35, 15, 1 },  // Lvl 7
        new int[] { 15,  20, 35, 25, 5 }   // Lvl 8
    };

    [Header("Pool Settings")]
    // Słownik: UnitData -> Ilość pozostałych kopii w puli
    private Dictionary<UnitData, int> unitPool = new Dictionary<UnitData, int>();

    // Twoje zdefiniowane limity ilości kart dla rzadkości (rarity 1-5)
    private int[] rarityPoolSizes = { 30, 25, 18, 10, 9 };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializePool(); // Wypełnij worek kartami
        UpdateUI();
        RefreshShop();
    }

    // Inicjalizacja: mnożymy UnitData przez ilość kopii w puli
    void InitializePool()
    {
        unitPool.Clear();
        foreach (UnitData data in allUnitData)
        {
            // Rarity w UnitData jest 1-5, tablica ma 0-4, więc odejmujemy 1
            int rarityIndex = Mathf.Clamp(data.rarity - 1, 0, 4);
            int count = rarityPoolSizes[rarityIndex];
            
            unitPool[data] = count;
        }
        Debug.Log("Pula kart zainicjalizowana.");
    }

    // Szuka jednostki o konkretnej rzadkości, która jeszcze jest w puli
    UnitData GetRandomUnitFromPool(int rarity)
    {
        // Filtrujemy pulę: tylko jednostki o danym rarity, które mają > 0 sztuk
        var availableUnits = unitPool.Keys
            .Where(u => u.rarity == rarity && unitPool[u] > 0)
            .ToList();

        if (availableUnits.Count == 0)
        {
            // Jeśli zabrakło jednostek danej rzadkości, spróbuj o poziom niżej
            if (rarity > 1) return GetRandomUnitFromPool(rarity - 1);
            return null;
        }

        UnitData selected = availableUnits[Random.Range(0, availableUnits.Count)];
        unitPool[selected]--; // Zabieramy jedną sztukę z puli
        return selected;
    }
    public void ReturnToPool(UnitData data)
    {
        if (data != null && unitPool.ContainsKey(data))
        {
            unitPool[data]++;
        }
    }
    void ReturnUnboughtCardsToPool()
    {
        foreach (Transform child in cardsContainer)
        {
            ShopSlotUI slot = child.GetComponent<ShopSlotUI>();
            
            // KLUCZOWA ZMIANA: Sprawdzamy, czy slot jest aktywny. 
            // Kupione sloty są ukrywane przez HideSlot() (ich dzieci lub one same są nieaktywne).
            // Dodatkowo sprawdzamy, czy slot ma w ogóle przypisane dane.
            if (slot != null && slot.gameObject.activeSelf) 
            {
                // Sprawdzamy czy w środku slotu elementy są aktywne 
                // (bo Twoje HideSlot wyłącza dzieci, a nie sam slot)
                bool isAlreadyBought = false;
                if (child.childCount > 0 && !child.GetChild(0).gameObject.activeSelf)
                {
                    isAlreadyBought = true;
                }

                if (!isAlreadyBought && slot.GetCurrentUnitData() != null)
                {
                    ReturnToPool(slot.GetCurrentUnitData());
                }
            }
        }
    }

    public void ManualRefresh()
    {
        if (gold >= 2)
        {
            gold -= 2;
            RefreshShop(); // RefreshShop już samo zadba o zwrot kart do puli
            UpdateUI();
        }
        else
        {
            Debug.Log("Za mało złota!");
        }
    }

    public void RefreshShop()
    {
        // 1. Najpierw zwracamy niesprzedane karty do puli (zanim je zniszczymy)
        ReturnUnboughtCardsToPool();

        // 2. Czyścimy stare sloty
        foreach (Transform child in cardsContainer) Destroy(child.gameObject);

        // 3. Losujemy 5 nowych kart
        for (int i = 0; i < 5; i++)
        {
            // Losujemy rarity na podstawie poziomu gracza (1-5)
            int rarity = GetRandomTierByLevel();

            // Pobieramy jednostkę z puli (to odejmie 1 sztukę z worka)
            UnitData data = GetRandomUnitFromPool(rarity);

            if (data != null)
            {
                GameObject slot = Instantiate(shopSlotPrefab, cardsContainer);
                ShopSlotUI slotUI = slot.GetComponent<ShopSlotUI>();
                
                // Ustalanie koloru na podstawie rarity z UnitData
                Color rarityColor = data.rarity switch
                {
                    1 => Color.gray,
                    2 => Color.green,
                    3 => Color.blue,
                    4 => new Color(0.6f, 0, 1),
                    5 => new Color(1f, 0.84f, 0),
                    _ => Color.gray
                };

                // WAŻNE: Przekazujemy data.rarity a nie wylosowany tier, 
                // bo GetRandomUnitFromPool mogło zwrócić niższą jednostkę jeśli wyższych brakło
                slotUI.Setup(data, data.rarity, rarityColor);
            }
        }
    }

    int GetRandomTierByLevel()
    {
        int roll = Random.Range(1, 101);
        int[] chances = shopChances[Mathf.Clamp(currentLevel - 1, 0, 7)];

        int cumulativeChance = 0;
        for (int i = 0; i < chances.Length; i++)
        {
            cumulativeChance += chances[i];
            if (roll <= cumulativeChance) return i + 1;
        }
        return 1;
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold: " + gold;
        
        if (levelText != null)
        {
            int currentOnArena = BoardManager.Instance.GetCurrentUnitsOnArenaCount();
            string xpDisplay = (currentLevel < maxLevel) ? $"{xp}/{xpToNextLevel}" : "MAX";
            levelText.text = $"Lvl: {currentLevel} ({currentOnArena}/{MaxTeamSize} Units)\nXP: {xpDisplay}";
        }
    }

    // Kupowanie przyjmuje teraz UnitData i wstrzykuje je do BoardManagera
    public bool BuyUnit(UnitData data, int cost, int tier)
    {
        if (gold >= cost)
        {
            if (BoardManager.Instance.HasFreeBenchSlot())
            {
                gold -= cost;
                UpdateUI();
                
                // 1. Spawnuj jednostkę
                BoardManager.Instance.SpawnUnit(data, tier);
                
                // 2. NATYCHMIAST sprawdź, czy można ją ewoluować
                CheckForEvolution(data);
                
                return true;
            }
        }
        return false;
    }

    private void CheckForEvolution(UnitData data)
    {
        // Szukamy ewolucji kaskadowo: najpierw z 1 na 2, potem z 2 na 3
        TryCombine(data, 1); // Sprawdź 1-gwiazdkowe
        TryCombine(data, 2); // Sprawdź 2-gwiazdkowe
    }

   private void TryCombine(UnitData data, int levelToCombine)
    {
        List<Unit> allUnits = BoardManager.Instance.GetAllPlayerUnits();
        
        // Szukamy pasujących jednostek
        List<Unit> matches = allUnits.FindAll(u => u.unitData == data && u.evolutionLevel == levelToCombine);

        if (matches.Count >= 3)
        {
            Debug.Log($"Ewolucja! Łączenie 3 sztuk {data.unitName} Lvl {levelToCombine}");

            // 1. Wybieramy jednostkę, która przetrwa (survivor)
            Unit survivor = matches.Find(u => u.currentTile != null && !u.currentTile.isBenchSlot);
            if (survivor == null) survivor = matches[0];

            // 2. Zbieramy przedmioty z pozostałych jednostek i usuwamy je
            List<ItemData> gatheredItems = new List<ItemData>();
            int removedCount = 0;

            foreach (Unit u in matches)
            {
                if (u == survivor) continue;
                if (removedCount >= 2) break; // Chcemy usunąć tylko 2 jednostki, by z 3 została 1

                // Pobieramy przedmioty przed zniszczeniem
                if (u.equippedItems != null && u.equippedItems.Count > 0)
                {
                    gatheredItems.AddRange(u.equippedItems);
                    u.equippedItems.Clear(); // Czyścimy, by uniknąć problemów przy niszczeniu
                }

                // Usuwamy jednostkę z pola/ławki
                if (u.currentTile != null) u.currentTile.currentUnit = null;
                
                Destroy(u.gameObject);
                removedCount++;
            }

            // 3. Przekazujemy zebrane przedmioty do survivora
            foreach (ItemData item in gatheredItems)
            {
                // EquipItem obsłuży crafting i limity slotów
                bool success = survivor.EquipItem(item);

                // Jeśli brak miejsca i nie da się skraftować - przedmiot wraca na ławkę
                if (!success)
                {
                    Debug.Log($"[Combine] Brak miejsca u survivora na {item.itemName}. Wraca na ławkę.");
                    BenchManager.Instance.AddItem(item);
                }
            }

            // 4. Awansujemy ocalałą jednostkę
            survivor.evolutionLevel++;
            survivor.LoadDataFromSO(); // Zakładam, że ta metoda odświeża staty bazowe
            survivor.ApplyEvolutionVisuals();
            
            // Bardzo ważne: po ewolucji przeliczamy statystyki z uwzględnieniem przedmiotów
            survivor.RecalculateStats(); 

            // 5. Rekurencyjne sprawdzanie kolejnego poziomu (np. z trzech 2* zrób 3*)
            if (survivor.evolutionLevel < 3)
            {
                TryCombine(data, survivor.evolutionLevel);
            }
            
            SynergyManager.Instance.RecalculateSynergies();
        }
    }

    public void SellUnit(Unit unit)
    {
        if (unit == null) return;
        unit.ClearTemporaryItems();
        // 1. ZWROT PRZEDMIOTÓW NA ŁAWKĘ (Kluczowy krok!)
        // Musi się to stać przed zniszczeniem obiektu, 
        // aby lista przedmiotów wewnątrz jednostki była jeszcze dostępna.
        unit.ReturnItemsToBench();

        // 2. LOGIKA ZŁOTA
        int baseCost = unit.unitData.cost;
        int refund = 0;

        // Schemat: 1* = cost, 2* = (cost*3)-1, 3* = (cost*9)-1
        if (unit.evolutionLevel == 1) refund = baseCost;
        else if (unit.evolutionLevel == 2) refund = (baseCost * 3) - 1;
        else if (unit.evolutionLevel == 3) refund = (baseCost * 9) - 1;

        gold += refund;

        // 3. ZWROT KOPII DO PULI SKLEPU
        int copiesToReturn = (int)Mathf.Pow(3, unit.evolutionLevel - 1);
        for (int i = 0; i < copiesToReturn; i++)
        {
            ReturnToPool(unit.unitData);
        }

        // 4. CZYSZCZENIE MAPY I NISZCZENIE
        if (unit.currentTile != null) unit.currentTile.currentUnit = null;
        
        // Niszczymy obiekt jednostki
        Destroy(unit.gameObject);
        
        // 5. AKTUALIZACJA SYSTEMÓW
        UpdateUI();
        SynergyManager.Instance.RecalculateSynergies();
    }

public void BuyXP()
{
    if (currentLevel >= maxLevel) return;
    if (gold >= 4)
    {
        gold -= 4;
        AddExperience(4);
    }
}

    void LevelUp()
    {
        currentLevel++;
        xp -= xpToNextLevel;
        switch (currentLevel)
        {
            case 2: xpToNextLevel = 6; break;
            case 3: xpToNextLevel = 10; break;
            case 4: xpToNextLevel = 20; break;
            case 5: xpToNextLevel = 36; break;
            case 6: xpToNextLevel = 60; break;
            case 7: xpToNextLevel = 68; break;
            case 8: xpToNextLevel = 0; xp = 0; break;
        }
    }
    // Wewnątrz klasy ShopManager
    public void AdvanceToNextStage()
    {
        // 1. Dodaj darmowe 2 XP
        AddExperience(2);
        
        // 2. Dodaj 5 golda za ukończenie walki
        gold += 5;
        
        // 3. Automatycznie odśwież sklep (za darmo)
        RefreshShop();
        
        // 4. Odśwież UI, żeby gracz widział nową kasę i XP
        UpdateUI();
        
        Debug.Log("Nowy etap: +5 Golda, +2 XP i darmowy Refresh!");
    }

    // Upewnij się, że Twoja metoda AddExperience aktualizuje też UI
    public void AddExperience(int amount)
    {
        xp += amount;
        
        // Sprawdź czy nastąpił awans na wyższy poziom
        if (xp >= xpToNextLevel && currentLevel < maxLevel)
        {
            LevelUp();
        }
        
        UpdateUI(); // Odśwież tekst poziomu i PD
    }
    public void PrintPoolStatus()
    {
        Debug.Log("=== AKTUALNY STAN PULI JEDNOSTEK ===");
        foreach (var entry in unitPool)
        {
            Debug.Log($"{entry.Key.unitName} (Rarity {entry.Key.rarity}): Zostało {entry.Value} sztuk");
        }
    }


        // W ShopManager.cs
    public int MaxTeamSize 
    {
        get 
        {
            int bonus = 0;
            
            // 1. Sprawdź przedmioty na ławce (BenchManager)
            if (BenchManager.Instance != null)
            {
                foreach (var slot in BenchManager.Instance.spawnedSlots)
                {
                    if (slot.currentItem != null && HasTeamSizeEffect(slot.currentItem))
                        bonus++;
                }
            }

            // 2. NOWOŚĆ: Sprawdź jednostki na polu
            // Szukamy wszystkich obiektów typu Unit na scenie
            Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (var u in allUnits)
            {
                // Interesują nas tylko sojusznicy, którzy NIE są przeciwnikami
                if (u.enemyData == null) 
                {
                    // Przeszukujemy listę equippedItems tej jednostki
                    if (u.equippedItems != null)
                    {
                        foreach (var item in u.equippedItems)
                        {
                            if (HasTeamSizeEffect(item))
                            {
                                bonus++;
                                // Debug.Log($"Znalazłem książkę u jednostki: {u.unitName}!");
                            }
                        }
                    }
                }
            }

            return currentLevel + bonus;
        }
    }

    private bool HasTeamSizeEffect(ItemData item)
    {
        // 1. Sprawdzamy czy przedmiot istnieje
        if (item == null) return false;

        // 2. Sprawdzamy czy przedmiot ma przypisany jakikolwiek efekt
        if (item.specialEffect == null) return false;

        // 3. Sprawdzamy, czy ten konkretny efekt to MaxTeamSizeEffect
        // Używamy operatora 'is', który zwraca true, jeśli typ się zgadza
        if (item.specialEffect is MaxTeamSizeEffect) 
        {
            return true;
        }

        return false;
    }
            
}