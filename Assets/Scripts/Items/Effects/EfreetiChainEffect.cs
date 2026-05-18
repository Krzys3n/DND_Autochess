using UnityEngine;

[CreateAssetMenu(fileName = "EfreetiChainEffect", menuName = "Items/Effects/Efreeti Chain")]
public class EfreetiChainEffect : ItemEffect
{
    [Header("Molten Aura Settings")]
    public float auraRadius = 2.5f; // Zasięg (2.5f to zazwyczaj odpowiednik 2 kratek, dostosuj do swojej skali)
    public int tickInterval = 2;    // 2 ticki * 0.5s = 1 sekunda
    
    [Header("Status Definition")]
    [Tooltip("Przeciągnij tutaj obiekt SO reprezentujący status Burned")]
    public StatusEffectSO burnStatus;

    public override void Apply(Unit target) { }

    public override void OnTick(Unit owner)
    {
        // Odpalamy co sekundę (ignorujemy tick 0 na samym starcie walki)
        if (owner.currentItemTick == 0 || owner.currentItemTick % tickInterval != 0)
            return;

        if (burnStatus == null) return;

        // Szukamy wszystkich koliderów w zasięgu
        Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, auraRadius);
        
        foreach (var col in colliders)
        {
            Unit enemy = col.GetComponent<Unit>();
            
            // Warunki: istnieje, żyje i jest WROGIEM
            if (enemy != null && enemy.currentHP > 0 && enemy.isEnemy != owner.isEnemy)
            {
                UnitStatusManager sm = enemy.GetComponent<UnitStatusManager>();
                if (sm != null)
                {
                    // Nakładamy status Burned
                    sm.ApplyEffect(burnStatus, owner);
                }
            }
        }
    }
}