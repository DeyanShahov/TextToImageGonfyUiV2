namespace TextToImageGonfyUiV2.Pages;
using TextToImageCore.services;

public partial class Painting : ContentPage
{
	public Painting()
	{
		InitializeComponent();
	}

    private void Frame_Tapped(object sender, TappedEventArgs e) => TextToImageGonfyUiV2.services.ButtonHandler.SetStyleButton(sender, Style1Types.Painting);
}