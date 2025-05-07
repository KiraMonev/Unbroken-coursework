using UnityEngine;
using UnityEngine.UI;

public class GameStatsUI : MonoBehaviour {
    public Text statsText;

    void Start() {
        var stats = GameStatsManager.Instance.stats;

        string output = $"Смертей всего: {stats.totalDeaths}\n" +
                        $"Общее время игры: {Mathf.RoundToInt(stats.totalPlayTime)} сек.\n";

        output += "\nСмерти по уровням:\n";
        foreach (var kv in stats.deathsPerLevel) {
            output += $"{kv.Key}: {kv.Value}\n";
        }

        output += "\nИспользование оружия:\n";
        foreach (var kv in stats.weaponUsage) {
            output += $"{kv.Key}: {kv.Value}\n";
        }

        statsText.text = output;
    }
}
