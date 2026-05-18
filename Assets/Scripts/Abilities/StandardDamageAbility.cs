using UnityEngine;

[CreateAssetMenu(fileName = "StandardDamageAbility", menuName = "Abilities/Standard Damage")]
public class StandardDamageAbility : Ability
{
    [Header("Ability Scaling")]
    public DamageType damageType = DamageType.Magic;
    public float apScaling = 1.0f; // np. 1.0 = 100% AP

    [Header("Optional Status")]
    public StatusEffectSO statusToApply; // Zostaw puste, jeśli ma tylko bić

    public override void Cast(Unit caster, Unit target)
    {
        if (target == null) return;

        int finalDamage = caster.CalculateFinalAbilityDamage(apScaling, target);

        // 3. Zadajemy obrażenia
        target.TakeDamage(finalDamage, damageType, caster);
        caster.TriggerOnHitEffects(target); // Opcjonalne, jeśli skille mają nakładać on-hity

        // 4. Nakładanie opcjonalnego statusu (np. Stun, Podpalenie)
        if (statusToApply != null)
        {
            UnitStatusManager sm = target.GetComponent<UnitStatusManager>();
            if (sm != null) sm.ApplyEffect(statusToApply, caster);
        }

        Debug.Log($"[{abilityName}] trafia {target.unitName} za {finalDamage}!");
    }
}