using Plugin.AdMob;
using Plugin.AdMob.Services;
using System.Diagnostics;
using TextToImageGonfyUiV2.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class AdvertisementPage : ContentPage
{
    private readonly IRewardedInterstitialAdService _rewardedInterstitialAdService; //For AdMob    

    public AdvertisementPage()
	{
		InitializeComponent();

        // Инициализиране на AdMob
        _rewardedInterstitialAdService = ServiceProvider.GetRequiredService<IRewardedInterstitialAdService>();
        _rewardedInterstitialAdService.OnAdLoaded += (_, __) => Debug.WriteLine("Rewarded interstitial ad prepared.");
        _rewardedInterstitialAdService.PrepareAd(onUserEarnedReward: UserDidEarnReward);      
    }

    override protected void OnAppearing()
    {
        base.OnAppearing();
        //LoadingIndicator.IsVisible = true;
        //LoadingIndicator.IsRunning = true;

        CreateRewardedInterstitial();

        //OnShowRewardedInterstitialClicked();
    }

    private void OnShowRewardedInterstitialClicked()
    {
        if (_rewardedInterstitialAdService.IsAdLoaded)
        {
            _rewardedInterstitialAdService.ShowAd();
        }

        _rewardedInterstitialAdService.PrepareAd(onUserEarnedReward: UserDidEarnReward);
    }

    private void CreateRewardedInterstitial()
    {
        var rewardedInterstitialAd = _rewardedInterstitialAdService.CreateAd();
        rewardedInterstitialAd.OnUserEarnedReward += (_, reward) =>
        {
            UserDidEarnReward(reward);
        };
        rewardedInterstitialAd.OnAdLoaded += RewardedInterstitialAd_OnAdLoaded;
        rewardedInterstitialAd.Load();

    }

    private void RewardedInterstitialAd_OnAdLoaded(object? sender, EventArgs e)
    {
        if (sender is IRewardedInterstitialAd rewardedInterstitialAd)
        {
            ToggleLoading(false);
            rewardedInterstitialAd.Show();
        }
    }

    private async void UserDidEarnReward(RewardItem rewardItem)
    {
        Debug.WriteLine($"User earned {rewardItem.Amount} {rewardItem.Type}.");
        AppSettings.imageCounter = 0; // Reset the counter after showing the ad
        await Navigation.PopModalAsync();
    }

 

    private void ToggleLoading(bool isLoading)
    {
        //LoadingIndicator.IsRunning = isLoading;
       // LoadingIndicator.IsVisible = isLoading;
    }
}

internal static class ServiceProvider
{
    public static TService? GetService<TService>()
    => Current.GetService<TService>();

    public static TService GetRequiredService<TService>() where TService : notnull
        => Current.GetRequiredService<TService>();

    public static IServiceProvider Current =>
        (IPlatformApplication.Current ?? throw new InvalidOperationException("Cannot resolve current application.")).Services;
}