using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;

namespace TextToImageGonfyUiV2
{
    // Модел за един стил
    public class StyleItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        public StyleItem(string name)
        {
            Name = name;
            IsSelected = false;
        }
    }

    // Статичен клас за управление на стиловете
    public static class StylesManager
    {
        // Колекция от всички стилове
        public static ObservableCollection<StyleItem> AllStyles { get; private set; } = new ObservableCollection<StyleItem>();
        
        // Колекция от избрани стилове
        public static ObservableCollection<StyleItem> SelectedStyles { get; private set; } = new ObservableCollection<StyleItem>();

        // Зареждане на стиловете от JSON файла
        public static async Task LoadStylesFromJson()
        {
            try
            {
                // Опит 1: Достъп до файл като embedded resource
                string jsonContent = null;
                
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "TextToImageGonfyUiV2.Resources.Raw.fooocus_styles.json";
                    
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        jsonContent = await reader.ReadToEndAsync();
                        System.Diagnostics.Debug.WriteLine("Файлът е зареден като embedded resource.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Грешка при зареждане като embedded resource: {ex.Message}");
                }
                
                // Опит 2: Ако първият опит е неуспешен, опитваме с FileSystem
                if (string.IsNullOrEmpty(jsonContent))
                {
                    try
                    {
                        using var stream = await FileSystem.OpenAppPackageFileAsync("fooocus_styles.json");
                        using var reader = new StreamReader(stream);
                        jsonContent = await reader.ReadToEndAsync();
                        System.Diagnostics.Debug.WriteLine("Файлът е зареден чрез FileSystem от основната директория.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Грешка при зареждане чрез FileSystem от основната директория: {ex.Message}");
                    }
                }
                
                // Опит 3: Опитваме с директен път в Raw директорията
                if (string.IsNullOrEmpty(jsonContent))
                {
                    try
                    {
                        using var stream = await FileSystem.OpenAppPackageFileAsync("Raw/fooocus_styles.json");
                        using var reader = new StreamReader(stream);
                        jsonContent = await reader.ReadToEndAsync();
                        System.Diagnostics.Debug.WriteLine("Файлът е зареден от Raw директорията.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Грешка при зареждане от Raw директорията: {ex.Message}");
                    }
                }
                
                // Ако всички опити са неуспешни, използваме hardcoded стойности за демонстрация
                if (string.IsNullOrEmpty(jsonContent))
                {
                    System.Diagnostics.Debug.WriteLine("Използване на hardcoded стойности, тъй като файлът не може да бъде зареден.");
                    var defaultStyles = new List<string>
                    {
                        "Fooocus Enhance",
                        "Fooocus Sharp",
                        "Fooocus Masterpiece",
                        "Fooocus Photograph",
                        "sai-3d-model",
                        "sai-anime",
                        "sai-cinematic"
                    };
                    
                    // Изчистваме съществуващите стилове
                    AllStyles.Clear();
                    
                    // Добавяме демо стиловете
                    foreach (var styleName in defaultStyles)
                    {
                        AllStyles.Add(new StyleItem(styleName));
                    }
                    
                    return;
                }
                
                // Десериализираме JSON съдържанието
                var styleNames = JsonSerializer.Deserialize<List<string>>(jsonContent);
                
                // Изчистваме съществуващите стилове
                AllStyles.Clear();
                
                // Добавяме новите стилове
                foreach (var styleName in styleNames)
                {
                    AllStyles.Add(new StyleItem(styleName));
                }
                
                System.Diagnostics.Debug.WriteLine($"Заредени са {AllStyles.Count} стила.");
            }
            catch (Exception ex)
            {
                // Грешка при зареждане - записване в лога
                System.Diagnostics.Debug.WriteLine($"Обща грешка при зареждане на стилове: {ex.Message}");
                
                // Зареждаме няколко демо стила, за да може интерфейсът да работи
                AllStyles.Clear();
                
                var defaultStyles = new List<string>
                {
                    "Демо стил 1",
                    "Демо стил 2",
                    "Демо стил 3"
                };
                
                foreach (var styleName in defaultStyles)
                {
                    AllStyles.Add(new StyleItem(styleName));
                }
            }
        }

        // Превключване на състоянието на стил (избран/неизбран)
        public static void ToggleStyleSelection(StyleItem style)
        {
            style.IsSelected = !style.IsSelected;
            
            if (style.IsSelected)
            {
                // Добавяме към избраните
                if (!SelectedStyles.Any(s => s.Name == style.Name))
                {
                    SelectedStyles.Add(style);
                }
            }
            else
            {
                // Премахваме от избраните
                var styleToRemove = SelectedStyles.FirstOrDefault(s => s.Name == style.Name);
                if (styleToRemove != null)
                {
                    SelectedStyles.Remove(styleToRemove);
                }
            }
        }

        // Получаване на списък с имената на избраните стилове
        public static List<string> GetSelectedStyleNames()
        {
            return SelectedStyles.Select(s => s.Name).ToList();
        }
    }
} 