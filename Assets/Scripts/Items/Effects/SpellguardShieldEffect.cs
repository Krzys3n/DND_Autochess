using UnityEngine;

[CreateAssetMenu(fileName = "SpellguardShieldEffect", menuName = "Items/Effects/Spellguard Shield")]
public class SpellguardShieldEffect : ItemEffect
{
    public override void Apply(Unit target) { }

    // Wywołuje się tylko raz, na starcie walki
    public override void OnCombatStart(Unit owner)
    {
        owner.hasSpellShield = true;
        Debug.Log($"[Spellguard Shield] {owner.unitName} otacza się barierą antymagiczną.");
    }
}