using System;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    [Header("Clock")]
    [SerializeField] private bool runClock = true;

    [SerializeField] private DayOfWeek currentDay = DayOfWeek.Monday;

    [Range(0f, 23.99f)]
    [SerializeField] private float currentHour = 6f;

    [Header("Time speed")]
    [Tooltip("How many game minutes pass for every real second.")]
    [Min(0f)]
    [SerializeField] private float gameMinutesPerRealSecond = 1f;

    public DayOfWeek CurrentDay => currentDay;
    public float CurrentHour => currentHour;

    public int CurrentHourInt =>
        Mathf.FloorToInt(currentHour);

    public int CurrentMinute =>
        Mathf.FloorToInt(
            (currentHour - CurrentHourInt) * 60f
        );

    public string CurrentTimeText =>
        $"{CurrentHourInt:00}:{CurrentMinute:00}";

    private void Update()
    {
        if (!runClock)
            return;

        float hoursPerSecond =
            gameMinutesPerRealSecond / 60f;

        currentHour +=
            hoursPerSecond * Time.deltaTime;

        while (currentHour >= 24f)
        {
            currentHour -= 24f;
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