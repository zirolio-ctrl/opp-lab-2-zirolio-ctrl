using System.Text;
using opp_lab_2_zirolio_ctrl.Models;
using opp_lab_2_zirolio_ctrl.Services.SearchStrategies;

namespace opp_lab_2_zirolio_ctrl.Tests;

public static class Strategies
{
    public static readonly string[] Names = { "dom", "linq", "sax" };

    public static TheoryData<string> All
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var n in Names) data.Add(n);
            return data;
        }
    }

    public static IXmlSearchStrategy Create(string name) => name switch
    {
        "dom" => new DomSearchStrategy(),
        "linq" => new LinqSearchStrategy(),
        "sax" => new SaxSearchStrategy(),
        _ => throw new ArgumentException(name)
    };
}

public sealed class TempFile : IDisposable
{
    public string FilePath { get; }

    public TempFile(string content, string extension = ".xml")
    {
        FilePath = Path.Combine(Path.GetTempPath(), $"lab2_{Guid.NewGuid():N}{extension}");
        File.WriteAllText(FilePath, content, new UTF8Encoding(false));
    }

    public void Dispose()
    {
        try { File.Delete(FilePath); } catch { /* ignore */ }
    }
}

public static class TestData
{
    public const string SampleXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Dormitory>
          <Student room="301a" block="А">
            <FullName>Чах Максим Олександрович</FullName>
            <Faculty>КН</Faculty>
            <Department>ПО</Department>
            <Course>1</Course>
            <SettlementDate>2026-03-01</SettlementDate>
          </Student>
          <Student room="302">
            <FullName>Шевченко Тарас Григорович</FullName>
            <Faculty>ФЕМ</Faculty>
            <Department>Менеджмент</Department>
            <Course>2</Course>
            <SettlementDate>2025-09-01</SettlementDate>
          </Student>
          <Student room="405b" block="Б" priority="high">
            <FullName>Коваленко Андрій Петрович</FullName>
            <Faculty>КН</Faculty>
            <Department>Системний аналіз</Department>
            <Course>1</Course>
            <SettlementDate>2026-02-15</SettlementDate>
          </Student>
          <Student room="210" priority="medium" status="contract">
            <FullName>Мельник Олена Володимирівна</FullName>
            <Faculty>ІТ</Faculty>
            <Department>Кібербезпека</Department>
            <Course>3</Course>
            <SettlementDate>2024-09-01</SettlementDate>
          </Student>
          <Student room="512" block="В">
            <FullName>Бойко Дмитро Сергійович</FullName>
            <Faculty>ПІ</Faculty>
            <Department>Інженерія програмного забезпечення</Department>
            <Course>4</Course>
            <SettlementDate>2023-09-01</SettlementDate>
          </Student>
          <Student room="301a" block="А">
            <FullName>Лисенко Ірина Андріївна</FullName>
            <Faculty>КН</Faculty>
            <Department>ПО</Department>
            <Course>2</Course>
            <SettlementDate>2025-09-01</SettlementDate>
          </Student>
        </Dormitory>
        """;

    public const string SimpleXsl = """
        <?xml version="1.0" encoding="utf-8"?>
        <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:output method="html" encoding="utf-8" indent="yes"/>
          <xsl:template match="/Dormitory">
            <html><body><ul>
              <xsl:for-each select="Student">
                <li><xsl:value-of select="FullName"/></li>
              </xsl:for-each>
            </ul></body></html>
          </xsl:template>
        </xsl:stylesheet>
        """;

    public static string Describe(StudentRecord s) =>
        string.Join("|", s.RoomNumber, s.Block, s.FullName, s.Faculty, s.Department, s.Course, s.SettlementDate);

    public static string RealFile(string name) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", name);
}
