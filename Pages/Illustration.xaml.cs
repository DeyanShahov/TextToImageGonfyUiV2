using TextToImageGonfyUiV2.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class Illustration : ContentPage
{
	public Illustration()
	{
		InitializeComponent();
	}

    private void Frame_Tapped(object sender, TappedEventArgs e) => TextToImageGonfyUiV2.services.ButtonHandler.SetStyleButton(sender, Style1Types.Illustration);

}