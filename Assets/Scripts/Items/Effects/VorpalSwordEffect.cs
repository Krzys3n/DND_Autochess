using UnityEngine;

[CreateAssetMenu(fileName = "VorpalSwordEffect", menuName = "Items/Effects/Vorpal Sword")]
public class VorpalSwordEffect : ItemEffect
{
    public override void Apply(Unit target)
    {
            
        Debug.Log("Założono Vorpal Sword");
    }

    public override void ApplyPassives(Unit target)
    {
        // Jeśli jednostka już MOŻE krytować umiejętnościami (np. ma drugi Vorpal Sword)
        if (target.canAbilitiesCrit)
        {
            // Dodajemy 10% do mnożnika Crit Damage
            // Jeśli bazowy to 1.4f, stanie się 1.5f
            target.critDamage += 0.10f;
            
        }
        else
        {
            // Pierwszy raz nakładamy efekt
            target.canAbilitiesCrit = true;
            Debug.Log($"[Vorpal Sword] {target.unitName} can now Crit with Abilities!");
        }
    }
}