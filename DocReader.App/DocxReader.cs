using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocReader.App;

public class DocxReader
{
    /// <summary>
    /// Doc noi dung text tu file .docx, tra ve string.
    /// </summary>
    public static string ReadText(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Khong tim thay file: {filePath}");

        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            sb.AppendLine(paragraph.InnerText);
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Doc noi dung text tu file .docx theo tung paragraph, tra ve danh sach cac dong.
    /// </summary>
    public static List<string> ReadParagraphs(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Khong tim thay file: {filePath}");

        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null)
            return new List<string>();

        return body.Elements<Paragraph>()
            .Select(p => p.InnerText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();
    }

    /// <summary>
    /// Doc noi dung text tu file .docx kem thong tin table (neu co).
    /// </summary>
    public static string ReadTextWithTables(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Khong tim thay file: {filePath}");

        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null)
            return string.Empty;

        var sb = new StringBuilder();

        foreach (var element in body.ChildElements)
        {
            if (element is Paragraph para)
            {
                var text = para.InnerText;
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }
            else if (element is Table table)
            {
                sb.AppendLine("[TABLE]");
                foreach (var row in table.Elements<TableRow>())
                {
                    var cells = row.Elements<TableCell>()
                        .Select(c => c.InnerText.Trim());
                    sb.AppendLine("| " + string.Join(" | ", cells) + " |");
                }
                sb.AppendLine("[/TABLE]");
            }
        }

        return sb.ToString().TrimEnd();
    }
}