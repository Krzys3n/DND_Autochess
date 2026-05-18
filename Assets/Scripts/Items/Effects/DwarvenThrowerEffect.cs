using UnityEngine;

[CreateAssetMenu(fileName = "DwarvenThrowerEffect", menuName = "Items/Effects/Dwarven Thrower")]
public class DwarvenThrowerEffect : ItemEffect
{
    public StatusEffectSO InvulnerableEndlessEffect;

    public override void Apply(Unit target)
    {
        // 1. Resetujemy stacki przy założeniu przedmiotu
        target.ResetStackableStats();
        
        Debug.Log($"[Dwarven Thrower] Założono na {target.unitName}. Licznik wyzerowany.");
    }

    public override void OnHit(Unit owner, Unit target)
    {
        AddStack(owner);
    }

    public override void OnOwnerTakeDamage(Unit owner, ref int damage, DamageType type)
    {
        AddStack(owner);
    }

    private void AddStack(Unit owner)
    {
        if (owner == null) return;

        // Korzystamy ze zmiennej dwarvenStacks bezpośrednio z Unit.cs!
        if (owner.dwarvenStacks < 25)
        {
            owner.dwarvenStacks++;
            
            // Zwiększamy procentowe bonusy o 2% (0.02) za każdy stack
            owner.stackADPercent += 0.02f;
            owner.stackASPercent += 0.02f;

            if (owner.dwarvenStacks == 25)
            {
                ApplyMaxStackBonus(owner);
            }

            // KLUCZOWA ZMIANA: Zamiast LoadDataFromSO, wołamy nasz nowy "Kalkulator"!
            // Przeliczy on czystą bazę, nałoży itemki, a na końcu doda nasze nowe stacki.
            owner.RecalculateStats(); 
        }
    }

    private void ApplyMaxStackBonus(Unit owner)
    {
        owner.bonusDefenseFromStacks = 5; 
        owner.bonusMagicDefenseFromStacks = 5;
        owner.LoadDataFromSO();

        if (InvulnerableEndlessEffect != null)
            owner.AddCondition(InvulnerableEndlessEffect);
        
    }
}