using UnityEngine;

// Ten atrybut sprawia, że nie możesz stworzyć "pustej" umiejętności, 
// musisz stworzyć jej konkretny rodzaj (np. Fireball).
public abstract class Ability : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    public Sprite icon;
    [TextArea] public string description;

    public bool isHostileAbility = true;


    // To wywołuje Unit.cs w metodzie TryCastAbility()
    public void Execute(Unit caster, Unit target)
    {
        // 1. Sprawdzenie Spellguard Shield (jeśli cel istnieje)
        if (target != null && target.ConsumeSpellShield())
        {
            Debug.Log($"[{abilityName}] zablokowane przez Spellguard Shield jednostki {target.unitName}!");
            return;
        }

        // 2. Wykonanie unikalnej logiki skilla (zdefiniowanej w klasach pochodnych)
        Cast(caster, target);
    }
    // Metoda wywoływana przez Unit.cs
    public abstract void Cast(Unit caster, Unit target);
}