using TextToImageGonfyUiV2.services;

namespace TextToImageGonfyUiV2.Pages;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
	}

    override protected void OnAppearing()
    {
        base.OnAppearing();

        ShowNumbersBeforeAdd();
        ModelStyle.Text = AppSettings.styleAnime;
    }

    private void UpButton_Clicked(object sender, EventArgs e)
    {
        AppSettings.numberOfImagesBeforeAdd++;
        ShowNumbersBeforeAdd();

    }

    private void DownButton_Clicked(object sender, EventArgs e)
    {
        AppSettings.numberOfImagesBeforeAdd--;
        ShowNumbersBeforeAdd();

    }

    private void ShowNumbersBeforeAdd()
    {
        Broqch.Text = AppSettings.numberOfImagesBeforeAdd.ToString();
    }

    private void SetCheckpoint_Clicked(object sender, EventArgs e)
    {
        AppSettings.isAnimeStyle = !AppSettings.isAnimeStyle;
        ModelStyle.Text = AppSettings.isAnimeStyle ? AppSettings.styleAnime : AppSettings.styleRealism;
    }
}