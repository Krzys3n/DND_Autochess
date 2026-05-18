using UnityEngine;

[CreateAssetMenu(fileName = "GauntletsOfFlamingFuryEffect", menuName = "Items/Effects/Gauntlets of Flaming Fury")]
public class GauntletsOfFlamingFuryEffect : ItemEffect
{
    [Header("Burst Settings (5 hits)")]
    public int baseDamage = 10;
    public float apScaling = 0.10f;
    public float burstRadius = 2.5f;

    [Header("Aura Settings (Continuous)")]
    public float auraRange = 2.5f; // Zasięg "dwóch pól"
    public StatusEffectSO hexEffect; // Przeciągnij tu SO statusu Hex

    public override void Apply(Unit target) { }

    // --- AURA: NAKŁADANIE HEXA CO TICK ---
    public override void OnTick(Unit owner)
    {
        // Szukamy wszystkich wrogów w zasięgu aury
        Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, auraRange);
        
        foreach (var col in colliders)
        {
            Unit enemy = col.GetComponent<Unit>();
            
            // Jeśli to wróg, nakładamy Hex
            if (enemy != null && enemy.isEnemy != owner.isEnemy)
            {
                UnitStatusManager sm = enemy.GetComponent<UnitStatusManager>();
                if (sm != null && hexEffect != null)
                {
                    // Nakładamy efekt. Czas trwania Hexa w SO powinien być nieco 
                    // dłuższy niż TICK_RATE (np. 1.0s), żeby aura była płynna.
                    sm.ApplyEffect(hexEffect, owner);
                }
            }
        }
    }

    // --- REAKCJA NA OBRAŻENIA (5 hitów) ---
    public override void OnOwnerTakeDamage(Unit owner, ref int damage, DamageType type)
    {
        if (type == DamageType.True) return;

        owner.flamingFuryHitCounter++;
        if (owner.flamingFuryHitCounter >= 5)
        {
            owner.flamingFuryHitCounter = 0;
            TriggerFireBurst(owner);
        }
    }

    private void TriggerFireBurst(Unit owner)
    {
        int finalDamage = baseDamage + Mathf.RoundToInt(owner.abilityPower * apScaling);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, burstRadius);
        
        foreach (var col in colliders)
        {
            Unit enemy = col.GetComponent<Unit>();
            if (enemy != null && enemy.isEnemy != owner.isEnemy)
            {
                enemy.TakeDamage(finalDamage, DamageType.Magic, owner);
            }
        }

        owner.guaranteedNextCrit = true;
    }
}