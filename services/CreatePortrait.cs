using System.Text.Json;

namespace TextToImageGonfyUiV2.services
{
    public class CreatePortrait
    {
        private static Random random = new Random();
        
        public static async Task<string> GeneratePortraitPrompt()
        {
            var portraitData = await LoadPortraitData();
            
            // Избираме произволен пол
            string gender = GetRandomItem(portraitData["gender_list"]);
            
            // Списък за съхранение на избраните характеристики
            List<string> portraitTraits = new List<string>();
            portraitTraits.Add(gender);
            
            // Избираме националност
            portraitTraits.Add(GetRandomItem(portraitData["nationality_list"]));
            
            // Избираме тип тяло
            portraitTraits.Add(GetRandomItem(portraitData["body_type_list"]));
            
            // Избираме форма на лицето
            portraitTraits.Add($"with {GetRandomItem(portraitData["face_shape_list"])} face");
            
            // Избираме цвят на очите
            portraitTraits.Add($"with {GetRandomItem(portraitData["eyes_color_list"])} eyes");
            
            // Избираме прическа и цвят на косата
            string hairColor = GetRandomItem(portraitData["hair_color_list"]);
            string hairStyle = GetRandomItem(portraitData["hair_style_list"]);
            portraitTraits.Add($"with {hairColor} {hairStyle}");
            
            // Избираме изражение на лицето
            portraitTraits.Add($"with {GetRandomItem(portraitData["face_expression_list"])} expression");
            
            // Ако полът е мъж, добавяме брада (50% шанс)
            if (gender == "Man" && random.Next(2) == 0)
            {
                portraitTraits.Add($"with {GetRandomItem(portraitData["beard_list"])}");
            }
            
            // Избираме поза
            portraitTraits.Add($"in {GetRandomItem(portraitData["model_pose_list"])}");
            
            // Избираме тип кадър
            portraitTraits.Add($"{GetRandomItem(portraitData["shot_list"])}");
            
            // Избираме осветление
            string lightType = GetRandomItem(portraitData["light_type_list"]);
            string lightDirection = GetRandomItem(portraitData["light_direction_list"]);
            portraitTraits.Add($"with {lightType} from {lightDirection}");
            
            // Създаваме финалното описание
            string portraitDescription = string.Join(", ", portraitTraits);
            
            return portraitDescription;
        }
        
        private static async Task<Dictionary<string, JsonElement>> LoadPortraitData()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("portrait_prompt.json");
            using var reader = new StreamReader(stream);
            string jsonContent = await reader.ReadToEndAsync();

            //using (JsonDocument document = JsonDocument.Parse(jsonContent))
            //{
            //    var rootElement = document.RootElement;
            //    Dictionary<string, JsonElement> result = new Dictionary<string, JsonElement>();

            //    foreach (var property in rootElement.EnumerateObject())
            //    {
            //        result.Add(property.Name, property.Value);
            //    }

            //    return result;
            //}

            using var document = JsonDocument.Parse(jsonContent);

            var dict = document.RootElement
                               .EnumerateObject()
                               .ToDictionary(p => p.Name, p => p.Value.Clone());
            return dict;
        }
        
        private static string GetRandomItem(JsonElement array)
        {
            int count = array.GetArrayLength();
            int index = random.Next(count);
            return array[index].GetString();
        }
    }
}
