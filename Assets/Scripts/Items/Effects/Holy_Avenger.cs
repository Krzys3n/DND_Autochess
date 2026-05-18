using UnityEngine;

[CreateAssetMenu(fileName = "HolyAvengerEffect", menuName = "Items/Effects/Holy Avenger")]
public class HolyAvengerEffect : ItemEffect
{

        public override void Apply(Unit target)
    {
            
        Debug.Log("Nałożono Holy Avenger");
    }
    public override void ApplyPassives(Unit target)
    {
        // Zwiększamy bazową manę za atak o 5
        target.manaPerAttack += 5;

        // Jeśli masz system pasywnego mana regen (np. co sekundę)
        // Możesz tutaj dodać flagę lub zmodyfikować statystykę
        // target.passiveManaRegen += 1; 

        Debug.Log($"[Holy Avenger] {target.unitName}: Mana per Attack increased to {target.manaPerAttack}");
    }
}