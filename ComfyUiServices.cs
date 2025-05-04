using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace TextToImageGonfyUiV2
{
    public class ComfyUiServices
    {
        static HttpClient http = new HttpClient();
        static string clientId = Guid.NewGuid().ToString();
        static string serverAddress = String.Empty;

        public ComfyUiServices(string url)
        {
            serverAddress = url;
        }

        public async Task<List<byte[]>> GenerateImageFromText(string promptText ,List<string> style1settings, List<string> style2settings, CancellationToken cancellationToken = default)
        {
            // Read prompt.json file from the Resources/raw folder
            using var stream = await FileSystem.OpenAppPackageFileAsync("prompt2.json");
            using var reader = new StreamReader(stream);
            string promptJsonText = await reader.ReadToEndAsync();

            var promptJson = JsonNode.Parse(promptJsonText);

            // Инжектираме seed и потребителски текст
            var rand = new Random();
            long seed = rand.NextInt64(1, 999999999);
            promptJson["3"]["inputs"]["seed"] = seed;
            //promptJson["6"]["inputs"]["text"] = promptText;
            if(!String.IsNullOrEmpty(promptJsonText)) promptJson["29"]["inputs"]["text"] = promptText;
            Debug.WriteLine(promptJson["29"]["inputs"]["text"]);
            //promptJson["7"]["inputs"]["text"] = "text, watermark, nsfw, nude, nudity, explicit content, sexual acts, pornographic, adult themes, graphic nudity, lewd, suggestive poses, inappropriate content, profanity, fetish, obscene, sexualized imagery, worst quality, low quality, normal quality, lowres, blurry, bad anatomy, bad hands, missing fingers, extra digit, fewer digits, deformed, disfigured, mutation, mutated hands, fused fingers, long neck, cropped, jpeg artifacts, signature, watermark, text, error, ugly, duplicate, morbid, mutilated, out of frame, extra limbs, bad proportions, poorly drawn hands, poorly drawn face, grainy, oversaturated, undersaturated, overexposed, underexposed, grayscale, bw, bad photo, bad photography, bad art, bad composition";

            var selectedStyle = String.Join(',', style2settings);
            if(selectedStyle.Length > 0) promptJson["15"]["inputs"]["select_styles"] = selectedStyle;
            Debug.WriteLine(promptJson["15"]["inputs"]["select_styles"]);




            for (int i = 0; i < style1settings.Count; i++)
            {
                if (style1settings[i] != null)
                {
                    promptJson["22"]["inputs"][$"style{i + 1}"] = style1settings[i];
                }
            }
            Debug.WriteLine(promptJson["22"]["inputs"]);


            return await GenerateImageAsync(promptJson, cancellationToken);
        }

        static async Task<List<byte[]>> GenerateImageAsync(JsonNode prompt, CancellationToken cancellationToken = default)
        {
            var promptId = await QueuePromptAsync(prompt);

            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);

            byte[] buffer = new byte[8192];
            List<byte[]> outputImages = new();

            while (true)
            {
                var result = await ws.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var json = JsonNode.Parse(msg);

                if (json["type"]?.ToString() == "executing")
                {
                    var data = json["data"];
                    if (data?["node"] == null && data?["prompt_id"]?.ToString() == promptId)
                        break;
                }
            }

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", cancellationToken);

            var history = await GetHistoryAsync(promptId, cancellationToken);
            foreach (var node in history["outputs"].AsObject())
            {
                var images = node.Value["images"];
                if (images is null) continue;

                foreach (var image in images.AsArray())
                {
                    string filename = image["filename"]?.ToString();
                    string subfolder = image["subfolder"]?.ToString();
                    string type = image["type"]?.ToString();

                    var imageData = await GetImageAsync(filename, subfolder, type, cancellationToken);
                    outputImages.Add(imageData);
                }
            }

            return outputImages;
        }

        static async Task<string> QueuePromptAsync(JsonNode prompt, CancellationToken cancellationToken = default)
        {
            var data = new
            {
                prompt = prompt,
                client_id = clientId
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await http.PostAsync($"http://{serverAddress}/prompt", content, cancellationToken);
            // отпечатване на респонсе на дебуг конзолата
            Debug.WriteLine($"Response: {response.StatusCode}");
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = JsonNode.Parse(json);
            return parsed["prompt_id"]?.ToString();
        }

        static async Task<JsonNode> GetHistoryAsync(string promptId, CancellationToken cancellationToken = default)
        {
            var json = await http.GetStringAsync($"http://{serverAddress}/history/{promptId}", cancellationToken);
            return JsonNode.Parse(json)[promptId];
        }

        static async Task<byte[]> GetImageAsync(string filename, string subfolder, string type, CancellationToken cancellationToken = default)
        {
            var url = $"http://{serverAddress}/view?filename={Uri.EscapeDataString(filename)}&subfolder={Uri.EscapeDataString(subfolder)}&type={Uri.EscapeDataString(type)}";
            return await http.GetByteArrayAsync(url);
        }

        //public async Task<Dictionary<string, List<byte[]>>> GetImagesAsync(string prompt, CancellationToken cancellationToken = default)
        //{
        //    // 1. Извикваме REST endpoint за queue_prompt → взимаме prompt_id
        //    var promptId = await QueuePromptAsync(prompt, cancellationToken);

        //    // Резултатна структура: nodeName → списък от byte[] (изображения)
        //    var outputImages = new Dictionary<string, List<byte[]>>();
        //    string currentNode = null;

        //    // 2. Създаваме и отваряме WebSocket
        //    using var ws = new ClientWebSocket();
        //    await ws.ConnectAsync(_wsUri, cancellationToken);

        //    var buffer = new byte[8192];
        //    while (true)
        //    {
        //        // 3. Четем фрейм от WebSocket
        //        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

        //        if (result.MessageType == WebSocketMessageType.Close)
        //        {
        //            // Сървърът затваря конекцията
        //            break;
        //        }

        //        if (result.MessageType == WebSocketMessageType.Text)
        //        {
        //            // 4а. Ако е текст, десериализираме JSON
        //            var jsonText = Encoding.UTF8.GetString(buffer, 0, result.Count);
        //            using var doc = JsonDocument.Parse(jsonText);
        //            var root = doc.RootElement;

        //            // Разглеждаме type и prompt_id
        //            if (root.GetProperty("type").GetString() == "executing")
        //            {
        //                var data = root.GetProperty("data");
        //                if (data.GetProperty("prompt_id").GetString() == promptId)
        //                {
        //                    // Ако node == null, значи свършваме
        //                    if (data.GetProperty("node").ValueKind == JsonValueKind.Null)
        //                    {
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        currentNode = data.GetProperty("node").GetString();
        //                    }
        //                }
        //            }
        //        }
        //        else if (result.MessageType == WebSocketMessageType.Binary)
        //        {
        //            // 4б. Ако е двоичен поток, значи имаме image chunk
        //            if (currentNode == "save_image_websocket_node")
        //            {
        //                // Отрязваме първите 8 байта (header) както в Python
        //                var imageData = new byte[result.Count - 8];
        //                Array.Copy(buffer, 8, imageData, 0, imageData.Length);

        //                if (!outputImages.TryGetValue(currentNode, out var list))
        //                {
        //                    list = new List<byte[]>();
        //                    outputImages[currentNode] = list;
        //                }
        //                list.Add(imageData);
        //            }
        //        }
        //    }

        //    // 5. Затваряме WebSocket
        //    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);

        //    return outputImages;
        //}
    
    }
}
