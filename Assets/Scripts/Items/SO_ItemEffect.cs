using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
    // Wywoływane przy zakładaniu przedmiotu (np. dla statystyk lub efektów wizualnych)
    public abstract void Apply(Unit target);

    public virtual void ApplyPassives(Unit target) { }
    public virtual void OnCombatStart(Unit owner) { }
    // NOWOŚĆ: Wywoływane przy każdym ataku
    // Virtual sprawia, że metoda jest opcjonalna - domyślnie nic nie robi
    public virtual void OnHit(Unit owner, Unit target) { }

    public virtual void OnOwnerTakeDamage(Unit owner, ref int damage, DamageType type) { }
    // NOWOŚĆ: Wywoływane cyklicznie podczas walki (np. co 0.5s)
    public virtual void OnTick(Unit owner) { }
    // NOWOŚĆ: Wywoływane, gdy właściciel rzuci zaklęcie / użyje umiejętności
    public virtual void OnAbilityCast(Unit owner) { }
}