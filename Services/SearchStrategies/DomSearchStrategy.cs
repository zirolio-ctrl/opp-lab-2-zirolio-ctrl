
using System;
using System.Collections.Generic;
using System.Xml;
using opp_lab_2_zirolio_ctrl.Models;

namespace opp_lab_2_zirolio_ctrl.Services;

public class DomSearchStrategy : IXmlSearchStrategy
{
    public List<StudentRecord> Search(string filePath, string faculty, string course, string room)
    {
        var results = new List<StudentRecord>();
        var doc = new XmlDocument();
        doc.Load(filePath);

        XmlNodeList? studentNodes = doc.SelectNodes("//Student");
        if (studentNodes == null) return results;

        foreach (XmlNode node in studentNodes)
        {
            var student = new StudentRecord
            {
                RoomNumber = node.Attributes?["room"]?.Value ?? string.Empty,
                Block = node.Attributes?["block"]?.Value ?? string.Empty,
                FullName = node["FullName"]?.InnerText ?? string.Empty,
                Faculty = node["Faculty"]?.InnerText ?? string.Empty,
                Department = node["Department"]?.InnerText ?? string.Empty,
                Course = node["Course"]?.InnerText ?? string.Empty,
                SettlementDate = node["SettlementDate"]?.InnerText ?? string.Empty
            };

            bool matchFaculty = string.IsNullOrEmpty(faculty) || student.Faculty.Equals(faculty, StringComparison.OrdinalIgnoreCase);
            bool matchCourse = string.IsNullOrEmpty(course) || student.Course == course;
            bool matchRoom = string.IsNullOrEmpty(room) || student.RoomNumber == room;

            if (matchFaculty && matchCourse && matchRoom)
            {
                results.Add(student);
            }
        }

        return results;
    }
}