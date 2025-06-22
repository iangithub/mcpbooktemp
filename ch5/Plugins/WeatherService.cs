using System.ComponentModel;
using Microsoft.SemanticKernel;

public class WeatherService
{
    private static readonly Dictionary<string, int> CityTemperatures = new Dictionary<string, int>
        {
            { "Taipei", 28 },
            { "Kaohsiung", 31 },
            { "Taichung", 27 },
            { "Tainan", 30 },
            { "Hsinchu", 26 }
        };

    [KernelFunction]
    [Description("Retrieves the today temperature of the city.")]
    public int GetTodayTemperature(
        [Description("The name of the city to get the temperature for.The city names in the weather data are in English")]
            string city)
    {
        if (CityTemperatures.TryGetValue(city, out int temp))
        {
            return temp;
        }
        // 模擬未知城市的溫度
        return 25;
    }
}