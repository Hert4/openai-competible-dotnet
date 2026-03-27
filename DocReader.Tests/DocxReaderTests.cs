using DocReader.App;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocReader.Tests;

public class DocxReaderTests : IDisposable
{
    private readonly string _tempDir;

    public DocxReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DocReaderTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTestDocx(string fileName, Action<Body> configureBody)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = new Body();
        configureBody(body);
        mainPart.Document.Append(body);
        mainPart.Document.Save();
        return filePath;
    }

    // === ReadText Tests ===

    [Fact]
    public void ReadText_SimpleDocument_ReturnsContent()
    {
        var path = CreateTestDocx("simple.docx", body =>
        {
            body.Append(new Paragraph(new Run(new Text("Dong thu nhat"))));
            body.Append(new Paragraph(new Run(new Text("Dong thu hai"))));
        });

        var result = DocxReader.ReadText(path);

        Assert.Contains("Dong thu nhat", result);
        Assert.Contains("Dong thu hai", result);
    }

    [Fact]
    public void ReadText_EmptyDocument_ReturnsEmpty()
    {
        var path = CreateTestDocx("empty.docx", body => { });

        var result = DocxReader.ReadText(path);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReadText_FileNotFound_ThrowsException()
    {
        var fakePath = Path.Combine(_tempDir, "khong_ton_tai.docx");

        Assert.Throws<FileNotFoundException>(() => DocxReader.ReadText(fakePath));
    }

    [Fact]
    public void ReadText_MultiParagraph_PreservesOrder()
    {
        var path = CreateTestDocx("multi.docx", body =>
        {
            for (int i = 1; i <= 5; i++)
                body.Append(new Paragraph(new Run(new Text($"Paragraph {i}"))));
        });

        var result = DocxReader.ReadText(path);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(5, lines.Length);
        Assert.Equal("Paragraph 1", lines[0]);
        Assert.Equal("Paragraph 5", lines[4]);
    }

    // === ReadParagraphs Tests ===

    [Fact]
    public void ReadParagraphs_ReturnsNonEmptyParagraphs()
    {
        var path = CreateTestDocx("paragraphs.docx", body =>
        {
            body.Append(new Paragraph(new Run(new Text("Co noi dung"))));
            body.Append(new Paragraph()); // empty paragraph
            body.Append(new Paragraph(new Run(new Text("Cung co noi dung"))));
        });

        var result = DocxReader.ReadParagraphs(path);

        Assert.Equal(2, result.Count);
        Assert.Equal("Co noi dung", result[0]);
        Assert.Equal("Cung co noi dung", result[1]);
    }

    [Fact]
    public void ReadParagraphs_EmptyDocument_ReturnsEmptyList()
    {
        var path = CreateTestDocx("empty_para.docx", body => { });

        var result = DocxReader.ReadParagraphs(path);

        Assert.Empty(result);
    }

    [Fact]
    public void ReadParagraphs_FileNotFound_ThrowsException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            DocxReader.ReadParagraphs(Path.Combine(_tempDir, "no.docx")));
    }

    // === ReadTextWithTables Tests ===

    [Fact]
    public void ReadTextWithTables_ParagraphsOnly_ReturnsText()
    {
        var path = CreateTestDocx("text_only.docx", body =>
        {
            body.Append(new Paragraph(new Run(new Text("Tieu de"))));
            body.Append(new Paragraph(new Run(new Text("Noi dung"))));
        });

        var result = DocxReader.ReadTextWithTables(path);

        Assert.Contains("Tieu de", result);
        Assert.Contains("Noi dung", result);
        Assert.DoesNotContain("[TABLE]", result);
    }

    [Fact]
    public void ReadTextWithTables_WithTable_IncludesTableMarkers()
    {
        var path = CreateTestDocx("with_table.docx", body =>
        {
            body.Append(new Paragraph(new Run(new Text("Truoc bang"))));

            var table = new Table();
            var row1 = new TableRow();
            row1.Append(new TableCell(new Paragraph(new Run(new Text("Header1")))));
            row1.Append(new TableCell(new Paragraph(new Run(new Text("Header2")))));
            table.Append(row1);

            var row2 = new TableRow();
            row2.Append(new TableCell(new Paragraph(new Run(new Text("Value1")))));
            row2.Append(new TableCell(new Paragraph(new Run(new Text("Value2")))));
            table.Append(row2);

            body.Append(table);
            body.Append(new Paragraph(new Run(new Text("Sau bang"))));
        });

        var result = DocxReader.ReadTextWithTables(path);

        Assert.Contains("Truoc bang", result);
        Assert.Contains("[TABLE]", result);
        Assert.Contains("| Header1 | Header2 |", result);
        Assert.Contains("| Value1 | Value2 |", result);
        Assert.Contains("[/TABLE]", result);
        Assert.Contains("Sau bang", result);
    }

    [Fact]
    public void ReadTextWithTables_EmptyDocument_ReturnsEmpty()
    {
        var path = CreateTestDocx("empty_table.docx", body => { });

        var result = DocxReader.ReadTextWithTables(path);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReadTextWithTables_FileNotFound_ThrowsException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            DocxReader.ReadTextWithTables(Path.Combine(_tempDir, "no.docx")));
    }

    // === Test voi file thuc te ===

    [Fact]
    public void ReadText_RealDocxFile_ReturnsNonEmptyContent()
    {
        // Tim file AVA_Tuyen_Dung tu project root
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var realFile = Path.Combine(projectRoot, "AVA_Tuyen_Dung_Chi_Tiet_Skills_Tools.docx");

        if (!File.Exists(realFile))
        {
            // Skip test neu khong co file thuc te
            return;
        }

        var result = DocxReader.ReadText(realFile);

        Assert.False(string.IsNullOrWhiteSpace(result), "File thuc te phai co noi dung");
        Assert.True(result.Length > 10, $"Noi dung qua ngan: {result.Length} ky tu");
    }

    [Fact]
    public void ReadTextWithTables_RealDocxFile_ReturnsContent()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var realFile = Path.Combine(projectRoot, "AVA_Tuyen_Dung_Chi_Tiet_Skills_Tools.docx");

        if (!File.Exists(realFile))
            return;

        var result = DocxReader.ReadTextWithTables(realFile);

        Assert.False(string.IsNullOrWhiteSpace(result));
        Console.WriteLine($"Noi dung file thuc te ({result.Length} ky tu):");
        Console.WriteLine(result[..Math.Min(result.Length, 1000)]);
    }

    [Fact]
    public void ReadParagraphs_RealDocxFile_ReturnsMultipleParagraphs()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var realFile = Path.Combine(projectRoot, "AVA_Tuyen_Dung_Chi_Tiet_Skills_Tools.docx");

        if (!File.Exists(realFile))
            return;

        var result = DocxReader.ReadParagraphs(realFile);

        Assert.True(result.Count > 0, "File thuc te phai co it nhat 1 paragraph");
        Console.WriteLine($"So paragraph: {result.Count}");
        foreach (var p in result.Take(10))
            Console.WriteLine($"  - {p[..Math.Min(p.Length, 100)]}");
    }
}