using UnityEngine;

[CreateAssetMenu(fileName = "NewSynergy", menuName = "DND_Chess/Synergy Data")]
public class SynergyData : ScriptableObject
{
    public string synergyName; // Tutaj wpiszesz "Fighter" lub "Avernus"
    
    [Header("Typ Synergii")]
    public bool isFaction; // Zaznaczysz dla Avernus, odznaczysz dla Fighter

    [Header("Wygląd")]
    public Sprite icon;

    [Header("Logika Bonusów")]
    public int[] thresholds; // np. 2, 4, 6
    public int bonusAttackPerThreshold;

    public int GetTotalBonus(int count)
    {
        int bonus = 0;
        if (thresholds == null) return 0;
        foreach (int t in thresholds)
        {
            if (count >= t) bonus += bonusAttackPerThreshold;
        }
        return bonus;
    }
}