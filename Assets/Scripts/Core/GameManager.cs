using UnityEngine;
using System.Collections.Generic;

public enum GameState { Setup, Combat, Results }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState currentState = GameState.Setup;
    public int currentStage = 1;
    [HideInInspector] public int sessionSeed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        sessionSeed = Random.Range(0, 999999);
    }

    void Start()
    {
        // Spawnowanie przeciwników dla pierwszego etapu zaraz po uruchomieniu gry
        SpawnEnemiesForCurrentStage();
    }

    // Wywołaj tę funkcję przyciskiem "Start Combat" w UI
   public void StartBattle()
    {
        if (currentState == GameState.Setup)
        {
            BattleStatsTracker tracker = Object.FindFirstObjectByType<BattleStatsTracker>();
            
            // 1. Najpierw znajdujemy wszystkie jednostki obecne na scenie
            Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

            foreach (Unit u in allUnits)
            {
                u.ResetRoundStats(); 
                u.RecalculateStats();
                if (!u.isEnemy) u.SaveBattleState(); 
            }

            // 2. Autofill - jednostki wskakują na swoje miejsca na arenie
            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.AutoFillArena();
            }

            // 3. DOPIERO TERAZ: Efekty startowe (np. Kołczan), bo jednostki już stoją na miejscach
            foreach (Unit unit in allUnits)
            {
                if (unit.equippedItems == null) continue;

                // TWORZYMY KOPIĘ LISTY PRZEDMIOTÓW DO ITERACJI
                // Dzięki temu Rękawice Złodzieja mogą bezpiecznie dorzucać itemy do oryginalnej listy!
                List<ItemData> itemsToTrigger = new List<ItemData>(unit.equippedItems);

                foreach (ItemData item in itemsToTrigger)
                {
                    if (item != null && item.specialEffect != null)
                    {
                        item.specialEffect.OnCombatStart(unit);
                    }
                }
            }

            // 4. Rejestracja w trackerze
            if (tracker != null) 
            {
                tracker.RegisterUnitsForBattle();
            }

            currentState = GameState.Combat;
        }
    }

    public void EndBattle(bool playerWin)
    {
        // 1. Znajdujemy tracker raz na początku metody
        BattleStatsTracker tracker = Object.FindFirstObjectByType<BattleStatsTracker>();

        // 2. ZAPISUJEMY DANE (Snapshot) zanim cokolwiek zostanie zniszczone
        if (tracker != null)
        {
            tracker.CreateFinalSnapshot();
        }

        if (playerWin)
        {
            currentStage++;
            ResetPlayerUnits();
            currentState = GameState.Setup;

            // 3. Czyścimy wrogów dopiero GDY mamy już snapshot danych
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.CleanUpEnemies();
            }
            
            
            SpawnEnemiesForCurrentStage();
        }
        else
        {
            currentState = GameState.Results;
            Debug.Log("GAME OVER!");
        }
        
        // 4. Odświeżamy UI sklepu i trackera (teraz tracker użyje snapshota)
        if (ShopManager.Instance != null) ShopManager.Instance.UpdateUI();
        
        if (tracker != null)
        {
            tracker.RefreshStats();
        }
    }

    private void SpawnEnemiesForCurrentStage()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.SpawnStage(currentStage);
            Debug.Log($"Przeciwnicy dla etapu {currentStage} pojawili się na arenie.");
        }
    }

    private void ResetPlayerUnits()
    {
        Unit[] allUnits = Resources.FindObjectsOfTypeAll<Unit>(); 
        foreach (Unit u in allUnits)
        {
            if (!u.isEnemy && u.gameObject.scene.isLoaded)
            {
                u.gameObject.SetActive(true);
                
                // TO JEST KLUCZ: ResetAfterBattle wewnątrz Unit.cs robi wszystko:
                // teleportuje, czyści stare itemy i wywołuje nowy roll (przez RecalculateStats)
                u.ResetAfterBattle(); 
            }
        }
    }
}