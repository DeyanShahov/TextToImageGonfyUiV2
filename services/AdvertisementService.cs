using Plugin.AdMob;
using Plugin.AdMob.Services;
using System.Diagnostics;
using TextToImageCore.services;

namespace TextToImageGonfyUiV2.services
{
    internal class AdvertisementService
    {
        private readonly IRewardedInterstitialAdService _rewardedInterstitialAdService; //For AdMob
        private readonly Action<bool> ToggleLoading; // For loading indicator
        private readonly Action ClosePage; // For closing the advertisement page


        public AdvertisementService(Action<bool> callback, Action callback1)
        {
            _rewardedInterstitialAdService = ServiceProvider.GetRequiredService<IRewardedInterstitialAdService>();
            _rewardedInterstitialAdService.OnAdLoaded += (_, __) => Debug.WriteLine("Rewarded interstitial ad prepared.");
            _rewardedInterstitialAdService.PrepareAd(onUserEarnedReward: UserDidEarnReward);

            ToggleLoading = callback;
            ClosePage = callback1;
        }

        public void CreateRewardedInterstitial()
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
                ToggleLoading?.Invoke(false);
                rewardedInterstitialAd.Show();
            }
        }

        private void UserDidEarnReward(RewardItem rewardItem)
        {
            Debug.WriteLine($"User earned {rewardItem.Amount} {rewardItem.Type}.");
            AppSettings.imageCounter = 0; // Reset the counter after showing the ad
            ClosePage?.Invoke();
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
}
