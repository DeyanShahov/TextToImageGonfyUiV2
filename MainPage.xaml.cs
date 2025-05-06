using CommunityToolkit.Maui.Core.Platform;
using TextToImageGonfyUiV2.Pages;
using TextToImageGonfyUiV2.services;
using TextToImageCore;
using TextToImageCore.services;

#if ANDROID
using Android.App;
using Android.Views.InputMethods;
using Android.Content;
#endif

namespace TextToImageGonfyUiV2
{
    public partial class MainPage : ContentPage
    {
        static string serverAddress = "77.77.134.134:8188";
  
        private bool isGenerating = false;
        private ComfyUiServices comfyUi;
        private IDispatcherTimer autoGenerateTimer; // Timer for auto-generation
        private IDispatcherTimer countdownTimer; // Timer for visual countdown
        private bool isTimerWaitingToTrigger = false; // Flag to track if the timer has been started for the current input session
        private int remainingSeconds = 5; // The countdown value in seconds

        public MainPage()
        {
            InitializeComponent();
            comfyUi = new ComfyUiServices(serverAddress);

            // Initialize the auto-generate timer
            autoGenerateTimer = Dispatcher.CreateTimer();
            autoGenerateTimer.Interval = TimeSpan.FromSeconds(5); // 5 seconds delay
            autoGenerateTimer.Tick += AutoGenerateTimer_Tick;
            autoGenerateTimer.IsRepeating = false; // Timer should tick only once after the interval

            // Initialize the countdown timer (ticks every second)
            countdownTimer = Dispatcher.CreateTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1); // 1 second interval
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.IsRepeating = true; // This timer repeats every second

            // Subscribe to the TextChanged event
            PromptEntry.TextChanged += PromptEntry_TextChanged;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            TimerCountdown.Text = remainingSeconds.ToString();

            if (remainingSeconds <= 0)
            {
                countdownTimer.Stop();
                TimerIndicator.IsVisible = false;
                remainingSeconds = 5; // Reset for next time
            }
        }

        private async void OnGenerateClicked(object sender, EventArgs e)
        {
            if (isGenerating) return;

            HideKeyboard();
            StopTimerAndResetFlag(); // Stop the timer if manual generation is triggered
                                     // 
            await GenerateImageAsync(false);
        }


        private async void RandomPortraitGenerateButton_Clicked(object sender, EventArgs e)
        {
            if (isGenerating) return;
            HideKeyboard();
            StopTimerAndResetFlag();

            await GenerateImageAsync(true);
        }

        private void PromptEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentText = e.NewTextValue;

