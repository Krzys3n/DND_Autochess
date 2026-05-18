using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SunforgedAegisEffect", menuName = "Items/Effects//Sunforged Aegis")]
public class SunforgedAegisEffect : ItemEffect
{
    [Header("Status References")]
    public StatusEffectSO immortalSO;
    public StatusEffectSO invulnerableSO;

    private bool wasUsedThisCombat = false;

    public override void Apply(Unit target) { }

    public override void OnOwnerTakeDamage(Unit owner, ref int damage, DamageType type)
    {
        if (wasUsedThisCombat) return;

        // Sprawdzamy czy cios jest śmiertelny
        if (owner.currentHP - damage <= 0)
        {
            wasUsedThisCombat = true;

            // 1. Zostawiamy 1 HP
            damage = owner.currentHP - 1;

            // 2. Odpalamy logikę ratunkową
            owner.StartCoroutine(ActivateAegis(owner));
        }
    }

    private IEnumerator ActivateAegis(Unit owner)
    {
        UnitStatusManager sm = owner.GetComponent<UnitStatusManager>();
        
        if (sm != null)
        {
            // A. Czyścimy negatywne statusy
            sm.ClearAllEffects(); 

            // B. Nakładamy statusy przez Managera (on zadba o ikonki i czas)
            if (immortalSO != null) sm.ApplyEffect(immortalSO);
            if (invulnerableSO != null) sm.ApplyEffect(invulnerableSO);
        }

        // C. Wizualizacja (Złoty kolor)
        owner.SetVisualColor(new Color(1f, 0.84f, 0f));

        // Czekamy tyle, ile trwają statusy (pobieramy czas z SO)
        float duration = immortalSO != null ? immortalSO.duration : 3f;
        yield return new WaitForSeconds(duration);

        // D. Powrót do normalnego koloru
        owner.SetVisualColor(Color.white);
    }

    // Reset na początku walki (pamiętaj, by wywołać to w GameManagerze przy starcie rundy!)
    public void ResetEffect() => wasUsedThisCombat = false;
}