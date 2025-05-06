using TextToImageCore.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class CustomButtonsElement : ContentPage
{
    public static readonly BindableProperty ButtonTextProperty =
           BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(CustomButtonsElement), string.Empty);

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public event EventHandler Clicked;

    public CustomButtonsElement()
	{
		InitializeComponent();
	}

    private void Frame_Tapped(object sender, TappedEventArgs e) => TextToImageGonfyUiV2.services.ButtonHandler.SetStyleButton(sender, Style1Types.Vector);
    
    //private void Frame_Tapped(object sender, TappedEventArgs e) => Clicked?.Invoke(this, e);

}