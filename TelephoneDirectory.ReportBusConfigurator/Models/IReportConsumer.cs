using System;
using System.Collections.Generic;
using System.Text;
using TelephoneDirectory.ReportService.DataAccessLayer.Entitites;

namespace TelephoneDirectory.ReportBusConfigurator.Models
{
   public interface IReportConsumer
    {
        Guid ID { get; set; }
        string ReportName { get; set; }
        string Location { get; set; }
        int PersonCount { get; set; }

        int PhoneCount { get; set; }
        DateTime RequestedDate { get; set; }
        string CreatedDate { get; set; }

        ReportState Status { get; set; }
    }
}
