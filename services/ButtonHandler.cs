using System.Diagnostics;
using TextToImageCore;
using TextToImageCore.services;

namespace TextToImageGonfyUiV2.services
{
    public static class ButtonHandler
    {
        public static void SetStyleButton(object sender, Style1Types styleType )
        {
            if (sender is Frame tappedFrame && tappedFrame.Content is Label labelInside)
            {
                string buttonName = labelInside.Text;

                // Отпечатване в дебъг конзолата
                Debug.WriteLine($"Натиснат е елемент: {buttonName}");

                string styleName = styleType != Style1Types.S3D  ? styleType.ToString() : "3D";

                bool isSuccess = data.AddStyle(buttonName, styleName); // Добавяне на стил

                // Визуален ефект - превключване на цвета на фона
                if (tappedFrame.BackgroundColor == Colors.Transparent)
                {
                    if (!isSuccess) return;
                    //data.AddStyle(buttonName); // Добавяне на стил

                    // Активиране - задаваме цвят, различен от прозрачен
                    tappedFrame.BackgroundColor = Colors.DarkGray; // Може да избереш друг цвят
                }
                else
                {
                    data.RemoveStyle(buttonName, styleName); // Премахване на стил
                    // Деактивиране - връщаме прозрачен фон
                    tappedFrame.BackgroundColor = Colors.Transparent;
                }
            }
        }
    }
}
