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
					Country = city.countryName,
					CountryCode= countryCode,
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
			var latitude = location.Latitude.ToString("F6", CultureInfo.InvariantCulture);
			var longitude = location.Longitude.ToString("F6", CultureInfo.InvariantCulture);

			// LocationIQ Reverse Geocoding API
			var url = $"https://us1.locationiq.com/v1/reverse.php?key=pk.542b0e16be8de286956311ce5e738f5a&lat={latitude}&lon={longitude}&format=json";

			Console.WriteLine($"Request URL: {url}");

			var response = await _httpClient.GetStringAsync(url);
			var data = JsonConvert.DeserializeObject<dynamic>(response);

			if (data?.address == null)
			{
				Console.WriteLine("No data found in API response.");
				return null;
			}

			var city = (string)data.address.city ?? (string)data.address.town ?? (string)data.address.village;
			var country = (string)data.address.country;
			var countryCode = (string)data.address.country_code;
			
			if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(country))
			{
				Console.WriteLine("City or country data is missing.");
				return null;
			}

			return new City { Name = city, Country = country, CountryCode = countryCode?.ToUpperInvariant() };
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