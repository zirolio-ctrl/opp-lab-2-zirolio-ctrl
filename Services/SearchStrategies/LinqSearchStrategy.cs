
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using opp_lab_2_zirolio_ctrl.Models;

namespace opp_lab_2_zirolio_ctrl.Services.SearchStrategies;

public class LinqSearchStrategy : IXmlSearchStrategy
{
    public List<StudentRecord> Search(string filePath, string faculty, string course, string room)
    {
        var doc = XDocument.Load(filePath);

        var query = from student in doc.Descendants("Student")
                    let f = student.Element("Faculty")?.Value ?? string.Empty
                    let c = student.Element("Course")?.Value ?? string.Empty
                    let r = (string?)student.Attribute("room") ?? string.Empty
                    let b = (string?)student.Attribute("block") ?? string.Empty
                    where   (string.IsNullOrEmpty(faculty) || f.Equals(faculty, StringComparison.OrdinalIgnoreCase)) &&
                            (string.IsNullOrEmpty(course) || c == course) &&
                            (string.IsNullOrEmpty(room) || r == room)
                    select new StudentRecord
                    {
                        RoomNumber = r,
                        Block = b,
                        FullName = student.Element("FullName")?.Value ?? string.Empty,
                        Faculty = f,
                        Department = student.Element("Department")?.Value ?? string.Empty,
                        Course = c,
                        SettlementDate = student.Element("SettlementDate")?.Value ?? string.Empty
                    };

        return query.ToList();
    }
}