using System.Xml;
using opp_lab_2_zirolio_ctrl.Services.Export;

namespace opp_lab_2_zirolio_ctrl.Tests;

public class XsltTransformerServiceTests
{
    private readonly XsltTransformerService _svc = new();

    [Fact]
    public void TransformToString_ProducesHtmlWithAllStudents()
    {
        using var xsl = new TempFile(TestData.SimpleXsl, ".xsl");

        var html = _svc.TransformToString(TestData.SampleXml, xsl.FilePath);

        Assert.Contains("<html", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Чах Максим Олександрович", html);   // кириллица не портится
        Assert.Contains("Лисенко Ірина Андріївна", html);
        Assert.Equal(6, html.Split("<li>").Length - 1);
    }

    [Fact]
    public void TransformToString_HasNoBom()
    {
        using var xsl = new TempFile(TestData.SimpleXsl, ".xsl");
        var html = _svc.TransformToString("<Dormitory/>", xsl.FilePath);
        Assert.False(html.StartsWith('\uFEFF'));
    }

    [Fact]
    public void TransformToString_MissingXsl_ThrowsFileNotFound()
    {
        var ex = Assert.Throws<FileNotFoundException>(() => _svc.TransformToString("<a/>", "missing.xsl"));
        Assert.Equal("missing.xsl", ex.FileName);
    }

    [Fact]
    public void TransformToString_InvalidXml_ThrowsXmlException()
    {
        using var xsl = new TempFile(TestData.SimpleXsl, ".xsl");
        Assert.ThrowsAny<XmlException>(() => _svc.TransformToString("<Dormitory><Student>", xsl.FilePath));
    }

    [Fact]
    public void TransformToHtml_WritesOutputFile()
    {
        using var xml = new TempFile(TestData.SampleXml);
        using var xsl = new TempFile(TestData.SimpleXsl, ".xsl");
        var output = Path.Combine(Path.GetTempPath(), $"lab2_out_{Guid.NewGuid():N}.html");

        try
        {
            _svc.TransformToHtml(xml.FilePath, xsl.FilePath, output);

            Assert.True(File.Exists(output));
            Assert.Contains("Бойко Дмитро Сергійович", File.ReadAllText(output));
        }
        finally
        {
            File.Delete(output);
        }
    }
}
