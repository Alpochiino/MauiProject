namespace MauiProject.Services;
using MauiProject.Models.Location;
using MauiProject.Models.Weather;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using MauiProject.Models.Forecasts;

public class ApiService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _geoNamesUsername = "Alpochiino";

    public async Task<List<Country>> GetCountriesAsync()
    {
        var countries = new List<Country>();
        try
        {
            var response = await _httpClient.GetStringAsync($"http://api.geonames.org/countryInfoJSON?username={_geoNamesUsername}");
            var countryData = JsonConvert.DeserializeObject<dynamic>(response);

            foreach (var country in countryData.geonames)
            {
                countries.Add(new Country
                {
                    Name = country.countryName,
                    Code = country.countryCode
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching countries: {ex.Message}");
        }
        return countries;
    }

    public async Task<List<City>> GetCitiesByCountryAsync(string countryCode)
    {
        var cities = new List<City>();
        try
        {
            var response = await _httpClient.GetStringAsync($"http://api.geonames.org/searchJSON?country={countryCode}&maxRows=100&username={_geoNamesUsername}");
            var cityData = JsonConvert.DeserializeObject<dynamic>(response);

            foreach (var city in cityData.geonames)
            {
                cities.Add(new City
                {
                    Name = city.name,
                    Country = city.countryName
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching cities: {ex.Message}");
        }
        return cities;
    }

    public async Task<WeatherData> GetWeatherDataAsync(string cityName)
    {
        var weather = new WeatherData();
        try
        {
            var weatherResponse = await _httpClient.GetStringAsync($"https://api.openweathermap.org/data/2.5/forecast?q={cityName}&appid=47b00682e84fa9b2dc24565d4a3d431a&units=metric");
            var weatherJson = JsonConvert.DeserializeObject<dynamic>(weatherResponse);

            weather.Temperature = weatherJson.list[0].main.temp;
            weather.WeatherCondition = weatherJson.list[0].weather[0].description;
            weather.WindSpeed = weatherJson.list[0].wind.speed;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching weather: {ex.Message}");
        }
        return weather;
    }

    public async Task<List<WeatherData>> GetDailyForecastAsync(string cityName)
    {
        var forecastList = new List<WeatherData>();
        try
        {
            var forecastResponse = await _httpClient.GetStringAsync($"https://api.openweathermap.org/data/2.5/forecast?q={cityName}&appid=47b00682e84fa9b2dc24565d4a3d431a&units=metric");
            var forecastJson = JsonConvert.DeserializeObject<dynamic>(forecastResponse);

            if (forecastJson?.list != null && forecastJson.list.Count > 0)
            {
                foreach (var entry in forecastJson.list)
                {
                    DateTime rawDate = DateTime.ParseExact(entry.dt_txt.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    forecastList.Add(new WeatherData
                    {
                        RawDate = rawDate,
                        Date = rawDate.ToString("ddd, MMM dd, hh:mm tt", CultureInfo.InvariantCulture),
                        Temperature = entry.main.temp,
                        WeatherCondition = entry.weather[0].description
                    });
                }
            }
            else
            {
                Console.WriteLine("No forecast data found in the 'list' section.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching 5-day forecast: {ex.Message}");
        }

        return forecastList;
    }

    public List<HourlyForecastGroup> GroupWeatherData(List<WeatherData> dailyForecasts)
    {
        try
        {
            var groupedData = dailyForecasts
                .GroupBy(w => w.RawDate.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => new HourlyForecastGroup(
                    g.Key.ToString(),
                    g.Select(weatherData => new HourlyForecast
                    {
                        Hour = weatherData.RawDate.ToString("HH:mm"),
                        Temperature = weatherData.Temperature,
                        WeatherCondition = weatherData.WeatherCondition,
                        Date = weatherData.RawDate
                    }).ToList()))
                .ToList();

            return groupedData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error grouping weather data: {ex.Message}");
            return new List<HourlyForecastGroup>();
        }
    }
}