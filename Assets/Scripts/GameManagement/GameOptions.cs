using UnityEngine;

[CreateAssetMenu(fileName = "New GameOptions", menuName = "ScriptableObjects/GameOptions")]
public class GameOptions : ScriptableObject
{
    [Header("Day Time Options")]
    public System.DayOfWeek currentDay;
    [Range(0f, 23.59f)] public float currentHour;
    
    [Header("Weather Options")]
    public int temperatureInCelsius;
    public float humidityPercent;
    public float windSpeed;
}