using UnityEngine;

[CreateAssetMenu(fileName = "RobeOfTheArchmagiEffect", menuName = "Items/Effects/Robe of the Archmagi")]
public class RobeOfTheArchmagiEffect : ItemEffect
{
    [Header("Arcane Mastery Settings")]
    public float apGainPerCast = 0.1f; 
    public float shieldManaPercent = 0.2f; // 20%
    public float shieldDuration = 3f;

    public override void Apply(Unit target) { }

    public override void OnAbilityCast(Unit owner)
    {
        // 1. Zwiększamy ilość stosów Arcymaga
        owner.archmagiStacks++;

        // 2. Przeliczamy statystyki (AP rośnie)
        owner.RecalculateStats();

        // 3. Obliczamy tarczę z MAXIMUM MANA zamiast HP
        int shieldAmount = Mathf.RoundToInt(owner.maxMana * shieldManaPercent);
        
        // 4. Odświeżamy tarczę (zamiast ją stackować!)
        owner.RefreshArchmagiShield(shieldAmount, shieldDuration);

        Debug.Log($"[Robe of the Archmagi] {owner.unitName} zyskuje/odświeża {shieldAmount} tarczy z Many. AP: {owner.abilityPower}");
    }
}