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
			var response = await _httpClient.GetStringAsync($"http://localhost:3000/geonames/countryInfoJSON?username={_geoNamesUsername}");
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
			var response = await _httpClient.GetStringAsync($"http://localhost:3000/geonames/searchJSON?country={countryCode}&maxRows=100&username={_geoNamesUsername}");
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

			string iconCode = weatherJson.list[0].weather[0].icon;
			weather.IconUrl = $"https://openweathermap.org/img/wn/{iconCode}@2x.png";
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
						WeatherCondition = entry.weather[0].description,
						IconUrl = $"https://openweathermap.org/img/wn/{entry.weather[0].icon}@2x.png"
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
				.OrderBy(g => (int)g.Key == 0 ? 7 : (int)g.Key)
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
	
	public async Task<Location> GetCurrentLocationAsync()
	{
		try
		{
			var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
			return location;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error getting location: {ex.Message}");
			return null;
		}
	}
	
	public async Task<City> GetCityFromLocationAsync(Location location)
	{
		if (location == null)
		{
			Console.WriteLine("Location is null.");
			return null;
		}

		try
		{
			// Format latitude and longitude
			var latitude = location.Latitude.ToString("F6", CultureInfo.InvariantCulture);
			var longitude = location.Longitude.ToString("F6", CultureInfo.InvariantCulture);
			var query = Uri.EscapeDataString($"{latitude},{longitude}");

			// Construct the URL
			var url = $"https://api.positionstack.com/v1/reverse?access_key=96ca26a1377f94f0b3dcd2cbd00dbf60&query={query}";
			Console.WriteLine($"Request URL: {url}");

			// Make the HTTP request
			var response = await _httpClient.GetStringAsync(url);
			var data = JsonConvert.DeserializeObject<dynamic>(response);

			// Validate response data
			if (data?.data == null || data.data.Count == 0)
			{
				Console.WriteLine("No data found in API response.");
				return null;
			}

			// Extract city and country information
			var city = (string)data.data[0]?.locality;
			var country = (string)data.data[0]?.country;

			if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(country))
			{
				Console.WriteLine("City or country data is missing.");
				return null;
			}

			return new City { Name = city, Country = country };
		}
		catch (HttpRequestException httpEx)
		{
			Console.WriteLine($"HTTP request error: {httpEx.Message}");
			return null;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching city from location: {ex.Message}");
			return null;
		}
	}
}