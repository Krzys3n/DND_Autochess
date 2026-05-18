using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "DND_Chess/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea] public string description;
    
    [Header("Usage Settings")]
    public bool isConsumable; 
    public bool isFinishedItem; // TRUE = przedmiot składany, FALSE = komponent podstawowy

    [Header("Recipe (Only if Finished Item)")]
    public ItemData recipeComponentA;
    public ItemData recipeComponentB;

    [Header("Flat Stat Bonuses")]
    public int healthBonus = 0;
    public int attackBonus = 0;
    public float vampirism = 0f;
    public int armorBonus = 0;
    public int magicResistBonus = 0;
    public int abilityPowerBonus = 0;
    public int startingMana = 0;
    public int manaRegen = 0;

    [Header("Percent Stat Bonuses (%)")]
    [Tooltip("Value in percent, e.g., 20 = +20% Attack Speed")]
    public float attackSpeedPercent = 0f;
    [Tooltip("Value in percent, e.g., 15 = +15% Crit Chance")]
    public float critChancePercent = 0f;
    [Tooltip("Value in percent, e.g., 10 = +10% Attack Damage")]
    public float attackDamagePercent = 0f;
    
    // NOWE POLA PROCENTOWE:
    [Tooltip("Value in percent, e.g., 15 = +15% Max Health")]
    public float healthPercentBonus = 0f;
    [Tooltip("Value in percent, e.g., 10 = +10% Armor")]
    public float armorPercentBonus = 0f;
    [Tooltip("Value in percent, e.g., 10 = +10% Magic Resist")]
    public float magicResistPercentBonus = 0f;

    [Header("Special Effect")]
    public ItemEffect specialEffect;

    public bool IsCraftableFrom(ItemData a, ItemData b)
    {
        if (!isFinishedItem || recipeComponentA == null || recipeComponentB == null) 
            return false;

        return (recipeComponentA == a && recipeComponentB == b) || 
               (recipeComponentA == b && recipeComponentB == a);
    }
}