using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;
    public List<StageData> stages; // Przeciągnij tu swoje 15 plików SO
    public GameObject unitPrefab; 

    private List<Unit> spawnedEnemies = new List<Unit>();

    void Awake() { Instance = this; }

    public void SpawnStage(int stageNumber)
    {
        HardDestroyEnemies();

        int index = stageNumber - 1;
        if (index < 0 || index >= stages.Count) return;

        StageData stage = stages[index];

        foreach (var info in stage.enemies)
        {
            Vector3 spawnPos = new Vector3(info.gridX, info.gridY, -1f);
            GameObject go = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            
            Unit u = go.GetComponent<Unit>();
            u.isEnemy = true;
            u.enemyData = info.enemyData; 
            
            // KLUCZOWE: Pobranie kafelka i przypisanie jednostki
            Tile targetTile = BoardManager.Instance.GetTileAtPosition(info.gridX, info.gridY);
            if (targetTile != null)
            {
                u.currentTile = targetTile;
                targetTile.currentUnit = u;
            }
            
            u.LoadDataFromSO(); // To ustawi sprite'a i statystyki
            spawnedEnemies.Add(u);
        }
    }

    public void CleanUpEnemies()
    {
        // Zmieniamy logikę: Ta metoda będzie teraz tylko UKRYWAĆ wrogów,
        // aby ich dane o DMG przetrwały fazę Setup.
        foreach (Unit u in spawnedEnemies)
        {
            if (u != null) u.gameObject.SetActive(false); 
        }
        // NIE czyścimy listy spawnedEnemies tutaj!
    }

    // Dodaj nową metodę do RoundManager:
    public void HardDestroyEnemies()
    {
        foreach (Unit u in spawnedEnemies)
        {
            if (u != null) Destroy(u.gameObject);
        }
        spawnedEnemies.Clear();
    }
}