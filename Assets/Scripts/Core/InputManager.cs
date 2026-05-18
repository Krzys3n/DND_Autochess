using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Skróty działają tylko w fazie Setup (Przygotowania)
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Setup) 
            return;

        // --- D: REFRESH SKLEPU ---
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            ShopManager.Instance.ManualRefresh(); 
        }

        // --- F: KUPNO XP ---
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ShopManager.Instance.BuyXP();
        }

        // --- E: SPRZEDAŻ JEDNOSTKI POD MYSZKĄ ---
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TrySellUnitUnderMouse();
        }

                // Wewnątrz Update() w InputManager.cs
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            ShopManager.Instance.PrintPoolStatus();
        }

                // Kliknij I, aby zasymulować wypadnięcie przedmiotu
        if (Keyboard.current.iKey.wasPressedThisFrame) 
        {
            // Twoja logika dodawania przedmiotu
            ItemData testItem = Resources.Load<ItemData>("Items/TestSword"); 
            //ItemBench.Instance.AddItemToBench(testItem);
        }
    }

    void TrySellUnitUnderMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        
        // Raycast sprawdza, czy pod myszką jest Collider jednostki
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Unit unitToSell = hit.collider.GetComponent<Unit>();
            
            // Sprzedajemy tylko jeśli to jednostka gracza (nie wróg)
            if (unitToSell != null && !unitToSell.isEnemy)
            {
                ShopManager.Instance.SellUnit(unitToSell);
            }
        }
    }
}