using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    private float checkTimer = 0.5f; // Sprawdzamy co pół sekundy, żeby nie obciążać procesora

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Sprawdzamy tylko, gdy trwa walka
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Combat)
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0)
            {
                checkTimer = 0.5f;
                CheckBattleState();
            }
        }
    }

    void CheckBattleState()
    {
        // Szukamy wszystkich jednostek na scenie
        Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        
        bool enemyAlive = false;
        bool playerAlive = false;

        foreach (Unit u in units)
        {
            // NOWOŚĆ: Jeśli jednostka stoi na ławce, ignorujemy ją w obliczeniach końca walki
            if (u.currentTile != null && u.currentTile.isBenchSlot) continue;

            if (u.isEnemy) enemyAlive = true;
            else playerAlive = true;
        }

        // Warunki zakończenia sprawdzają teraz tylko jednostki na arenie
        if (!enemyAlive && playerAlive)
        {
            Debug.Log("ZWYCIĘSTWO! Arena oczyszczona.");
            GameManager.Instance.EndBattle(true);
        }
        else if (!playerAlive && enemyAlive)
        {
            Debug.Log("PORAŻKA! Twoje jednostki na arenie wyginęły.");
            GameManager.Instance.EndBattle(false);
        }
    }
}