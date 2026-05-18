using UnityEngine;

[CreateAssetMenu(fileName = "AddClassEffect", menuName = "Items/Effects/Add Class")]
public class AddClassEffect : ItemEffect
{
    [Tooltip("Wybierz synergię (klasę), którą ten przedmiot ma nadać jednostce.")]
    public SynergyData classToAdd;

    // Apply() wywoła się tylko raz przy przeciągnięciu itemka
    public override void Apply(Unit target)
    {
        if (SynergyManager.Instance != null)
            SynergyManager.Instance.RecalculateSynergies();
            
        Debug.Log($"Założono przedmiot dodający klasę {classToAdd.synergyName} do {target.unitName}");
    }

    // ApplyPassives() wywoła się w RecalculateStats, tuż po tym jak Unit.cs zresetuje swoje klasy!
    public override void ApplyPassives(Unit target)
    {
        if (!CanApply(target)) return;

        // Dodajemy klasę ponownie, by nie zniknęła po przeliczeniu statystyk
        target.activeClasses.Add(classToAdd);
    }

    // Funkcja sprawdzająca (bez zmian)
    public bool CanApply(Unit target)
    {
        if (target == null || classToAdd == null) return false;
        
        // Jeśli jednostka już ma tę klasę bazowo, zwracamy false
        if (target.activeClasses.Contains(classToAdd)) return false;

        return true;
    }
}