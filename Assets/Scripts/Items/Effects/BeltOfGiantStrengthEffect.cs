using UnityEngine;

[CreateAssetMenu(fileName = "BeltOfGiantStrengthEffect", menuName = "Items/Effects/Belt Of Giant Strength")]
public class BeltOfGiantStrengthEffect : ItemEffect
{
    // Używamy HashSet, aby pamiętać, które jednostki już zużyły tarczę w tej walce
    private System.Collections.Generic.HashSet<Unit> usedInCombat = new System.Collections.Generic.HashSet<Unit>();

    public override void Apply(Unit target)
    {
        // AD% i HP są ładowane z bazy w ItemData, 
        // tutaj możemy zresetować stan użycia na początku nowej rundy/walki
        usedInCombat.Remove(target);
        Debug.Log($"[Belt of Giant Strength] Gotowy do aktywacji dla {target.unitName}");
    }

    public override void OnOwnerTakeDamage(Unit owner, ref int damage, DamageType type)
    {
        if (owner == null) return;

        // Sprawdzamy czy postać ma poniżej 60% HP i czy nie użyła już tarczy
        float healthPercent = (float)owner.currentHP / owner.maxHP;

        if (healthPercent < 0.6f && !usedInCombat.Contains(owner))
        {
            // Obliczamy 50% Max HP
            int shieldAmount = Mathf.RoundToInt(owner.maxHP * 0.50f);

            // Używamy Twojej wbudowanej metody z Unit.cs!
            owner.AddShield(shieldAmount, 4f);

            // Zapisujemy, że ta jednostka już wykorzystała efekt w tej walce
            usedInCombat.Add(owner);

            Debug.Log($"<color=green>[BELT PROC!]</color> {owner.unitName} spada poniżej 60% HP! Przyznano tarczę: {shieldAmount} na 4 sekundy.");
        }
    }
}