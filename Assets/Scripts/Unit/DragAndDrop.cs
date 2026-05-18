using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour
{
    private bool isDragging = false;
    private Tile startTile;
    private Unit unit;
    private Camera mainCamera;

    void Start()
    {
        unit = GetComponent<Unit>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // BLOKADA: Nie dotykaj wrogów
                if (unit.isEnemy) 
                {
                    Debug.Log("Nie możesz przestawiać jednostek przeciwnika!");
                    return;
                }

                if (GameManager.Instance.currentState == GameState.Setup)
                {
                    StartDragging();
                }
            }
        }

        if (isDragging)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = -2f; 
            transform.position = worldPos;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                StopDragging();
            }
        }
    }

    void StartDragging()
    {
        isDragging = true;
        startTile = unit.currentTile;
        if (startTile != null) startTile.currentUnit = null;
    }

    void StopDragging()
    {
        isDragging = false;
        
        // --- NOWA LOGIKA SPRZEDAŻY ---
        Vector2 mousePos = Mouse.current.position.ReadValue();
        // Jeśli puścimy myszkę w dolnych 20% wysokości ekranu (tam gdzie jest UI sklepu)
        if (mousePos.y < Screen.height * 0.22f) 
        {
            ShopManager.Instance.SellUnit(unit);
            return; // Przerywamy funkcję, jednostka już nie istnieje
        }
        // -----------------------------

        Tile closestTile = FindClosestTile();

        // 1. Jeśli brak pola lub pole WROGA -> Wracamy na start
        if (closestTile == null || !closestTile.IsPlayerTerritory())
        {
            Debug.Log("Nie możesz tam postawić jednostki!");
            PlaceOnTile(startTile);
            return;
        }

        // 2. Jeśli pole zajęte -> ZAMIANA
        if (closestTile.occupied && closestTile.currentUnit != unit)
        {
            Unit otherUnit = closestTile.currentUnit;
            Tile fromTile = startTile;

            otherUnit.currentTile = fromTile;
            fromTile.currentUnit = otherUnit;
            otherUnit.transform.position = new Vector3(fromTile.transform.position.x, fromTile.transform.position.y, -1f);

            PlaceOnTile(closestTile);
        }
        // 3. Jeśli pole puste -> SPRAWDŹ LIMIT
        else
        {
            bool movingFromBenchToArena = startTile.isBenchSlot && !closestTile.isBenchSlot;

            if (movingFromBenchToArena)
            {
                if (BoardManager.Instance.CanPlaceMoreUnits())
                {
                    PlaceOnTile(closestTile);
                }
                else
                {
                    Debug.Log("Limit jednostek na arenie osiągnięty!");
                    PlaceOnTile(startTile);
                }
            }
            else
            {
                PlaceOnTile(closestTile);
            }
        }
        SynergyManager.Instance.RecalculateSynergies();
    }

    void PlaceOnTile(Tile tile)
    {
        if (tile == null) return;
        unit.currentTile = tile;
        tile.currentUnit = unit;
        
        Vector3 newPos = tile.transform.position;
        newPos.z = -1f;
        transform.position = newPos;
    }

    Tile FindClosestTile()
    {
        Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        Tile closest = null;
        float minDist = 0.8f;

        foreach (Tile t in allTiles)
        {
            float dist = Vector2.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = t;
            }
        }
        return closest;
    }
}