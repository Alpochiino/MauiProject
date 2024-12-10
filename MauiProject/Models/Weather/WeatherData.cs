namespace MauiProject.Models.Weather;

public class WeatherData
{
	public DateTime RawDate { get; set; }
	public double Temperature { get; set; }
	public double WindSpeed { get; set; }
	public string? WeatherCondition { get; set; }
	public string? IconUrl { get; set; }

	public string? Date { get; set; }
}
