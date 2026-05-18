using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "DND_Chess/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    public int rarity; // Siła potwora (np. 1-5)
    public string race;

    [Header("Mana System")]
    public int maxMana = 100;
    public int startMana = 0;       
    public int manaPerAttack = 10;  
    public int manaRegenPerSecond = 0; // Dodane dla spójności z sojusznikami

    [Header("Combat Stats")]
    [Tooltip("Szanasa na trafienie (1.0 = 100%)")]
    public float hitChance = 1.0f;
    [Tooltip("Szansa na unik (0.05 = 5%)")]
    public float dodgeChance = 0.00f;

    [Header("Stats")]
    public int maxHP = 50;
    public int attack = 5;
    public float vampirism = 0f;
    public int defense = 2;
    public int magicDefense = 2; // Dodane (brakowało wcześniej)
    public float attackSpeed = 1.0f;
    public float attackRange = 1.5f;
    
    [Space]
    [Tooltip("Bazowa moc umiejętności przeciwnika")]
    public int baseAbilityPower = 50; 
    [Tooltip("Mnożnik mocy (domyślnie 100 dla 100%)")]
    public int abilityPower = 0;

    [Header("Ability")]
    public Ability defaultAbility; 

    [Header("Visuals")]
    public Sprite enemySprite;
    public Color spriteTint = Color.red;
}