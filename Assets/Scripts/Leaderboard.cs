using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] entryTexts;

    private const int    MaxEntries = 5;
    private const string CountKey   = "lb_count";
    private const string TimeKey    = "lb_time_";

    private void OnEnable() => RefreshDisplay();

    public void SubmitTime(float time)
    {
        var times = LoadTimes();
        times.Add(time);
        times.Sort();
        if (times.Count > MaxEntries)
            times.RemoveRange(MaxEntries, times.Count - MaxEntries);
        SaveTimes(times);
        RefreshDisplay(times);
    }

    private void RefreshDisplay() => RefreshDisplay(LoadTimes());

    private void RefreshDisplay(List<float> times)
    {
        for (int i = 0; i < entryTexts.Length; i++)
        {
            if (entryTexts[i] == null) continue;
            entryTexts[i].text = i < times.Count
                ? $"{i + 1}.  {RunTimer.FormatTime(times[i])}"
                : $"{i + 1}.  ---";
        }
    }

    private static List<float> LoadTimes()
    {
        var times = new List<float>();
        int count = PlayerPrefs.GetInt(CountKey, 0);
        for (int i = 0; i < count; i++)
            times.Add(PlayerPrefs.GetFloat(TimeKey + i));
        return times;
    }

    private static void SaveTimes(List<float> times)
    {
        PlayerPrefs.SetInt(CountKey, times.Count);
        for (int i = 0; i < times.Count; i++)
            PlayerPrefs.SetFloat(TimeKey + i, times[i]);
        PlayerPrefs.Save();
    }
}
