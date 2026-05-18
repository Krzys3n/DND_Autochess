using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct EnemySpawnInfo
{
    public EnemyData enemyData; // Używamy dedykowanych danych potwora
    public int gridX;
    public int gridY; 
}

[CreateAssetMenu(fileName = "NewStage", menuName = "DND_Chess/Stage Data")]
public class StageData : ScriptableObject
{
    public int stageNumber;
    public List<EnemySpawnInfo> enemies;
    public int goldReward = 2;
}