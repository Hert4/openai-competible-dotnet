using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DocReader.App;

// === CAU HINH ===
const string API_KEY = "YOUR_API_KEY_HERE";
const string BASE_URL = "https://YOUR_BASE_URL/v1/chat/completions";
const string MODEL = "gpt-4.1";

// === DOC FILE DOCX ===
string docxPath = args.Length > 0
    ? args[0]
    : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AVA_Tuyen_Dung_Chi_Tiet_Skills_Tools.docx");

docxPath = Path.GetFullPath(docxPath);

Console.WriteLine($"Doc file: {docxPath}");
Console.WriteLine(new string('=', 60));

string fileContent;
try
{
    fileContent = DocxReader.ReadTextWithTables(docxPath);
    Console.WriteLine($"Da doc thanh cong! ({fileContent.Length} ky tu)");
    Console.WriteLine(new string('-', 60));

    // In preview 500 ky tu dau
    var preview = fileContent.Length > 500 ? fileContent[..500] + "\n..." : fileContent;
    Console.WriteLine("Preview noi dung:");
    Console.WriteLine(preview);
    Console.WriteLine(new string('=', 60));
}
catch (Exception ex)
{
    Console.WriteLine($"Loi doc file: {ex.Message}");
    return;
}

// === GUI LEN API ===
if (API_KEY == "YOUR_API_KEY_HERE" || BASE_URL.Contains("YOUR_BASE_URL"))
{
    Console.WriteLine("\n[SKIP] Chua cau hinh API_KEY va BASE_URL. Chi test doc file.");
    Console.WriteLine("Hay cap nhat API_KEY va BASE_URL trong Program.cs de gui len API.");
    return;
}

using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
client.Timeout = TimeSpan.FromMinutes(2);

var requestBody = new
{
    model = MODEL,
    messages = new object[]
    {
        new
        {
            role = "system",
            content = "Ban la tro ly HR chuyen phan tich tai lieu tuyen dung. Hay doc noi dung va tra ve thong tin co cau truc."
        },
        new
        {
            role = "user",
            content = $"Phan tich noi dung tai lieu tuyen dung sau va tra ve thong tin co cau truc:\n\n{fileContent}"
        }
    },
    response_format = new
    {
        type = "json_schema",
        json_schema = new
        {
            name = "recruitment_analysis",
            strict = true,
            schema = new
            {
                type = "object",
                properties = new
                {
                    data = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                ViTri = new { type = "string" },
                                MoTa = new { type = "string" },
                                Skills = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                Tools = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                YeuCauKinhNghiem = new { type = "string" },
                                MucLuong = new { type = "string" }
                            },
                            required = new[] { "ViTri", "MoTa", "Skills", "Tools", "YeuCauKinhNghiem", "MucLuong" },
                            additionalProperties = false
                        }
                    }
                },
                required = new[] { "data" },
                additionalProperties = false
            }
        }
    }
};

var json = JsonSerializer.Serialize(requestBody);
var content = new StringContent(json, Encoding.UTF8, "application/json");

Console.WriteLine("\nDang gui request len API...");

try
{
    var response = await client.PostAsync(BASE_URL, content);
    var responseBody = await response.Content.ReadAsStringAsync();

    Console.WriteLine($"Status: {response.StatusCode}");
    Console.WriteLine($"Response:");

    // Pretty print JSON response
    try
    {
        var jsonDoc = JsonDocument.Parse(responseBody);
        Console.WriteLine(JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true }));
    }
    catch
    {
        Console.WriteLine(responseBody);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Loi khi goi API: {ex.Message}");
}