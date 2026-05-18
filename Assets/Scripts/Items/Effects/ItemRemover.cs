using UnityEngine;

[CreateAssetMenu(fileName = "ItemRemover", menuName = "Items/Effects/ItemRemover")]
public class RemoveItemsEffect : ItemEffect
{
    public override void Apply(Unit target)
    {
        if (target != null)
        {
            target.ReturnItemsToBench();
            Debug.Log($"[ItemEffect] Zdjęto przedmioty z {target.unitName}");
        }
    }
}