namespace MauiProject.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiProject.Models.Forecasts;
using MauiProject.Models.Location;
using MauiProject.Models.Weather;
using MauiProject.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public partial class WeatherViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty] private List<Country> countries = new List<Country>();

    [ObservableProperty] private List<City> cities = new List<City>();

    [ObservableProperty] private ObservableCollection<HourlyForecastGroup> groupedDailyForecasts = new ObservableCollection<HourlyForecastGroup>();

    [ObservableProperty] private WeatherData? weatherData;

    [ObservableProperty] private string? temperature;

    [ObservableProperty] private string? currentCondition;

    [ObservableProperty] private string? weatherIconUrl;

    [ObservableProperty] private City? selectedCity;

    [ObservableProperty] private Country? selectedCountry;

    public WeatherViewModel(ApiService apiService)
    {
        _apiService = apiService;
        InitializeViewModelAsync();
    }

    public async Task InitializeViewModelAsync()
    {
        await LoadCountriesAsync();
        await InitializeWeatherForCurrentLocationAsync();
    }

    partial void OnSelectedCountryChanged(Country value)
    {
        if (value != null)
        {
            LoadCitiesAsync(value.Code);
        }
    }

    partial void OnSelectedCityChanged(City value)
    {
        if (value != null)
        {
            LoadWeatherDataAsync(value.Name);
        }
    }

    private async Task LoadCountriesAsync()
    {
        try
        {
            var fetchedCountries = await _apiService.GetCountriesAsync();
            Countries = fetchedCountries;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading countries: {ex.Message}");
        }
    }

    private async Task LoadCitiesAsync(string countryCode)
    {
        try
        {
            var fetchedCities = await _apiService.GetCitiesByCountryAsync(countryCode);
            Cities = fetchedCities;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cities: {ex.Message}");
        }
    }

    private async Task LoadWeatherDataAsync(string cityName)
    {
        try
        {
            WeatherData = await _apiService.GetWeatherDataAsync(cityName);
            Temperature = $"{WeatherData.Temperature}°C";
            CurrentCondition = WeatherData.WeatherCondition;
            WeatherIconUrl = WeatherData.IconUrl;

            var dailyForecasts = await _apiService.GetDailyForecastAsync(cityName);
            GroupedDailyForecasts = new ObservableCollection<HourlyForecastGroup>(
                GroupWeatherData(dailyForecasts)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading weather data: {ex.Message}");
        }
    }

    public List<HourlyForecastGroup> GroupWeatherData(List<WeatherData> dailyForecasts)
    {
        try
        {
            var today = (int)DateTime.Now.DayOfWeek;
            var groupedData = dailyForecasts
                .GroupBy(w => w.RawDate.DayOfWeek)
                .OrderBy(g => (int)g.Key >= today ? (int)g.Key - today : (int)g.Key + 7 - today)
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

            var country = Countries.FirstOrDefault(c => c.Code == city.CountryCode);
            if (country != null)
            {
                SelectedCountry = country;
                await LoadCitiesAsync(country.Code);
            }

            if (Cities.Any(c => c.Name == city.Name))
            {
                SelectedCity = Cities.First(c => c.Name == city.Name);
                await LoadWeatherDataAsync(SelectedCity.Name);
            }
            else
            {
                await LoadWeatherDataAsync(city.Name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing weather for current location: {ex.Message}");
        }
    }
}
