using UnityEngine;

[CreateAssetMenu(fileName = "AdamantineArmorEffect", menuName = "Items/Effects/Adamantine Armor")]
public class AdamantineArmorEffect : ItemEffect
{
    public override void Apply(Unit target) { }

    public override void ApplyPassives(Unit target)
    {
        // Zbroja z Adamantium daje całkowitą odporność na krytyki i odepchnięcia
        target.immuneToCrits = true;
        target.immuneToForcedMovement = true;
    }
}