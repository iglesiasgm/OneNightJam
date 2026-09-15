using System;
using UnityEngine;
using UnityEngine.Events;

public class GameClock : MonoBehaviour
{
    [Header("Clock")]
    [SerializeField] private bool runClock = true;

    [SerializeField] private DayOfWeek currentDay = DayOfWeek.Monday;

    [Range(0f, 23.59f)]
    [SerializeField] private float currentGameTimeHourFormat = 6f;

    [Header("Time speed")]
    [Tooltip("How many game minutes pass for every real second.")]
    [Min(0f)]
    [SerializeField] private float gameMinutesPerRealSecond = 1f;

    [Header("Other Systems Related")] 
    [SerializeField] private float timeForUpdates = 5f;
    private float currentTimeForUpdates = -1f;

    public DayOfWeek CurrentDay => currentDay;
    public float CurrentHour => currentGameTimeHourFormat;

    public int CurrentHourInt => Mathf.FloorToInt(currentGameTimeHourFormat);

    public int CurrentMinute => Mathf.FloorToInt((currentGameTimeHourFormat - CurrentHourInt) * 60f);

    public string CurrentTimeText => $"{CurrentHourInt:00}:{CurrentMinute:00}";
    
    public static event UnityAction<TimeData> OnChangeHour;

    public void Initialize(GameOptions gameOptions)
    {
        currentDay = gameOptions.currentDay;
        currentGameTimeHourFormat = gameOptions.currentHour;
    }

    private void Update()
    {
        if (!runClock) return;

        float hoursPerSecond = gameMinutesPerRealSecond / 60f;

        currentGameTimeHourFormat += hoursPerSecond * Time.deltaTime;
        currentTimeForUpdates -= Time.deltaTime;

        if (currentTimeForUpdates < 0f)
        {
            OnChangeHour?.Invoke(new TimeData(currentGameTimeHourFormat));
            currentTimeForUpdates = timeForUpdates;
        }
        
        while (currentGameTimeHourFormat >= 24f)
        {
            currentGameTimeHourFormat -= 24f;
            AdvanceDay();
        }
    }

    private void AdvanceDay()
    {
        int nextDay =
            ((int)currentDay + 1) % 7;

        currentDay =
            (DayOfWeek)nextDay;
    }
}

public struct TimeData
{
    public float CurrentHour;

    public TimeData(float currentHour)
    {
        CurrentHour = currentHour;
    }
}