namespace MauiProject;
using MauiProject.ViewModels;

public partial class MainPage : ContentPage
{
	public MainPage(WeatherViewModel vm)
	{
		BindingContext = vm;
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
        if (BindingContext is WeatherViewModel viewModel)
        {
            await viewModel.InitializeWeatherForCurrentLocationAsync();
        }
    }
}
