using UnityEngine;

[CreateAssetMenu(fileName = "SunBladeEffect", menuName = "Items/Effects/Sun Blade")]
public class SunBladeEffect : ItemEffect
{
    public StatusEffectSO burnEffect;    // Przeciągnij tu plik Burn
    public StatusEffectSO woundEffect;
    public override void Apply(Unit target)
    {
        // Statystyki AD% i AP są ładowane automatycznie z ItemData, 
        // tutaj nie musimy nic dopisywać, chyba że chcemy unikalny bonus.
        Debug.Log($"[Sun Blade] {target.unitName} equipped the celestial blade.");
    }

    public override void OnHit(Unit owner, Unit target)
    {
        if (target == null) return;

        // Przekazujemy 'owner' jako drugi parametr (źródło efektu)
        if (burnEffect != null) target.AddCondition(burnEffect, owner);
        if (woundEffect != null) target.AddCondition(woundEffect, owner);
    }
}