            // Start timers only if text is entered, generation is not running, AND the timer hasn't been started yet for this session
            if (!string.IsNullOrEmpty(currentText) && !isGenerating && !isTimerWaitingToTrigger)
            {
                // Start the auto-generate timer
                autoGenerateTimer.Start();
                
                // Update the countdown UI
                remainingSeconds = 5;
                TimerCountdown.Text = remainingSeconds.ToString();
                TimerIndicator.IsVisible = true;
                
                // Start or restart the countdown timer
                countdownTimer.Start();
                
                // Set the flag indicating the timer is now running
                isTimerWaitingToTrigger = true;
                System.Diagnostics.Debug.WriteLine("Timer started...");
            }
            else if (string.IsNullOrEmpty(currentText))
            {
                // If text is cleared, stop the timer and reset the flag
                StopTimerAndResetFlag();
                System.Diagnostics.Debug.WriteLine("Text cleared, timer stopped.");
            }
        }

        private async void AutoGenerateTimer_Tick(object sender, EventArgs e)
        {
            // Timer elapsed, reset the flag first
            isTimerWaitingToTrigger = false; // Reset flag *before* potentially starting generation
            System.Diagnostics.Debug.WriteLine("Timer ticked.");

            // Attempt generation if not already generating and there is text
            if (!isGenerating && !string.IsNullOrWhiteSpace(PromptEntry.Text))
            {
                System.Diagnostics.Debug.WriteLine("Auto-generating image after timer...");
                await GenerateImageAsync(false);
            }
        }

        private async Task GenerateImageAsync(bool isRandomPrompt)
        {
#if ANDROID
            if (AppSettings.imageCounter >= AppSettings.numberOfImagesBeforeAdd)
            {
                await Navigation.PushModalAsync(new AdvertisementPage());             
            }
#endif
            if (AppSettings.imageCounter >= AppSettings.numberOfImagesBeforeAdd) return;

            if (isGenerating) return;
            
            StopTimerAndResetFlag();

            string promptText = !isRandomPrompt ? PromptEntry.Text?.Trim() : CreatePortrait.GeneratePortraitPrompt();

            if (string.IsNullOrEmpty(promptText))
            {             
                await DisplayAlert("Грешка", "Моля, въведете текст за генериране на изображение", "OK");
                return;
            
            }
        
            // --- Начало на промените за таймаут ---
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                ResultImage.Opacity = 0.5;
                System.Diagnostics.Debug.WriteLine($"Generating image for prompt: {promptText}");
                isGenerating = true;
                GenerateButton.IsEnabled = false;
                GenerateButton.Text = "...";
                RandomPortreitGenerateButton.IsEnabled = false; // Деактивираме бутона за генериране на произволен портрет
                RandomPortreitGenerateButton.Text = "..."; // Променяме текста на бутона

                // Задаваме таймаут от 10 секунди
                cts.CancelAfter(TimeSpan.FromSeconds(10));

                // Получаваме списък с имената на избраните стилове
                var selectedStyle1Names = data.GetSelectedStyleNames();
                var selectedStyle2Names = StylesManager.GetSelectedStyleNames(); // Предполагам, че имаш и такъв мениджър

                // Извикваме услугата с CancellationToken
                // ВАЖНО: Увери се, че твоят ComfyUiServices.GenerateImageFromText метод приема CancellationToken!
                var images = await comfyUi.GenerateImageFromText(promptText, selectedStyle1Names, selectedStyle2Names, cts.Token);

                System.Diagnostics.Debug.WriteLine($"Generation finished. Images received: {images?.Count ?? 0}");
                if (images != null && images.Count > 0)
                {
                    // Показваме първото генерирано изображение
                    ResultImage.Source = ImageSource.FromStream(() => new MemoryStream(images[0]));
                    ResultImage.Opacity = 1;
                    AppSettings.imageCounter ++; // Увеличаваме брояча на генерираните изображения
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Generation returned no images.");
                    // Може да покажеш съобщение на потребителя тук
                }
            }
            catch (OperationCanceledException)
            {
                // Това се случва, ако cts.CancelAfter() се задейства (таймаут)
                System.Diagnostics.Debug.WriteLine("Image generation timed out after 10 seconds.");
                await DisplayAlert("Таймаут", "Генерирането отне твърде дълго време и беше прекратено.", "OK");
                ResultImage.Source = null; // Изчистваме изображението при таймаут
                ResultImage.Opacity = 1; // Връщаме нормална прозрачност
            }
            catch (Exception ex)
            {
                // Хващаме други грешки (мрежови, от сървъра и т.н.)
                System.Diagnostics.Debug.WriteLine($"An error occurred during image generation: {ex.Message}");
                await DisplayAlert("Грешка", $"Възникна грешка при генериране: {ex.Message}", "OK");
                ResultImage.Source = null; // Изчистваме изображението при грешка
                ResultImage.Opacity = 1;
            }
            finally
            {
                // Този блок се изпълнява ВИНАГИ - при успех, таймаут или грешка
                ResetGenerationState();
                cts.Dispose(); // Освобождаваме ресурсите на CancellationTokenSource
            }
            // --- Край на промените за таймаут ---
            
        }

        private void StopTimerAndResetFlag()
        {
            //autoGenerateTimer.Stop();
            //countdownTimer.Stop();
            isTimerWaitingToTrigger = false;
            TimerIndicator.IsVisible = false;
            remainingSeconds = 5;
            TimerCountdown.Text = remainingSeconds.ToString(); // Нулираме и текста на таймера
        }

        private void ResetGenerationState()
        {
            isGenerating = false;
            GenerateButton.IsEnabled = true;
            GenerateButton.Text = "GO";
            RandomPortreitGenerateButton.IsEnabled = true; // Активираме бутона за генериране на произволен портрет
            RandomPortreitGenerateButton.Text = "RAND"; // Връщаме текста на бутона
            isTimerWaitingToTrigger = false; // Уверяваме се, че и този флаг е нулиран
        }

        private async void HideKeyboard()
        {
            PromptEntry.Unfocus();

            await PromptEntry.HideKeyboardAsync();

#if ANDROID
            var context = Platform.AppContext;
            var inputMethodManager = context?.GetSystemService(Context.InputMethodService) as InputMethodManager;
            if (inputMethodManager != null && Platform.CurrentActivity?.CurrentFocus != null)
            {
                inputMethodManager.HideSoftInputFromWindow(Platform.CurrentActivity.CurrentFocus.WindowToken, HideSoftInputFlags.None);
                // Допълнително може да премахнем фокуса експлицитно
                Platform.CurrentActivity.CurrentFocus.ClearFocus();
            }
#elif IOS
            // На iOS Unfocus() обикновено е достатъчно, но ако има проблеми, може да се добави:
            //UIKit.UIApplication.SharedApplication.KeyWindow?.EndEditing(true);
#endif
        }

    }
}
