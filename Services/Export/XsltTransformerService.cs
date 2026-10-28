using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace opp_lab_2_zirolio_ctrl.Services.Export;

public class XsltTransformerService
{
    public void TransformToHtml(string xmlPath, string xslPath, string htmlOutputPath)
    {
        var xslt = new XslCompiledTransform();
        xslt.Load(xslPath);
        xslt.Transform(xmlPath, htmlOutputPath);
    }

    public string TransformToString(string xmlContent, string xslPath)
    {
        if (!File.Exists(xslPath))
            throw new FileNotFoundException($"Файл шаблону не знайдено: {xslPath}", xslPath);

        var xslt = new XslCompiledTransform();
        xslt.Load(xslPath);

        var settings = xslt.OutputSettings?.Clone() ?? new XmlWriterSettings();
        settings.Encoding = new UTF8Encoding(false);

        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, settings))
        using (var reader = XmlReader.Create(new StringReader(xmlContent)))
        {
            xslt.Transform(reader, writer);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}