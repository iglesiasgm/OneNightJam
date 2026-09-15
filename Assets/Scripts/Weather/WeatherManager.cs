using UnityEngine;
using UnityEngine.Events;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private int temperatureInCelsius;
    [SerializeField] private float humidityPercent;
    [SerializeField] private float windSpeed;

    public static event UnityAction<WeatherData> OnChangeWeatherStat;

    public void Initialize(GameOptions gameOptions)
    {
        temperatureInCelsius = gameOptions.temperatureInCelsius;
        humidityPercent = gameOptions.humidityPercent;
        windSpeed = gameOptions.windSpeed;
        
        UpdateWeather();
    }

    private void UpdateWeather()
    {
        OnChangeWeatherStat?.Invoke(GetWeatherData());
    }

    public WeatherData GetWeatherData()
    {
        return new WeatherData(temperatureInCelsius, humidityPercent, windSpeed);
    }
}

public struct WeatherData
{
    public int TemperatureInCelsius;
    public float HumidityPercent;
    public float WindSpeed;
    
    public WeatherData(
        int temperatureInCelsius, 
        float humidityPercent,
        float windSpeed)
    {
        TemperatureInCelsius = temperatureInCelsius;
        HumidityPercent = humidityPercent;
        WindSpeed = windSpeed;
    }
}