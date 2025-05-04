using Microsoft.Maui.Handlers;
using System.Diagnostics;
using TextToImageGonfyUiV2.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class Photography : ContentPage
{
	public Photography()
	{
		InitializeComponent();
	}

    private void Frame_Tapped(object sender, TappedEventArgs e) => TextToImageGonfyUiV2.services.ButtonHandler.SetStyleButton(sender, Style1Types.Photography);


}