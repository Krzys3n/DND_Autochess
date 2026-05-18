using UnityEngine;

[CreateAssetMenu(fileName = "CloakOfProtectionEffect", menuName = "Items/Effects/Cloak of Protection")]
public class CloakOfProtectionEffect : ItemEffect
{
    public float reductionAmount = 0.1f; // 10%

    public override void Apply(Unit target)
    {
        // Brak natychmiastowego efektu przy założeniu (statystyki robią to same)
    }

    public override void ApplyPassives(Unit target)
    {
        // Dodajemy 10% do ogólnej puli redukcji obrażeń jednostki
        target.damageReduction += reductionAmount;
        
        // Opcjonalnie: Debug.Log($"[Cloak of Protection] Dodano {reductionAmount * 100}% redukcji obrażeń. Suma: {target.damageReduction}");
    }
}