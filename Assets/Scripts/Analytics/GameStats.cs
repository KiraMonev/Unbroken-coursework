using System;
using System.Collections.Generic;

[Serializable]
public class GameStats {
    public int totalDeaths = 0;
    public float totalPlayTime = 0f;
    public Dictionary<string, int> deathsPerLevel = new();
    public Dictionary<string, int> killsPerEnemyType = new();
    public Dictionary<string, int> weaponUsage = new();

    public void RegisterDeath(string levelName) {
        totalDeaths++;
        if (!deathsPerLevel.ContainsKey(levelName))
            deathsPerLevel[levelName] = 0;
        deathsPerLevel[levelName]++;
    }

    public void RegisterKill(string enemyType) {
        if (!killsPerEnemyType.ContainsKey(enemyType))
            killsPerEnemyType[enemyType] = 0;
        killsPerEnemyType[enemyType]++;
    }

    public void RegisterWeaponUse(string weaponName) {
        if (!weaponUsage.ContainsKey(weaponName))
            weaponUsage[weaponName] = 0;
        weaponUsage[weaponName]++;
    }

    public void AddPlayTime(float seconds) {
        totalPlayTime += seconds;
    }
}
