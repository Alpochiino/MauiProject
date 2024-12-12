namespace MauiProject.Models.Forecasts;
using System.Collections.Generic;

public class HourlyForecastGroup
{
    public string? Key { get; set; }
    public List<HourlyForecast>? Items { get; set; }

    public HourlyForecastGroup(string key, List<HourlyForecast> items) { Key = key; Items = items; }
}
