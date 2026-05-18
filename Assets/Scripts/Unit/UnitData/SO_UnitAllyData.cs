using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "DND_Chess/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string unitName;
    public SynergyData faction; 
    public List<SynergyData> unitClasses = new List<SynergyData>();
    public string race;
    public int rarity;
    public int cost;

    [Header("Mana System")]
    public int maxMana = 100;
    public int startMana = 0;
    public int manaPerAttack = 10;
    public int manaRegenPerSecond = 0;

    [Header("Combat Stats")]
    [Tooltip("Domyślnie 100. Przedmioty dodają do tej wartości (np. +20 AP = 120).")]
    public int abilityPower = 0; // JEDNO POLE NA GÓRZE
    [Tooltip("Szanasa na trafienie (1.0 = 100%)")]
    public float hitChance = 1.0f;
    [Tooltip("Szansa na unik (0.05 = 5%)")]
    public float dodgeChance = 0.00f;

    [Header("Stats LEVEL 1 (Base)")]
    public int maxHP = 50;
    public int attack = 10;
    public float vampirism = 0f;
    public int defense = 5;
    public int magicDefense = 3;
    public float critChance = 0.1f;
    public float attackSpeed = 1.0f;
    public float attackRange = 1.5f;
    [Tooltip("Bazowa moc/obrażenia skilla na 1*")]
    public int baseAbilityPower = 100; 

    [Header("Stats LEVEL 2 (Silver)")]
    public int maxHP_Lvl2 = 250;
    public int attack_Lvl2 = 25;
    public int defense_Lvl2 = 6;
    public int magicDefense_Lvl2 = 4;
    [Tooltip("Bazowa moc/obrażenia skilla na 2*")]
    public int baseAbilityPower_Lvl2 = 200;

    [Header("Stats LEVEL 3 (Gold)")]
    public int maxHP_Lvl3 = 600;
    public int attack_Lvl3 = 60;
    public int defense_Lvl3 = 10;
    public int magicDefense_Lvl3 = 6;
    [Tooltip("Bazowa moc/obrażenia skilla na 3*")]
    public int baseAbilityPower_Lvl3 = 400;

    [Header("Ability Info")]
    public Ability defaultAbility;

    [Header("Visuals")]
    public Sprite unitSprite; 
    public Sprite factionIcon; 
    public Color spriteTint = Color.white;
}