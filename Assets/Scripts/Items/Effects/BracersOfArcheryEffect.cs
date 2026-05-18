using UnityEngine;

[CreateAssetMenu(fileName = "BracersOfArcheryEffect", menuName = "Items/Effects/Bracers Of Archery")]
public class BracersOfArcheryEffect : ItemEffect
{
    [Header("Status Effect")]
    [Tooltip("Przeciągnij tutaj plik SO efektu Corrosion z folderu Resources.")]
    public StatusEffectSO corrosionEffect;

    public override void Apply(Unit target)
    {
        // Odpala się raz, przy założeniu. Dodajemy trochę lore dla klimatu w logach!
        Debug.Log($"[Bracers of Archery] {target.unitName} zakłada karwasze Wysokich Elfów. Strzały będą teraz przebijać najgrubszy pancerz!");
    }

    // Ta metoda wywoła się automatycznie przy każdym podstawowym ataku (lub zaklęciu, jeśli masz to podpięte pod OnHit)
    public override void OnHit(Unit owner, Unit target)
    {
        if (target == null || corrosionEffect == null) return;

        // Szukamy Status Managera na celu i nakładamy efekt
        UnitStatusManager targetStatusManager = target.GetComponent<UnitStatusManager>();
        
        if (targetStatusManager != null)
        {
            // Przekazujemy 'owner' jako castera, żeby gra wiedziała, kto nałożył korozję!
            targetStatusManager.ApplyEffect(corrosionEffect, owner);
        }
    }
}