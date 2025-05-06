using TextToImageCore.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class ART : ContentPage
{
	public ART()
	{
		InitializeComponent();
	}

    private void Frame_Tapped(object sender, TappedEventArgs e) => TextToImageGonfyUiV2.services.ButtonHandler.SetStyleButton(sender, Style1Types.ART);
}