using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using opp_lab_2_zirolio_ctrl.Models;

namespace opp_lab_2_zirolio_ctrl.Services.Export;

public interface IExportFormat
{
    string Extension { get; }
    string MimeType { get; }
    string Build(IEnumerable<StudentRecord> items);
}

public sealed class XmlExportFormat : IExportFormat
{
    public string Extension => "xml";
    public string MimeType => "application/xml";

    public string Build(IEnumerable<StudentRecord> items)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Dormitory",
                items.Select(s => new XElement("Student",
                    Attr("room", s.RoomNumber),
                    Attr("block", s.Block),
                    new XElement("FullName", s.FullName),
                    new XElement("Faculty", s.Faculty),
                    new XElement("Department", s.Department),
                    new XElement("Course", s.Course),
                    new XElement("SettlementDate", s.SettlementDate)))));

        return $"{doc.Declaration}{Environment.NewLine}{doc}";
    }

    private static XAttribute? Attr(string name, string value) =>
        string.IsNullOrEmpty(value) ? null : new XAttribute(name, value);
}

public sealed class HtmlExportFormat : IExportFormat
{
    private readonly XsltTransformerService _transformer;
    private readonly string _xslPath;

    public HtmlExportFormat(XsltTransformerService transformer, string xslPath)
    {
        _transformer = transformer;
        _xslPath = xslPath;
    }

    public string Extension => "html";
    public string MimeType => "text/html";

    public string Build(IEnumerable<StudentRecord> items)
    {
        string xml = new XmlExportFormat().Build(items);
        return _transformer.TransformToString(xml, _xslPath);
    }
}

public static class ExportFormatFactory
{
    private static readonly string DefaultXslPath = Path.Combine(AppContext.BaseDirectory, "dormitory.xsl");

    public static IExportFormat Create(string format) => format.ToUpperInvariant() switch
    {
        "XML" => new XmlExportFormat(),
        "HTML" => new HtmlExportFormat(new XsltTransformerService(), DefaultXslPath),
        _ => throw new ArgumentException($"Невідомий формат експорту: {format}", nameof(format))
    };
}
