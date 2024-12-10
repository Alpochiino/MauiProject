using MauiProject.ViewModels;

namespace MauiProject;


public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}
	
	protected override async void OnAppearing()
	{
		base.OnAppearing();
        var viewModel = BindingContext as WeatherViewModel;
        if (viewModel != null)
        {
            await viewModel.InitializeWeatherForCurrentLocationAsync();
        }	}
}
