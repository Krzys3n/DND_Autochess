using UnityEngine;

[CreateAssetMenu(fileName = "MantleOfFreeActionEffect", menuName = "Items/Effects/Mantle of Free Action")]
public class MantleOfFreeActionEffect : ItemEffect
{
    [Header("Status Settings")]
    [Tooltip("Przeciągnij tu SO statusu Invulnerable")]
    public StatusEffectSO invulnerableStatus; 

    public override void Apply(Unit target) { }

    public override void OnCombatStart(Unit owner)
    {
        if (invulnerableStatus != null)
        {
            UnitStatusManager sm = owner.GetComponent<UnitStatusManager>();
            if (sm != null)
            {
                // Nakładamy status Invulnerable na właściciela.
                // Aby trwał na "nieskończoność", po prostu w pliku SO tego statusu 
                // ustaw mu czas trwania (duration) na np. 9999 sekund!
                sm.ApplyEffect(invulnerableStatus, owner);
                
                Debug.Log($"[Mantle of Free Action] {owner.unitName} okrywa się płaszczem i zyskuje status Invulnerable!");
            }
        }
    }
}