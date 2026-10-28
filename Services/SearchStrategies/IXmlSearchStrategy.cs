
using System.Collections.Generic;
using opp_lab_2_zirolio_ctrl.Models;

namespace opp_lab_2_zirolio_ctrl.Services;

public interface IXmlSearchStrategy
{
    List<StudentRecord> Search(string filePath, string faculty, string course, string room);
}