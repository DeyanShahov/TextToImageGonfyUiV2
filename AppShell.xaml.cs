using TextToImageGonfyUiV2.Pages;

namespace TextToImageGonfyUiV2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Регистрирам рутинга за новата страница
            Routing.RegisterRoute(nameof(ButtonCollectionPage), typeof(ButtonCollectionPage));
            Routing.RegisterRoute(nameof(Photography), typeof(Photography));
            Routing.RegisterRoute(nameof(Painting), typeof(Painting));
            Routing.RegisterRoute(nameof(Illustration), typeof(Illustration));
            Routing.RegisterRoute(nameof(Drawing), typeof(Drawing));
            Routing.RegisterRoute(nameof(_3D), typeof(_3D));
            Routing.RegisterRoute(nameof(Vector), typeof(Vector));
            Routing.RegisterRoute(nameof(Design), typeof(Design));
            Routing.RegisterRoute(nameof(Fashion), typeof(Fashion));
            Routing.RegisterRoute(nameof(ART), typeof(ART));
            Routing.RegisterRoute(nameof(Craft), typeof(Craft));
            Routing.RegisterRoute(nameof(Experimental), typeof(Experimental));
            Routing.RegisterRoute(nameof(TextToImageGonfyUiV2.Pages.Settings), typeof(TextToImageGonfyUiV2.Pages.Settings));

        }

        private void Secret_label_function_Tapped(object sender, TappedEventArgs e)
        {
            SettingsOption.IsVisible = !SettingsOption.IsVisible;
        }
    }
}
