using UnityEngine;

[CreateAssetMenu(fileName = "MantleOfFaithEffect", menuName = "Items/Effects/Mantle of Faith")]
public class MantleOfFaithEffect : ItemEffect
{
    [Header("Aura Settings")]
    public float auraRange = 2.5f; // Zasięg leczenia (ok. 2 pola)
    public float healPercent = 0.03f; // 3% Max HP
    public int manaRestore = 10; // 10 Many
    public int tickInterval = 6; // 6 ticków * 0.5s = 3 sekundy

    public override void Apply(Unit target) { }

    public override void OnTick(Unit owner)
    {
        // Sprawdzamy, czy to jest odpowiedni moment (co 6. tick).
        // owner.currentItemTick != 0 zapobiega odpaleniu leczenia w pierwszej sekundzie walki.
        if (owner.currentItemTick == 0 || owner.currentItemTick % tickInterval != 0)
            return;

        // Szukamy wszystkich jednostek w zasięgu aury (w tym właściciela, bo on też jest w swoim własnym zasięgu)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, auraRange);
        
        foreach (var col in colliders)
        {
            Unit ally = col.GetComponent<Unit>();
            
            // Warunki: to musi być jednostka, musi żyć i musi być sojusznikiem (lub mną samym)
            if (ally != null && ally.currentHP > 0 && ally.isEnemy == owner.isEnemy)
            {
                // --- 1. LECZENIE (3% Max HP) ---
                int healAmount = Mathf.RoundToInt(ally.maxHP * healPercent);
                if (healAmount > 0)
                {
                    ally.RestoreHP(healAmount);
                }

                // --- 2. ODNOWIENIE MANY ---
                ally.currentMana += manaRestore;
                // Zabezpieczenie przed przekroczeniem limitu many
                if (ally.currentMana > ally.maxMana) 
                {
                    ally.currentMana = ally.maxMana;
                }

                Debug.Log($"[Mantle of Faith] {owner.unitName} leczy {ally.unitName} za {healAmount} HP i przywraca {manaRestore} Many.");
            }
        }
    }
}