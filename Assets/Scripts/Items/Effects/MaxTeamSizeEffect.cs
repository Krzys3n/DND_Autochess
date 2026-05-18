using UnityEngine;

[CreateAssetMenu(fileName = "MaxTeamSizeEffect", menuName = "Items/Effects//Max Team Size")]
public class MaxTeamSizeEffect : ItemEffect
{
    public int bonusSize = 1;

    public override void Apply(Unit target)
    {
        // Ten efekt technicznie nie musi nic robić na samej jednostce,
        // bo obsłużymy go globalnie w InventoryManagerze.
        Debug.Log("Tome of Ultimate Mastery applied to unit: " + target.unitName);
    }
}