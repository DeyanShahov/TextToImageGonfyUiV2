using TextToImageGonfyUiV2.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class AdvertisementPage : ContentPage
{
    AdvertisementService _advertisementService;

    public AdvertisementPage()
	{
		InitializeComponent();
        _advertisementService = new AdvertisementService(isLoading => ToggleLoading(isLoading),  ClosePage);       
    }

    override protected void OnAppearing()
    {
        base.OnAppearing();
        _advertisementService.CreateRewardedInterstitial();
    }

    private async void ClosePage() => await Navigation.PopModalAsync();

    private void ToggleLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }
}
