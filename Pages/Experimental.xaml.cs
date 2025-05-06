using TextToImageCore.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class Experimental : ContentPage
{
	public Experimental()
	{
		InitializeComponent();
	}

    private void Frame_Tapped(object sender, TappedEventArgs e) => TextToImageGonfyUiV2.services.ButtonHandler.SetStyleButton(sender, Style1Types.Experimental);
}