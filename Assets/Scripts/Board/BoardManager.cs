using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public GameObject tilePrefab;
    public GameObject unitPrefab; // Twój Master Prefab

    [Header("Bench Settings")]
    public int benchSize = 8;
    public float benchYOffset = 1.5f;
    public Color benchColor = Color.black;
    public List<Tile> benchTiles = new List<Tile>();

    private Tile[,] tiles;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateBoard();
        CreateBench(benchSize);
    }

    // --- LOGIKA SKLEPU I SPAWNOWANIA ---

    public bool HasFreeBenchSlot()
    {
        foreach (Tile tile in benchTiles)
        {
            if (tile.currentUnit == null) return true;
        }
        return false;
    }

    // To jest główna metoda, której szuka ShopManager
    public void SpawnUnit(UnitData data, int tier)
    {
        Tile freeTile = GetFirstFreeBenchTile();
        if (freeTile != null)
        {
            Vector3 pos = freeTile.transform.position;
            pos.z = -1f;

            GameObject newUnitObj = Instantiate(unitPrefab, pos, Quaternion.identity);
            Unit unitScript = newUnitObj.GetComponent<Unit>();

            if (unitScript != null)
            {
                unitScript.unitData = data;
                unitScript.unitTier = tier; 
                unitScript.isEnemy = false;
                
                unitScript.currentTile = freeTile;
                freeTile.currentUnit = unitScript;

                // Jeśli masz metodę odświeżającą statystyki w Unit.cs, odkomentuj to:
                // unitScript.LoadDataFromSO(); 
            }
            
            SynergyManager.Instance.RecalculateSynergies();
        }
    }

    private Tile GetFirstFreeBenchTile()
    {
        foreach (Tile tile in benchTiles)
        {
            if (tile.currentUnit == null) return tile;
        }
        return null;
    }

    // --- LOGIKA ARENY I LIMITÓW ---

    public int GetCurrentUnitsOnArenaCount()
    {
        int count = 0;
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit u in allUnits)
        {
            // Liczymy tylko sojuszników, którzy mają kafelek i NIE są na ławce
            if (!u.isEnemy && u.currentTile != null && !u.currentTile.isBenchSlot)
            {
                count++;
            }
        }
        return count;
    }

    public bool CanPlaceMoreUnits()
    {
        return GetCurrentUnitsOnArenaCount() < ShopManager.Instance.MaxTeamSize;
    }

    // --- GENEROWANIE PLANSZY ---

    void GenerateBoard()
    {
        tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                Tile tileScript = tile.GetComponent<Tile>();
                tileScript.gridX = x; 
                tileScript.gridY = y;
                tileScript.isBenchSlot = false;
                tiles[x, y] = tileScript;

                // Wizualizacja połowy gracza (do y=3 włącznie)
                if (y < 4) 
                {
                    tile.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 1f);
                }
            }
        }
    }

    public void CreateBench(int size)
    {
        for (int i = 0; i < size; i++)
        {
            Vector3 pos = new Vector3(i * 1.1f, -benchYOffset, 0);
            GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
            tile.GetComponent<SpriteRenderer>().color = benchColor;

            Tile tileScript = tile.GetComponent<Tile>();
            tileScript.isBenchSlot = true;
            benchTiles.Add(tileScript);
        }
    }

    // --- AUTOMATYKA (AUTOFULL) ---

    public void AutoFillArena()
    {
        int currentUnits = GetCurrentUnitsOnArenaCount();
        int maxUnits = ShopManager.Instance.MaxTeamSize;
        int spaceLeft = maxUnits - currentUnits;

        if (spaceLeft <= 0) return;

        foreach (Tile benchTile in benchTiles)
        {
            if (spaceLeft <= 0) break;

            if (benchTile.currentUnit != null)
            {
                Unit unitToMove = benchTile.currentUnit;
                Tile emptyArenaTile = FindFirstEmptyArenaTile();

                if (emptyArenaTile != null)
                {
                    MoveUnitToTile(unitToMove, benchTile, emptyArenaTile);
                    spaceLeft--;
                }
            }
        }
        SynergyManager.Instance.RecalculateSynergies();
    }

    private Tile FindFirstEmptyArenaTile()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tiles[x, y].currentUnit == null) return tiles[x, y];
            }
        }
        return null;
    }

    private void MoveUnitToTile(Unit unit, Tile fromTile, Tile toTile)
    {
        fromTile.currentUnit = null;
        unit.currentTile = toTile;
        toTile.currentUnit = unit;
        
        Vector3 newPos = toTile.transform.position;
        newPos.z = -1f;
        unit.transform.position = newPos;
    }
    public void TileClicked(Tile tile)
    {
        // Na razie zostawiamy puste, żeby nie było błędów.
        // Tutaj możesz w przyszłości dodać logikę np. zaznaczania jednostki.
        Debug.Log($"Kliknięto kafelek: {tile.gridX}, {tile.gridY}");
    }
    public List<Unit> GetAllPlayerUnits()
    {
        List<Unit> playerUnits = new List<Unit>();
        // Pobieramy wszystkie jednostki na scenie
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        
        foreach (Unit u in allUnits)
        {
            if (u != null && !u.isEnemy)
            {
                playerUnits.Add(u);
            }
        }
        return playerUnits;
    }

    // Dodaj tę metodę do klasy BoardManager
    public Tile GetTileAtPosition(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return tiles[x, y];
        }
        return null;
    }
}