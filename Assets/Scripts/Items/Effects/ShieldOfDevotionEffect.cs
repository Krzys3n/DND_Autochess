using UnityEngine;

[CreateAssetMenu(fileName = "ShieldOfDevotionEffect", menuName = "Items/Effects/Shield of Devotion")]
public class ShieldOfDevotionEffect : ItemEffect
{
    [Header("Shield Settings")]
    public float maxManaPercent = 0.5f; // 50%
    public float duration = 5f;         // 5 sekund

    public override void Apply(Unit target) { }

    // Ta metoda odpali się TYLKO, gdy wywołasz ją w Unit.cs podczas castowania
    public override void OnAbilityCast(Unit owner)
    {
        // 1. Obliczamy wartość tarczy (50% z Max Many)
        int shieldAmount = Mathf.RoundToInt(owner.maxMana * maxManaPercent);

        // 2. Nakładamy tarczę
        // UWAGA: Użyj nazwy Twojej funkcji z Unit.cs dodającej tarczę (np. AddShield)
        owner.AddShield(shieldAmount, duration);

        Debug.Log($"[Shield of Devotion] {owner.unitName} zyskuje {shieldAmount} tarczy na {duration}s po użyciu umiejętności!");
    }
}