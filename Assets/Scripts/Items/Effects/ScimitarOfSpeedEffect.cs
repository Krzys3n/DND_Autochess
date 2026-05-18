using UnityEngine;

[CreateAssetMenu(fileName = "ScimitarOfSpeedEffect", menuName = "Items/Effects/Scimitar of Speed")]
public class ScimitarOfSpeedEffect : ItemEffect
{
    public override void Apply(Unit target)
    {
        // Przy założeniu przedmiotu resetujemy stosy na wszelki wypadek
        target.flickerStacks = 0;
    }

    public override void OnHit(Unit owner, Unit target)
    {
        // Każdy atak dodaje 1 ładunek Flicker
        owner.flickerStacks++;
        
        // Natychmiast przeliczamy statystyki, żeby gracz widział wzrost AS
        owner.RecalculateStats();
        
        Debug.Log($"[Scimitar] {owner.unitName} ma teraz {owner.flickerStacks} stosów Flicker. AS: {owner.attackSpeed:F2}");
    }
}