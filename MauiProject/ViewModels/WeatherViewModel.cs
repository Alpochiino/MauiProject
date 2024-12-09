﻿namespace MauiProject.ViewModels;

using MauiProject.Models.Forecasts;
using MauiProject.Models.Location;
using MauiProject.Models.Weather;
using MauiProject.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

public class WeatherViewModel : ViewModelBase
{
    private ApiService _apiService;

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

    private List<WeatherData> _dailyForecasts;
    public List<WeatherData> DailyForecasts
    {
        get => _dailyForecasts;
        set => SetProperty(ref _dailyForecasts, value);
    }

    private WeatherData _weatherData;
    public WeatherData WeatherData
    {
        get => _weatherData;
        set => SetProperty(ref _weatherData, value);
    }

    private City _selectedCity;
    public City SelectedCity
    {
        get => _selectedCity;
        set
        {
            if (SetProperty(ref _selectedCity, value))
            {
                LoadWeatherData(value.Name);
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
                LoadCities(value.Code);
            }
        }
    }

    public WeatherViewModel()
    {
        _apiService = new ApiService();
        LoadCountries();
    }

    public async Task LoadCountries()
    {
        Countries = await _apiService.GetCountriesAsync();
    }

    public async Task LoadCities(string countryCode)
    {
        Cities = await _apiService.GetCitiesByCountryAsync(countryCode);
    }

    public async Task LoadWeatherData(string cityName)
    {
        WeatherData = await _apiService.GetWeatherDataAsync(cityName);
        DailyForecasts = await _apiService.GetDailyForecastAsync(cityName);

        GroupedDailyForecasts = new ObservableCollection<HourlyForecastGroup>(
            _apiService.GroupWeatherData(DailyForecasts)
        );

        OnPropertyChanged(nameof(GroupedDailyForecasts));
    }

    public ObservableCollection<HourlyForecastGroup> GroupedDailyForecasts { get; set; } = new ObservableCollection<HourlyForecastGroup>();
}
