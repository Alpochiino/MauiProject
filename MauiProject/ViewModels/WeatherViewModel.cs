namespace MauiProject.ViewModels;

using MauiProject.Models.Forecasts;
using MauiProject.Models.Location;
using MauiProject.Models.Weather;
using MauiProject.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class WeatherViewModel : ViewModelBase
{
	private readonly ApiService _apiService;

	private List<Country> _countries;
	public List<Country> Countries
	{
		get => _countries;
		set => SetProperty(ref _countries, value);
	}

	private List<City> _cities;
	public List<City> Cities
	{
		get => _cities;
		set => SetProperty(ref _cities, value);
	}

	private ObservableCollection<HourlyForecastGroup> _groupedDailyForecasts = new();
	public ObservableCollection<HourlyForecastGroup> GroupedDailyForecasts
	{
		get => _groupedDailyForecasts;
		set => SetProperty(ref _groupedDailyForecasts, value);
	}

	private WeatherData _weatherData;
	public WeatherData WeatherData
	{
		get => _weatherData;
		set => SetProperty(ref _weatherData, value);
	}

	private string _temperature;
	public string Temperature
	{
		get => _temperature;
		set => SetProperty(ref _temperature, value);
	}

	private string _currentCondition;
	public string CurrentCondition
	{
		get => _currentCondition;
		set => SetProperty(ref _currentCondition, value);
	}

	private string _weatherIconUrl;
	public string WeatherIconUrl
	{
		get => _weatherIconUrl;
		set => SetProperty(ref _weatherIconUrl, value);
	}

	private City _selectedCity;
	public City SelectedCity
	{
		get => _selectedCity;
		set
		{
			if (SetProperty(ref _selectedCity, value))
			{
				LoadWeatherDataAsync(value.Name);
			}
		}
	}

	private Country _selectedCountry;
	public Country SelectedCountry
	{
		get => _selectedCountry;
		set
		{
			if (SetProperty(ref _selectedCountry, value))
			{
				LoadCitiesAsync(value.Code);
			}
		}
	}

	public WeatherViewModel()
	{
		_apiService = new ApiService();
		LoadCountriesAsync();
		InitializeWeatherForCurrentLocationAsync();
	}

	private async Task LoadCountriesAsync()
	{
		Countries = await _apiService.GetCountriesAsync();
	}

	private async Task LoadCitiesAsync(string countryCode)
	{
		Cities = await _apiService.GetCitiesByCountryAsync(countryCode);
	}

	private async Task LoadWeatherDataAsync(string cityName)
	{
		WeatherData = await _apiService.GetWeatherDataAsync(cityName);
		Temperature = $"{WeatherData.Temperature}°C";
		CurrentCondition = WeatherData.WeatherCondition;
		WeatherIconUrl = WeatherData.IconUrl;

		var dailyForecasts = await _apiService.GetDailyForecastAsync(cityName);
		GroupedDailyForecasts = new ObservableCollection<HourlyForecastGroup>(
			_apiService.GroupWeatherData(dailyForecasts)
		);
	}

	internal async Task InitializeWeatherForCurrentLocationAsync()
	{
		try
		{
			var location = await _apiService.GetCurrentLocationAsync();
			if (location == null)
			{
				Console.WriteLine("Unable to determine location.");
				return;
			}

			var city = await _apiService.GetCityFromLocationAsync(location);
			if (city == null)
			{
				Console.WriteLine("Unable to determine city from location.");
				return;
			}

			var country = Countries.FirstOrDefault(c => c.Name == city.Country);
			if (country != null)
			{
				SelectedCountry = country;
				await LoadCitiesAsync(country.Code);
			}
			
			await Task.Delay(100);
			if (city != null)
			{
				SelectedCity = city;
			}
			await LoadWeatherDataAsync(city.Name);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error initializing weather for current location: {ex.Message}");
		}
	}
}