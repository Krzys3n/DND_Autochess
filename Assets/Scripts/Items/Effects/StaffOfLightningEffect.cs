using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StaffOfLightningEffect", menuName = "Items/Effects/Staff of Lightning")]
public class StaffOfLightningEffect : ItemEffect
{
    [Header("Damage Settings")]
    public int baseDamage = 10;
    public float apScaling = 0.1f; // 10% AP

    [Header("Chain Settings")]
    public int maxTargets = 4;
    public float jumpRange = 5f;
    public StatusEffectSO hexEffect;

    public override void Apply(Unit target)
    {
        // Kostur nie ma efektu natychmiastowego przy założeniu
    }

    public override void OnHit(Unit owner, Unit target)
    {
        // Używamy licznika zapisanego w Unit.cs
        owner.staffOfLightningHits++;

        if (owner.staffOfLightningHits >= 3)
        {
            owner.staffOfLightningHits = 0;
            TriggerChainLightning(owner, target);
        }
    }

    private void TriggerChainLightning(Unit owner, Unit initialTarget)
    {
        List<Unit> hitTargets = new List<Unit>();
        Unit currentSource = initialTarget;

        // DYNAMICZNE OBLICZANIE OBRAŻEŃ:
        // Pobieramy aktualne AP jednostki (które domyślnie jest 0, ale rośnie z przedmiotami)
        int finalDamage = baseDamage + Mathf.RoundToInt(owner.abilityPower * apScaling);

        for (int i = 0; i < maxTargets; i++)
        {
            if (currentSource == null) break;

            // 1. Zadaj przeliczone obrażenia i nałóż Hex
            ApplyLightningEffect(owner, currentSource, finalDamage);
            hitTargets.Add(currentSource);

            // 2. Szukaj następnego celu
            currentSource = FindNextTarget(currentSource, hitTargets, owner.isEnemy);
        }
    }

    private void ApplyLightningEffect(Unit owner, Unit target, int damageToDeal)
    {
        // Zadajemy obrażenia magiczne (uwzględniające skalowanie AP)
        target.TakeDamage(damageToDeal, DamageType.Magic, owner);

        // Nakładamy Hex (osłabienie odporności magicznej)
        UnitStatusManager sm = target.GetComponent<UnitStatusManager>();
        if (sm != null && hexEffect != null)
        {
            sm.ApplyEffect(hexEffect, owner);
        }
        
        Debug.Log($"[Staff of Lightning] Błyskawica przeskoczyła na {target.unitName} zadając {damageToDeal} DMG.");
    }

    private Unit FindNextTarget(Unit source, List<Unit> alreadyHit, bool ownerIsEnemy)
    {
        // Szukamy wszystkich potencjalnych celów w zasięgu jumpRange
        Collider2D[] colliders = Physics2D.OverlapCircleAll(source.transform.position, jumpRange);
        
        Unit closestNext = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            Unit potentialTarget = col.GetComponent<Unit>();
            
            // Warunki: 
            // 1. To musi być jednostka (Unit).
            // 2. Nie może być z tej samej drużyny co właściciel (ownerIsEnemy).
            // 3. Nie mogła być już trafiona tą samą serią błyskawic (alreadyHit).
            if (potentialTarget != null && 
                potentialTarget.isEnemy != ownerIsEnemy && 
                !alreadyHit.Contains(potentialTarget))
            {
                float dist = Vector2.Distance(source.transform.position, potentialTarget.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestNext = potentialTarget;
                }
            }
        }

        return closestNext;
    }
}