namespace MauiProject.Models.Forecasts;
using System;

public class HourlyForecast
{
    public string? Hour { get; set; }
    public double Temperature { get; set; }
    public string? WeatherCondition { get; set; }
    public DateTime Date { get; set; }
}
