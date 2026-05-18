using UnityEngine;

[CreateAssetMenu(fileName = "DragonSlayerEffect", menuName = "Items/Effects/Dragon Slayer")]
public class DragonSlayerEffect : ItemEffect
{
    [Header("Damage Settings")]
    public float basePercent = 3f;      // 3% dla zwykłych celów
    public float dragonPercent = 5f;    // 5% dla smoków

    public override void Apply(Unit owner)
    {
        // Tu można by dodać np. efekty wizualne przy założeniu przedmiotu
    }

    public override void OnHit(Unit owner, Unit target)
    {
        if (target == null) return;

        // 1. Sprawdzamy rasę (pobieramy z UnitData lub EnemyData)
        string targetRace = "";

        if (target.unitData != null) targetRace = target.unitData.race;
        else if (target.enemyData != null) targetRace = target.enemyData.race;

        // 2. Jeśli rasa jest pusta, na pewno nie jest smokiem
        bool isDragon = !string.IsNullOrEmpty(targetRace) && 
                        targetRace.Equals("Dragon", System.StringComparison.OrdinalIgnoreCase);

        // 3. Wybieramy odpowiedni procent
        float currentPercent = isDragon ? dragonPercent : basePercent;

        // 4. Obliczamy obrażenia (z Max HP celu)
        int bonusDamage = Mathf.RoundToInt(target.maxHP * (currentPercent / 100f));

        if (bonusDamage > 0)
        {
            // Dragon Slayer zazwyczaj zadaje obrażenia fizyczne, ale ignorujące pancerz (True Damage)
            // lub zwykłe fizyczne. Tutaj użyjemy True Damage, żeby 3% to było faktycznie 3%.
            target.TakeDamage(bonusDamage, DamageType.True,owner);
            
            // Debug.Log($"[DragonSlayer] {owner.unitName} uderzył {target.unitName} za {bonusDamage} (Race: {targetRace})");
        }
    }
}