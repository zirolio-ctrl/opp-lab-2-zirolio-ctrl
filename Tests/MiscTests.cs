using System.Text.RegularExpressions;
using opp_lab_2_zirolio_ctrl.Models;
using opp_lab_2_zirolio_ctrl.Services;
using opp_lab_2_zirolio_ctrl.Services.Export;
using opp_lab_2_zirolio_ctrl.Services.SearchStrategies;

namespace opp_lab_2_zirolio_ctrl.Tests;

public class StudentRecordTests
{
    [Fact]
    public void NewRecord_HasEmptyStringsNotNulls()
    {
        var s = new StudentRecord();

        Assert.Equal(string.Empty, s.FullName);
        Assert.Equal(string.Empty, s.Faculty);
        Assert.Equal(string.Empty, s.Department);
        Assert.Equal(string.Empty, s.Course);
        Assert.Equal(string.Empty, s.RoomNumber);
        Assert.Equal(string.Empty, s.Block);
        Assert.Equal(string.Empty, s.SettlementDate);
    }

    [Fact]
    public void Properties_StoreValues()
    {
        var s = new StudentRecord { FullName = "A", Faculty = "B", Department = "C", Course = "1",
                                    RoomNumber = "2", Block = "Д", SettlementDate = "2026-01-01" };

        Assert.Equal("A|B|C|1|2|Д|2026-01-01",
            string.Join("|", s.FullName, s.Faculty, s.Department, s.Course, s.RoomNumber, s.Block, s.SettlementDate));
    }
}

public class EventLoggerTests
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "app_events.log");

    [Fact]
    public void Instance_IsSingleton() =>
        Assert.Same(EventLogger.Instance, EventLogger.Instance);

    [Fact]
    public void Log_AppendsLineInExpectedFormat()
    {
        var marker = Guid.NewGuid().ToString("N");

        EventLogger.Instance.Log("INFO", marker);

        var line = Assert.Single(File.ReadAllLines(LogPath), l => l.Contains(marker));
        Assert.Matches(new Regex(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2} INFO\. " + marker + "$"), line);
    }

    [Fact]
    public void Log_DoesNotOverwritePreviousEntries()
    {
        var first = Guid.NewGuid().ToString("N");
        var second = Guid.NewGuid().ToString("N");

        EventLogger.Instance.Log("WARN", first);
        EventLogger.Instance.Log("ERROR", second);

        var text = File.ReadAllText(LogPath);
        Assert.Contains(first, text);
        Assert.Contains(second, text);
        Assert.True(text.IndexOf(first, StringComparison.Ordinal) < text.IndexOf(second, StringComparison.Ordinal));
    }
}

// Интеграционные тесты на реальных файлах из Assets (подключены через Link в .csproj)
public class RealAssetsTests
{
    [Fact]
    public void RealXml_AllStrategiesAgree_AndAreNotEmpty()
    {
        var path = TestData.RealFile("dormitory.xml");

        var dom = new DomSearchStrategy().Search(path, "", "", "").Select(TestData.Describe).ToList();
        var linq = new LinqSearchStrategy().Search(path, "", "", "").Select(TestData.Describe).ToList();
        var sax = new SaxSearchStrategy().Search(path, "", "", "").Select(TestData.Describe).ToList();

        Assert.NotEmpty(dom);
        Assert.Equal(dom, linq);
        Assert.Equal(dom, sax);
    }

    [Fact]
    public void RealXsl_TransformsRealXml_ToNonEmptyHtml()
    {
        var xml = File.ReadAllText(TestData.RealFile("dormitory.xml"));

        var html = new XsltTransformerService().TransformToString(xml, TestData.RealFile("dormitory.xsl"));

        Assert.Contains("<html", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Export_RealData_ToHtml_ViaFactoryPipeline()
    {
        var students = new LinqSearchStrategy().Search(TestData.RealFile("dormitory.xml"), "", "", "");
        var format = new HtmlExportFormat(new XsltTransformerService(), TestData.RealFile("dormitory.xsl"));

        var html = format.Build(students);

        Assert.Contains(students[0].FullName, html);
    }
}
