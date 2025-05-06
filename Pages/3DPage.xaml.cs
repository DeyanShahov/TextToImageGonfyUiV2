using TextToImageCore.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class _3D : ContentPage
{
	public _3D()
	{
		InitializeComponent();
	}

    private void Frame_Tapped(object sender, TappedEventArgs e) => TextToImageGonfyUiV2.services.ButtonHandler.SetStyleButton(sender, Style1Types.S3D);

}