using UnityEngine;

[CreateAssetMenu(fileName = "VampiricShroudEffect", menuName = "Items/Effects/Vampiric Shroud")]
public class VampiricShroudEffect : ItemEffect
{
    [Header("Shield Settings")]
    public float hpThreshold = 0.4f;        // 40% HP
    public float shieldMaxHPPercent = 0.25f; // 25% Max HP
    public float shieldDuration = 5f;



    // Flaga sprawiająca, że tarcza odnawia się co walkę
    private bool wasTriggeredThisRound = false;

    public override void Apply(Unit target)
    {
   
            
        // Resetujemy flagę przy nakładaniu (na start rundy)
        wasTriggeredThisRound = false;
        
        Debug.Log($"[Vampiric Shroud] Nałożono na {target.unitName}. Wampiryzm: {target.vampirism}%");
    }

    public override void OnOwnerTakeDamage(Unit owner, ref int damage, DamageType type)
    {
        // Jeśli tarcza już raz wystrzeliła w tej rundzie, nic nie rób
        if (wasTriggeredThisRound) return;

        // Sprawdzamy stan zdrowia PO uwzględnieniu nadchodzących obrażeń
        // Dzięki temu tarcza aktywuje się "w samą porę", by uratować jednostkę
        float predictedHP = owner.currentHP - damage;
        float hpRatio = predictedHP / owner.maxHP;

        if (hpRatio <= hpThreshold)
        {
            ActivateShroud(owner);
        }
    }

    private void ActivateShroud(Unit owner)
    {
        wasTriggeredThisRound = true;

        int shieldAmount = Mathf.RoundToInt(owner.maxHP * shieldMaxHPPercent);
        owner.AddShield(shieldAmount, shieldDuration);
        
        
        Debug.Log($"[Vampiric Shroud] AKTYWACJA! {owner.unitName} otrzymuje {shieldAmount} tarczy.");
    }

    // Tę metodę powinieneś wywołać w Unit.cs w momencie resetu rundy
    public void ResetEffect()
    {
        wasTriggeredThisRound = false;
    }
}