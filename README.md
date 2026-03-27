# DocReader

C# console app doc file `.docx` va gui noi dung len OpenAI-compatible API de phan tich co cau truc (structured output).

## Cau truc project

| File | Mo ta |
|------|-------|
| `DocReader.App/DocxReader.cs` | Thu vien doc file `.docx` - 3 methods: `ReadText`, `ReadParagraphs`, `ReadTextWithTables` |
| `DocReader.App/Program.cs` | Chuong trinh chinh: doc file + gui API |
| `DocReader.Tests/DocxReaderTests.cs` | 14 test cases cho chuc nang doc file |
| `AVA_Tuyen_Dung_Chi_Tiet_Skills_Tools.docx` | File mau de test |

## Cach chay

### 1. Cai dat

```bash
cd DocReader
dotnet restore
```

### 2. Chay test

```bash
dotnet test
```

### 3. Chay app

Mo `DocReader.App/Program.cs`, thay 2 dong sau bang API key va base URL cua ban:

```csharp
const string API_KEY = "YOUR_API_KEY_HERE";
const string BASE_URL = "https://YOUR_BASE_URL/v1/chat/completions";
```

Sau do chay:

```bash
dotnet run --project DocReader.App
```

Doc file khac:

```bash
dotnet run --project DocReader.App -- "duong/dan/toi/file.docx"
```

## Yeu cau

- .NET 8.0+
- NuGet: `DocumentFormat.OpenXml`
