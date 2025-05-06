using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System.Collections.Specialized;
using System.Diagnostics;
using TextToImageCore;

namespace TextToImageGonfyUiV2
{
    public partial class ButtonCollectionPage : ContentPage
    {
        private const int ButtonHeight = 40;
        private const int ButtonMinWidth = 120;
        private const float ButtonFlex = 0.3f; // Приблизителен процент от ширината на контейнера

        public ButtonCollectionPage()
        {
            InitializeComponent();

            // Абонираме се за промени в колекцията от избрани стилове
            StylesManager.SelectedStyles.CollectionChanged += SelectedStyles_CollectionChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Зареждаме стиловете, ако колекцията е празна
            if (StylesManager.AllStyles.Count == 0)
            {
                try
                {
                    await LoadStyles();
                }
                catch (Exception ex)
                {
                    // Показваме грешката на потребителя
                    await DisplayAlert("Грешка при зареждане на стилове", 
                        $"Детайли: {ex.Message}", "OK");
                }
            }
            else
            {
                // Обновяваме брояча на избрани стилове
                UpdateSelectedCounter();
            }
        }

        private async Task LoadStyles()
        {
            try
            {
                // Показваме индикатор за зареждане
                await DisplayAlert("Информация", "Зареждане на стилове...", "ОК");

                // Зареждаме стиловете от JSON файла
                StylesManager.LoadStyles();

                // Ако няма заредени стилове, показваме съобщение за грешка
                if (StylesManager.AllStyles.Count == 0)
                {
                    await DisplayAlert("Грешка", "Не можаха да бъдат заредени стилове.", "ОК");
                    return;
                }

                // Създаваме бутони за всеки стил
                CreateStyleButtons();

                // Обновяваме брояча на избрани стилове
                UpdateSelectedCounter();
            }
            catch (Exception ex)
            {
                // Показваме детайлна информация за грешката
                Debug.WriteLine($"Детайлна грешка при зареждане на стилове: {ex}");
                throw; // Предаваме грешката нагоре, за да бъде хваната в OnAppearing
            }
        }

        private void CreateStyleButtons()
        {
            // Изчистваме предишни бутони, ако има такива
            StylesContainer.Clear();

            // Създаваме нови бутони за стиловете
            foreach (var style in StylesManager.AllStyles)
            {
                var button = new Button
                {
                    Text = style.Name,
                    HeightRequest = ButtonHeight,
                    BackgroundColor = style.IsSelected ? Color.FromArgb("#007AFF") : Color.FromArgb("#333333"),
                    TextColor = Colors.White,
                    FontSize = 14,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 0),
                    MinimumWidthRequest = ButtonMinWidth,
                    BindingContext = style,
                    HorizontalOptions = LayoutOptions.Start
                };

                // Задаваме FlexLayout размера
                FlexLayout.SetBasis(button, new FlexBasis(ButtonFlex, true));

                // Добавяме обработчик на събитие за натискане на бутона
                button.Clicked += StyleButton_Clicked;

                // Добавяме бутона към контейнера
                StylesContainer.Add(button);
            }
        }

        private void StyleButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is StyleItem style)
            {
                // Превключваме състоянието на стила (избран/неизбран)
                StylesManager.ToggleStyleSelection(style);

                // Променяме външния вид на бутона според новото състояние
                button.BackgroundColor = style.IsSelected ? Color.FromArgb("#007AFF") : Color.FromArgb("#333333");
            }
        }

        private void SelectedStyles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Обновяваме брояча и видимостта на бутона за изчистване при промяна в колекцията
            UpdateSelectedCounter();
        }

        private void UpdateSelectedCounter()
        {
            // Обновяваме текста с броя избрани стилове
            SelectedCountLabel.Text = $"Избрани: {StylesManager.SelectedStyles.Count}";

            // Показваме или скриваме бутона за изчистване в зависимост от броя избрани стилове
            ClearSelectionButton.IsVisible = StylesManager.SelectedStyles.Count > 0;
        }

        private void ClearSelectionButton_Clicked(object sender, EventArgs e)
        {
            // Записваме всички стилове, които са били избрани
            var selectedStyles = StylesManager.SelectedStyles.ToList();

            // Изчистваме колекцията от избрани стилове
            StylesManager.SelectedStyles.Clear();

            // Обновяваме визуалното състояние на бутоните
            foreach (var child in StylesContainer.Children)
            {
                if (child is Button button && button.BindingContext is StyleItem style)
                {
                    style.IsSelected = false;
                    button.BackgroundColor = Color.FromArgb("#333333");
                }
            }

            // Обновяваме брояча
            UpdateSelectedCounter();
        }

        private async void ApplyButton_Clicked(object sender, EventArgs e)
        {
            var selectedCount = StylesManager.SelectedStyles.Count;
            
            if (selectedCount == 0)
            {
                await DisplayAlert("Информация", "Не сте избрали нито един стил.", "ОК");
                return;
            }

            // Получаваме списък с имената на избраните стилове
            var selectedStyleNames = StylesManager.GetSelectedStyleNames();
            
            // Съобщаваме броя избрани стилове
            await DisplayAlert("Избрани стилове", $"Избрани са {selectedCount} стила.", "ОК");
            
            // Тук можете да направите допълнителни действия с избраните стилове
            // Например, да ги предадете на MainPage или да ги използвате по друг начин
        }
    }
} 