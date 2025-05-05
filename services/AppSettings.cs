namespace TextToImageGonfyUiV2.services
{
    internal class AppSettings
    {
        //private static string style1 = "Anime"; // Default style of the image
        //private static string style2 = "Realism"; // Default style of the image
        public static string styleAnime = "aniversePonyXL_v40.safetensors"; // Default style of the image
        public static string styleRealism = "Juggernaut-X-RunDiffusion-NSFW.safetensors"; // Default style of the image


        public static int imageCounter = 0; // Counter for the number of images generated
        public static int numberOfImagesBeforeAdd = 5; // Counter for the number of images generated before an ad is shown
        public static bool isAnimeStyle = true; // Flag to check style to set

        //public static string currentStyle = style1; // Current style of the image


    }
}
