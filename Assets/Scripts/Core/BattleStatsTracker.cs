using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TrackerMode { Damage, Tanking, Healing }

[System.Serializable]
public class UnitDataLog 
{
    public int id;
    public string name;
    public float physDmg, magDmg, trueDmg, totalDmg;
    public float physTanked, magTanked, totalTanked;
    public float totalHealing, totalShielding, totalSupport;
    public float shieldingGenerated; // NOWE: Rzucone tarcze
    public float shieldingAbsorbed;  // NOWE: Zużyte tarcze
}

public class BattleStatsTracker : MonoBehaviour
{
    public GameObject unitRowPrefab;
    public Transform rowsContainer;
    public GameObject contentObject;
    public GameObject tabsHeader; 
    
    public TrackerMode currentMode = TrackerMode.Damage;
    private bool isExpanded = false;
    private float refreshTimer = 0f;
    public float refreshRate = 0.5f; 

    private List<Unit> unitsInCurrentBattle = new List<Unit>();
    private List<UnitDataLog> lastBattleLog = new List<UnitDataLog>();

    void Start()
    {
        // Upewniamy się, że na starcie wszystko jest ukryte, niezależnie od ustawień w edytorze
        isExpanded = false;
        if (contentObject != null) contentObject.SetActive(false);
        if (tabsHeader != null) tabsHeader.SetActive(false);
    }

    void Update()
    {
        if (isExpanded && GameManager.Instance != null && GameManager.Instance.currentState == GameState.Combat)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshRate)
            {
                RefreshStats();
                refreshTimer = 0f;
            }
        }
    }

    // Metody dla przycisków na górze panelu
    public void SetModeDamage() { currentMode = TrackerMode.Damage; RefreshStats(); }
    public void SetModeTanking() { currentMode = TrackerMode.Tanking; RefreshStats(); }
    public void SetModeHealing() { currentMode = TrackerMode.Healing; RefreshStats(); }

    public void RegisterUnitsForBattle()
    {
        unitsInCurrentBattle.Clear();
        lastBattleLog.Clear(); 

        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit u in allUnits)
        {
            if (u.isEnemy || (u.currentTile != null && !u.currentTile.isBenchSlot))
            {
                unitsInCurrentBattle.Add(u);
                lastBattleLog.Add(new UnitDataLog {
                    id = u.GetInstanceID(),
                    name = string.IsNullOrEmpty(u.unitName) ? (u.isEnemy ? "Przeciwnik" : "Bohater") : u.unitName
                });
            }
        }
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (!isExpanded) return;
        foreach (Transform child in rowsContainer) Destroy(child.gameObject);
        if (GameManager.Instance.currentState == GameState.Combat) UpdateLogDataFromLiveUnits();
        
        if (lastBattleLog.Count == 0) return;

        List<UnitDataLog> sortedLog = new List<UnitDataLog>();
        float maxVal = 0;

        switch (currentMode)
        {
            case TrackerMode.Damage:
                sortedLog = lastBattleLog.OrderByDescending(l => l.totalDmg).ToList();
                maxVal = lastBattleLog.Any() ? lastBattleLog.Max(l => l.totalDmg) : 0;
                break;
            case TrackerMode.Tanking:
                sortedLog = lastBattleLog.OrderByDescending(l => l.totalTanked).ToList();
                maxVal = lastBattleLog.Any() ? lastBattleLog.Max(l => l.totalTanked) : 0;
                break;
            case TrackerMode.Healing: 
                // Sortujemy po totalSupport (Heal + Absorbed), bo to jest realna wartość jednostki
                sortedLog = lastBattleLog.OrderByDescending(l => l.totalSupport).ToList();
                // MaxVal musi brać pod uwagę sumę wszystkich 3 pasków, żeby się nie rozjechały
                maxVal = lastBattleLog.Any() ? lastBattleLog.Max(l => Mathf.Max(l.totalHealing, l.shieldingGenerated, l.shieldingAbsorbed)) : 0;
                break;
        }

        foreach (var log in sortedLog)
        {
            GameObject rowObj = Instantiate(unitRowPrefab, rowsContainer);
            BattleStatsTrackerUI rowUI = rowObj.GetComponent<BattleStatsTrackerUI>();

            if (currentMode == TrackerMode.Damage)
                rowUI.SetupUniversal(log.name, log.physDmg, log.magDmg, log.trueDmg, maxVal, currentMode);
            else if (currentMode == TrackerMode.Tanking)
                rowUI.SetupUniversal(log.name, log.physTanked, log.magTanked, 0, maxVal, currentMode);
            else // TRYB SUPPORT
                // v1 = Healing (Zielony)
                // v2 = Generated Shields (Jasnoniebieski/Biały - potencjał)
                // v3 = Absorbed Shields (Ciemnoniebieski - realna pomoc)
                rowUI.SetupUniversal(log.name, log.totalHealing, log.shieldingGenerated, log.shieldingAbsorbed, maxVal, currentMode);
        }
    }

    private void UpdateLogDataFromLiveUnits()
    {
        foreach (Unit u in unitsInCurrentBattle)
        {
            if (u == null) continue; 
            var log = lastBattleLog.FirstOrDefault(l => l.id == u.GetInstanceID());
            if (log != null)
            {
                // DMG: Phys + Mag + True
                log.physDmg = u.roundPhysDamageDealt;
                log.magDmg = u.roundMagicDamageDealt;
                log.trueDmg = u.roundTrueDamageDealt; // NOWE
                log.totalDmg = log.physDmg + log.magDmg + log.trueDmg;

                // TANKOWANIE: Redukcja z Armora + MR
                log.physTanked = u.roundPhysDamageTanked;
                log.magTanked = u.roundMagicDamageTanked;
                log.totalTanked = log.physTanked + log.magTanked;

                // SUPPORT: Leczenie + Tarcze
                log.totalHealing = u.roundHealingDone;
                log.shieldingGenerated = u.roundShieldingGenerated; //
                log.shieldingAbsorbed = u.roundShieldingAbsorbed;   //
                log.totalSupport = log.totalHealing + log.shieldingAbsorbed;
            }
        }
    }

    public void CreateFinalSnapshot() { UpdateLogDataFromLiveUnits(); }

    public void TogglePanel()
    {
        isExpanded = !isExpanded;

        // 1. Pokazujemy/ukrywamy kontener z wierszami (listę)
        if (contentObject != null) 
        {
            contentObject.SetActive(isExpanded);
        }

        // 2. Pokazujemy/ukrywamy pasek z przyciskami trybów (zakładki)
        if (tabsHeader != null) 
        {
            tabsHeader.SetActive(isExpanded);
        }

        // 3. Jeśli panel został otwarty, odświeżamy dane
        if (isExpanded) 
        {
            RefreshStats();
            
            // Wymuszamy aktualizację UI, aby paski i układ (Layout) od razu się przeliczyły
            Canvas.ForceUpdateCanvases();
        }
    }
}