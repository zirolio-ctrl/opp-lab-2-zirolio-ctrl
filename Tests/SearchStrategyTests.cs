using System.Xml;
using opp_lab_2_zirolio_ctrl.Models;
using opp_lab_2_zirolio_ctrl.Services.SearchStrategies;

namespace opp_lab_2_zirolio_ctrl.Tests;

public class SearchStrategyTests : IDisposable
{
    private readonly TempFile _xml = new(TestData.SampleXml);
    public void Dispose() => _xml.Dispose();

    private static readonly (string faculty, string course, string room, int expected)[] Cases =
    {
        ("", "", "", 6),            // без фильтров
        ("КН", "", "", 3),
        ("кн", "", "", 3),          // факультет без учёта регистра
        ("", "1", "", 2),
        ("", "", "301a", 2),
        ("КН", "1", "", 2),
        ("КН", "1", "301a", 1),     // комбинация всех трёх
        ("КН", "4", "", 0),
        ("Немає", "", "", 0),
        ("", "", "999", 0),
        ("", "", "301A", 0),        // комната: регистр важен
        ("", "01", "", 0),          // курс: точное сравнение
    };

    public static TheoryData<string, string, string, string, int> FilterCases()
    {
        var data = new TheoryData<string, string, string, string, int>();
        foreach (var name in Strategies.Names)
            foreach (var c in Cases)
                data.Add(name, c.faculty, c.course, c.room, c.expected);
        return data;
    }

    public static TheoryData<string, string, string, int> CrossCases()
    {
        var data = new TheoryData<string, string, string, int>();
        foreach (var c in Cases) data.Add(c.faculty, c.course, c.room, c.expected);
        return data;
    }

    [Theory]
    [MemberData(nameof(FilterCases))]
    public void Search_AppliesFilters(string strategy, string faculty, string course, string room, int expected)
    {
        var result = Strategies.Create(strategy).Search(_xml.FilePath, faculty, course, room);
        Assert.Equal(expected, result.Count);
    }

    [Theory]
    [MemberData(nameof(CrossCases))]
    public void AllStrategies_ReturnIdenticalResults(string faculty, string course, string room, int expected)
    {
        var dom = new DomSearchStrategy().Search(_xml.FilePath, faculty, course, room).Select(TestData.Describe).ToList();
        var linq = new LinqSearchStrategy().Search(_xml.FilePath, faculty, course, room).Select(TestData.Describe).ToList();
        var sax = new SaxSearchStrategy().Search(_xml.FilePath, faculty, course, room).Select(TestData.Describe).ToList();

        Assert.Equal(expected, dom.Count);
        Assert.Equal(dom, linq);
        Assert.Equal(dom, sax);
    }

    [Theory]
    [MemberData(nameof(Strategies.All), MemberType = typeof(Strategies))]
    public void Search_MapsAllFields(string strategy)
    {
        var s = Assert.Single(Strategies.Create(strategy).Search(_xml.FilePath, "ІТ", "", ""));

        Assert.Equal("Мельник Олена Володимирівна", s.FullName);
        Assert.Equal("ІТ", s.Faculty);
        Assert.Equal("Кібербезпека", s.Department);
        Assert.Equal("3", s.Course);
        Assert.Equal("210", s.RoomNumber);
        Assert.Equal(string.Empty, s.Block);   // атрибута block нет
        Assert.Equal("2024-09-01", s.SettlementDate);
    }

    [Theory]
    [MemberData(nameof(Strategies.All), MemberType = typeof(Strategies))]
    public void Search_ReadsBlockAttribute(string strategy)
    {
        var s = Assert.Single(Strategies.Create(strategy).Search(_xml.FilePath, "", "", "405b"));
        Assert.Equal("Б", s.Block);
    }

    [Theory]
    [MemberData(nameof(Strategies.All), MemberType = typeof(Strategies))]
    public void Search_MissingChildElements_BecomeEmptyStrings(string strategy)
    {
        using var f = new TempFile("<Dormitory><Student room=\"1\"><FullName>Тест</FullName></Student></Dormitory>");

        var s = Assert.Single(Strategies.Create(strategy).Search(f.FilePath, "", "", ""));

        Assert.Equal("Тест", s.FullName);
        Assert.Equal("1", s.RoomNumber);
        Assert.Equal(string.Empty, s.Faculty);
        Assert.Equal(string.Empty, s.Department);
        Assert.Equal(string.Empty, s.Course);
        Assert.Equal(string.Empty, s.SettlementDate);
    }

    [Theory]
    [MemberData(nameof(Strategies.All), MemberType = typeof(Strategies))]
    public void Search_EmptyDormitory_ReturnsEmptyList(string strategy)
    {
        using var f = new TempFile("<Dormitory/>");
        Assert.Empty(Strategies.Create(strategy).Search(f.FilePath, "", "", ""));
    }

    [Theory]
    [MemberData(nameof(Strategies.All), MemberType = typeof(Strategies))]
    public void Search_MissingFile_ThrowsIOException(string strategy)
    {
        var path = Path.Combine(Path.GetTempPath(), $"nope_{Guid.NewGuid():N}.xml");
        Assert.ThrowsAny<IOException>(() => Strategies.Create(strategy).Search(path, "", "", ""));
    }

    [Theory]
    [MemberData(nameof(Strategies.All), MemberType = typeof(Strategies))]
    public void Search_MalformedXml_ThrowsXmlException(string strategy)
    {
        using var f = new TempFile("<Dormitory><Student>");
        Assert.ThrowsAny<XmlException>(() => Strategies.Create(strategy).Search(f.FilePath, "", "", ""));
    }
}
