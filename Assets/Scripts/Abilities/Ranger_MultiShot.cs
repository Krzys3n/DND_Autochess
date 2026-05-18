using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "RangerMultiShot", menuName = "Abilities/Ranger MultiShot")]
public class RangerMultiShot : Ability
{
    [Header("Ability Scaling")]
    public float apScaling = 1f; // Np. 50% AP jako dodatkowe obrażenia fizyczne

    public override void Cast(Unit caster, Unit target)
    {
        List<Unit> targets = GetNearestEnemies(caster, 2);

        if (targets.Count == 0) return;

        // Pierwsza strzała 
        ShootArrow(caster, targets[0]);

        // Druga strzała
        if (targets.Count > 1)
        {
            ShootArrow(caster, targets[1]);
        }
        else
        {
            ShootArrow(caster, targets[0]);
        }

        Debug.Log($"{caster.unitName} używa {abilityName} i wystrzeliwuje dwie strzały!");
    }

    private void ShootArrow(Unit caster, Unit target)
    {
        if (target != null)
        {
            // KLUCZOWE: Sprawdzamy tarczę dodatkowych celów (bo Execute mogło jej nie złapać, jeśli to nie był główny target)
            // Używamy .hasSpellShield żeby nie wywoływać Consume niepotrzebnie
            if (target.hasSpellShield && target.ConsumeSpellShield())
            {
                Debug.Log($"Strzała zablokowana przez Spellguard Shield jednostki {target.unitName}!");
                return;
            }

            int finalDamage = caster.CalculateFinalAbilityDamage(apScaling, target);

            // 5. Zadajemy obrażenia fizyczne i odpalamy on-hity (jak u Rangera)
            target.TakeDamage(finalDamage, DamageType.True, caster);
            caster.TriggerOnHitEffects(target);
            
            Debug.Log($"Strzała trafia {target.unitName} za {finalDamage} pkt!");
        }
    }

    private List<Unit> GetNearestEnemies(Unit caster, int count)
    {
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        
        return allUnits
            .Where(u => u != null && u.gameObject.activeInHierarchy && u.isEnemy != caster.isEnemy)
            .Where(u => u.currentTile != null && !u.currentTile.isBenchSlot)
            .OrderBy(u => Vector2.Distance(caster.transform.position, u.transform.position))
            .Take(count)
            .ToList();
    }
}