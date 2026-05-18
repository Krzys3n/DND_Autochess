using UnityEngine;

[CreateAssetMenu(fileName = "CloakOfElvenkindEffect", menuName = "Items/Effects/Cloak Of Elvenkind")]
public class CloakOfElvenkindEffect : ItemEffect
{
    [Header("Dodge Settings")]
    [Tooltip("Szansa na unik w ułamku. 0.33 = 33%")]
    public float dodgeChanceBonus = 0.33f;

    public override void Apply(Unit target)
    {
        // Wywoływane tylko raz przy założeniu. 
        // Idealne miejsce, jeśli w przyszłości będziesz chciał dodać tu Particle System (np. spadające liście).
        Debug.Log($"[Cloak of Elvenkind] {target.unitName} rozpływa się w kolorach otoczenia!");
    }

    public override void ApplyPassives(Unit target)
    {
        if (target == null) return;

        // ApplyPassives wykonuje się ZAWSZE po załadowaniu czystej bazy.
        // Jeśli jednostka miała bazowo 5% uniku (0.05), teraz bezpiecznie będzie miała 38% (0.38).
        target.dodgeChance += dodgeChanceBonus;
    }
}