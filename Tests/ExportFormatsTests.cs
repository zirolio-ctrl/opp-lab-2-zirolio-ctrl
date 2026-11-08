using System.Xml.Linq;
using opp_lab_2_zirolio_ctrl.Models;
using opp_lab_2_zirolio_ctrl.Services.Export;
using opp_lab_2_zirolio_ctrl.Services.SearchStrategies;

namespace opp_lab_2_zirolio_ctrl.Tests;

public class XmlExportFormatTests
{
    private static StudentRecord[] Sample() => new[]
    {
        new StudentRecord { FullName = "Іваненко І.І.", Faculty = "КН", Department = "ПО", Course = "2",
                            RoomNumber = "101", Block = "А", SettlementDate = "2026-01-01" },
        new StudentRecord { FullName = "Без блоку", Faculty = "ІТ", Course = "1", RoomNumber = "202" },
    };

    [Fact]
    public void Metadata_IsXml()
    {
        var f = new XmlExportFormat();
        Assert.Equal("xml", f.Extension);
        Assert.Equal("application/xml", f.MimeType);
    }

    [Fact]
    public void Build_StartsWithDeclaration_AndHasDormitoryRoot()
    {
        var xml = new XmlExportFormat().Build(Sample());

        Assert.StartsWith("<?xml", xml);
        Assert.Equal("Dormitory", XDocument.Parse(xml).Root!.Name.LocalName);
    }

    [Fact]
    public void Build_EmptyList_ProducesEmptyRoot()
    {
        var root = XDocument.Parse(new XmlExportFormat().Build(Array.Empty<StudentRecord>())).Root!;
        Assert.Empty(root.Elements("Student"));
    }

    [Fact]
    public void Build_OmitsEmptyRoomAndBlockAttributes()
    {
        var student = new StudentRecord { FullName = "X" };
        var el = XDocument.Parse(new XmlExportFormat().Build(new[] { student })).Root!.Element("Student")!;

        Assert.Null(el.Attribute("room"));
        Assert.Null(el.Attribute("block"));
    }

    [Fact]
    public void Build_EscapesSpecialCharacters()
    {
        var student = new StudentRecord { FullName = "A & B <\"x\">" };
        var xml = new XmlExportFormat().Build(new[] { student });

        Assert.Equal("A & B <\"x\">", XDocument.Parse(xml).Root!.Element("Student")!.Element("FullName")!.Value);
    }

    [Fact]
    public void Build_RoundTrip_ThroughSearchStrategy()
    {
        var items = Sample();
        using var f = new TempFile(new XmlExportFormat().Build(items));

        var back = new LinqSearchStrategy().Search(f.FilePath, "", "", "");

        Assert.Equal(items.Select(TestData.Describe), back.Select(TestData.Describe));
    }
}

public class HtmlExportFormatTests
{
    [Fact]
    public void Build_UsesXslt_AndContainsStudentNames()
    {
        using var xsl = new TempFile(TestData.SimpleXsl, ".xsl");
        var format = new HtmlExportFormat(new XsltTransformerService(), xsl.FilePath);

        var html = format.Build(new[] { new StudentRecord { FullName = "Тест Тестович" } });

        Assert.Equal("html", format.Extension);
        Assert.Equal("text/html", format.MimeType);
        Assert.Contains("<li>Тест Тестович</li>", html);
    }

    [Fact]
    public void Build_MissingXsl_ThrowsFileNotFound()
    {
        var format = new HtmlExportFormat(new XsltTransformerService(), "no_such_file.xsl");
        Assert.Throws<FileNotFoundException>(() => format.Build(Array.Empty<StudentRecord>()));
    }
}

public class ExportFormatFactoryTests
{
    [Theory]
    [InlineData("xml")]
    [InlineData("XML")]
    [InlineData("Xml")]
    public void Create_Xml_ReturnsXmlFormat(string name) =>
        Assert.IsType<XmlExportFormat>(ExportFormatFactory.Create(name));

    [Theory]
    [InlineData("html")]
    [InlineData("HTML")]
    public void Create_Html_ReturnsHtmlFormat(string name) =>
        Assert.IsType<HtmlExportFormat>(ExportFormatFactory.Create(name));

    [Theory]
    [InlineData("pdf")]
    [InlineData("")]
    [InlineData("csv")]
    public void Create_Unknown_ThrowsArgumentException(string name)
    {
        var ex = Assert.Throws<ArgumentException>(() => ExportFormatFactory.Create(name));
        Assert.Equal("format", ex.ParamName);
    }
}
