using UnityEngine;

[CreateAssetMenu(fileName = "NewShieldAbility", menuName = "DND_Chess/Abilities/Shield")]
public class ShieldAbility : Ability
{
    [Header("Shield Scaling")]
    public float apScaling = 0.5f; // Mniejsze skalowanie (tylko dla bonusu z AP!)
    public float duration = 4f;
    public bool targetSelf = true;

    public override void Cast(Unit caster, Unit target)
    {
        Unit finalTarget = targetSelf ? caster : target;

        if (finalTarget != null)
        {
            // 1. Święta Baza z ewolucji
            int baseShield = caster.baseAbilityPower;

            // 2. Obliczamy samą wartość z AP (ile procent bonusu dostajemy)
            float apBonus = Mathf.Max(0, caster.abilityPower) / 100f;

            // 3. WZÓR: 100% bazy (1f) + (bonus z AP przemnożony przez skalowanie tarczy)
            float finalMultiplier = 1f + (apBonus * apScaling);

            // 4. Wynik
            int totalShieldAmount = Mathf.RoundToInt(baseShield * finalMultiplier);

            finalTarget.AddShield(totalShieldAmount, duration);
            Debug.Log($"{caster.unitName} nakłada {totalShieldAmount} tarczy na {finalTarget.unitName} na {duration}s!");
        }
    }
}