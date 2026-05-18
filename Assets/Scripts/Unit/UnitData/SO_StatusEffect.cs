using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Combat/Status Effect")]
public class StatusEffectSO : ScriptableObject
{
    public ConditionType type;
    public string effectName;
    public float duration = 3f;
    
   [Header("Visuals")]
    public Sprite icon; // <-- TUTAJ PRZECIĄGNIJ SWOJE OBRAZKI
    public Color iconTint = Color.white; // Opcjonalnie do zmiany koloru
    
    [Header("Mechanics")]
    public float modifierValue = 0.33f; // Twoje 33% z tabeli
    public float tickRate = 1f; // Dla efektów typu Burned


}