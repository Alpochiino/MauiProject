namespace MauiProject;
using MauiProject.ViewModels;

public partial class MainPage : ContentPage
{
	public MainPage(WeatherViewModel vm)
	{
		BindingContext = vm;
		InitializeComponent();
	}
}
