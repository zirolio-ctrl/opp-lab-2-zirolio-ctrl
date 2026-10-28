<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes"/>

  <xsl:template match="/">
    <html lang="uk">
      <head>
        <meta charset="utf-8"/>
        <meta name="viewport" content="width=device-width, initial-scale=1"/>
        <title>Звіт: Гуртожиток</title>
        <style>
          :root {
            --accent: #0078d4;
            --accent-dark: #005a9e;
            --bg: #f3f5f8;
            --card: #ffffff;
            --text: #1f2933;
            --muted: #6b7785;
            --line: #e4e8ee;
          }
          * { box-sizing: border-box; }
          body {
            margin: 0;
            padding: 32px 20px;
            background: var(--bg);
            color: var(--text);
            font-family: "Segoe UI", Roboto, Arial, sans-serif;
          }
          .card {
            max-width: 1100px;
            margin: 0 auto;
            background: var(--card);
            border-radius: 12px;
            box-shadow: 0 4px 18px rgba(15, 30, 60, 0.10);
            overflow: hidden;
          }
          header {
            padding: 24px 28px;
            color: #fff;
            background: linear-gradient(135deg, var(--accent), var(--accent-dark));
          }
          header h1 { margin: 0 0 6px; font-size: 24px; font-weight: 600; }
          header p  { margin: 0; opacity: 0.9; font-size: 14px; }
          .table-wrap { overflow-x: auto; padding: 8px 12px 20px; }
          table { width: 100%; border-collapse: collapse; font-size: 14px; }
          th {
            padding: 12px 14px;
            text-align: left;
            font-size: 12px;
            letter-spacing: 0.04em;
            text-transform: uppercase;
            color: var(--muted);
            border-bottom: 2px solid var(--line);
            white-space: nowrap;
          }
          td { padding: 12px 14px; border-bottom: 1px solid var(--line); }
          tbody tr:nth-child(even) { background: #fafbfc; }
          tbody tr:hover { background: #eaf3fc; }
          .num  { width: 44px; color: var(--muted); }
          .name { font-weight: 600; }
          .badge {
            display: inline-block;
            min-width: 28px;
            padding: 2px 10px;
            border-radius: 999px;
            background: #e6f1fb;
            color: var(--accent-dark);
            font-weight: 600;
            text-align: center;
          }
          .badge.empty { background: transparent; color: var(--muted); font-weight: 400; }
          .empty-row td { padding: 32px; text-align: center; color: var(--muted); }
          footer { padding: 14px 28px; border-top: 1px solid var(--line); color: var(--muted); font-size: 12px; }
          @media print {
            body { background: #fff; padding: 0; }
            .card { box-shadow: none; border-radius: 0; }
            header { background: var(--accent) !important; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
            tbody tr:hover { background: none; }
          }
        </style>
      </head>

      <body>
        <div class="card">
          <header>
            <h1>Список мешканців гуртожитку</h1>
            <p>Усього записів: <strong><xsl:value-of select="count(/*/Student)"/></strong></p>
          </header>

          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>№</th>
                  <th>П.І.Б.</th>
                  <th>Факультет</th>
                  <th>Кафедра</th>
                  <th>Курс</th>
                  <th>Кімната</th>
                  <th>Блок</th>
                  <th>Дата поселення</th>
                </tr>
              </thead>
              <tbody>
                <xsl:for-each select="/*/Student">
                  <tr>
                    <td class="num"><xsl:value-of select="position()"/></td>
                    <td class="name"><xsl:value-of select="FullName"/></td>
                    <td><xsl:value-of select="Faculty"/></td>
                    <td><xsl:value-of select="Department"/></td>
                    <td><span class="badge"><xsl:value-of select="Course"/></span></td>
                    <td><xsl:value-of select="@room"/></td>
                    <td>
                      <xsl:choose>
                        <xsl:when test="@block">
                          <span class="badge"><xsl:value-of select="@block"/></span>
                        </xsl:when>
                        <xsl:otherwise>
                          <span class="badge empty">—</span>
                        </xsl:otherwise>
                      </xsl:choose>
                    </td>
                    <td><xsl:value-of select="SettlementDate"/></td>
                  </tr>
                </xsl:for-each>

                <xsl:if test="count(/*/Student) = 0">
                  <tr class="empty-row">
                    <td colspan="8">Записів не знайдено</td>
                  </tr>
                </xsl:if>
              </tbody>
            </table>
          </div>

          <footer>Інформаційна система «Гуртожиток»</footer>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>