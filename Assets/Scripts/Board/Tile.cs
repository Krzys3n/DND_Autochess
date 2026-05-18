using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerDownHandler
{
    [Header("Coordinates")]
    public int gridX;
    public int gridY;

    [Header("Status")]
    public bool isBenchSlot = false; // Czy to slot na ławce?
    public Unit currentUnit;        // Jednostka stojąca na tym polu

    // Właściwość 'occupied' - zwraca true, jeśli na polu ktoś stoi
    // Dzięki temu w BoardManagerze możemy pisać: if (tile.occupied)
    public bool occupied => currentUnit != null;

    // Funkcja wywoływana przez EventSystem przy kliknięciu (wymaga Physics2DRaycaster na Kamerze)
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Kliknięto {(isBenchSlot ? "Ławkę" : "Pole")} [{gridX},{gridY}]");
        
        // Informujemy BoardManager o kliknięciu, jeśli masz tam jakąś logikę wyboru
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.TileClicked(this);
        }
    }

    /// <summary>
    /// Sprawdza, czy pole należy do terytorium gracza (ławka lub dolna połowa areny).
    /// </summary>
    public bool IsPlayerTerritory()
    {
        if (isBenchSlot) return true;
        return gridY < 4; // Zakładamy, że rzędy 0, 1, 2, 3 to strona gracza
    }
}