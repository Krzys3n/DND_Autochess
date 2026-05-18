using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuiverOfEhlonnaEffect", menuName = "Items/Effects/Quiver of Ehlonna")]
public class QuiverOfEhlonnaEffect : ItemEffect
{
    public float auraRange = 1f; // Zasięg szukania
    public float bonusValue = 0.2f; // 20%

    public override void Apply(Unit target) { }

    public override void OnCombatStart(Unit owner)
    {
        // 1. Bonus dla właściciela
        owner.quiverAuraBonus = bonusValue;
        owner.RecalculateStats();

        // 2. Znajdź sąsiadów po lewej i prawej
        FindAndBuffNeighbors(owner);
    }

    private void FindAndBuffNeighbors(Unit owner)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, auraRange);
        
        Unit closestLeft = null;
        Unit closestRight = null;
        float minDistLeft = float.MaxValue;
        float minDistRight = float.MaxValue;

        foreach (var col in colliders)
        {
            Unit ally = col.GetComponent<Unit>();
            
            // Warunki: to musi być jednostka, nie ja, i ta sama drużyna
            if (ally == null || ally == owner || ally.isEnemy != owner.isEnemy) continue;

            float distance = Vector2.Distance(owner.transform.position, ally.transform.position);

            // Sprawdzamy pozycję na osi X
            if (ally.transform.position.x < owner.transform.position.x)
            {
                // Kandydat na lewą stronę
                if (distance < minDistLeft)
                {
                    minDistLeft = distance;
                    closestLeft = ally;
                }
            }
            else
            {
                // Kandydat na prawą stronę
                if (distance < minDistRight)
                {
                    minDistRight = distance;
                    closestRight = ally;
                }
            }
        }

        // 3. Przyznaj bonusy znalezionym jednostkom
        if (closestLeft != null)
        {
            closestLeft.quiverAuraBonus = bonusValue;
            closestLeft.RecalculateStats();
            Debug.Log($"[Quiver] Wzmocniono sojusznika po LEWEJ: {closestLeft.unitName}");
        }

        if (closestRight != null)
        {
            closestRight.quiverAuraBonus = bonusValue;
            closestRight.RecalculateStats();
            Debug.Log($"[Quiver] Wzmocniono sojusznika po PRAWEJ: {closestRight.unitName}");
        }
    }
}