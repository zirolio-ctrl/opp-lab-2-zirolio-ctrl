
using System;
using System.Collections.Generic;
using System.Xml;
using opp_lab_2_zirolio_ctrl.Models;

namespace opp_lab_2_zirolio_ctrl.Services.SearchStrategies;

public class SaxSearchStrategy : IXmlSearchStrategy
{
    public List<StudentRecord> Search(string filePath, string faculty, string course, string room)
    {
        var results = new List<StudentRecord>();
        using var reader = XmlReader.Create(filePath);

        StudentRecord? currentStudent = null;
        string currentElement = string.Empty;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentElement = reader.Name;
                    if (currentElement == "Student")
                    {
                        currentStudent = new StudentRecord();
                        if (reader.HasAttributes)
                        {
                            currentStudent.RoomNumber = reader.GetAttribute("room") ?? string.Empty;
                            currentStudent.Block = reader.GetAttribute("block") ?? string.Empty;
                        }
                    }
                    break;

                case XmlNodeType.Text:
                    if (currentStudent != null)
                    {
                        string value = reader.Value.Trim();
                        switch (currentElement)
                        {
                            case "FullName": currentStudent.FullName = value; break;
                            case "Faculty": currentStudent.Faculty = value; break;
                            case "Department": currentStudent.Department = value; break;
                            case "Course": currentStudent.Course = value; break;
                            case "SettlementDate": currentStudent.SettlementDate = value; break;
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (reader.Name == "Student" && currentStudent != null)
                    {
                        bool matchFaculty = string.IsNullOrEmpty(faculty) || currentStudent.Faculty.Equals(faculty, StringComparison.OrdinalIgnoreCase);
                        bool matchCourse = string.IsNullOrEmpty(course) || currentStudent.Course == course;
                        bool matchRoom = string.IsNullOrEmpty(room) || currentStudent.RoomNumber == room;

                        if (matchFaculty && matchCourse && matchRoom)
                        {
                            results.Add(currentStudent);
                        }
                        currentStudent = null;
                    }
                    break;
            }
        }

        return results;
    }
}