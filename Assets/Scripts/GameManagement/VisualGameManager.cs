using UnityEngine;

/// <summary>
/// Se encarga de actualizar la de forma coherente los visuals del juego. Pendiente de los distintos managers de clima, hora y demás.
/// </summary>
public class VisualGameManager : MonoBehaviour
{
    // Referenciar elementos de pantalla y escena (UI, Shaders y Objetos)
    
    private void OnEnable()
    {
        WeatherManager.OnChangeWeatherStat += OnChangeWeather;
        GameClock.OnChangeHour += OnChangeHour;
    }

    private void OnDisable()
    {
        WeatherManager.OnChangeWeatherStat -= OnChangeWeather;
        GameClock.OnChangeHour -= OnChangeHour;
    }

    private void OnChangeWeather(WeatherData weatherData)
    {
        Debug.Log("Clima actualizado en VisualGameManager");
        // cambiar las referencias visuales en funcion de los valores de entrada
        // del clima
    }
    
    private void OnChangeHour(TimeData timeData)
    {
        Debug.Log("Tiempo actualizado en VisualGameManager");
        // cambiar las referencias visuales en funcion de los valores de entrada
        // del tiempo del dia
    }
}