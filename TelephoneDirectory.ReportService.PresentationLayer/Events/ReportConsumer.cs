using ClosedXML.Excel;
using MassTransit;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TelephoneDirectory.ReportBusConfigurator;
using TelephoneDirectory.ReportBusConfigurator.Models;
using TelephoneDirectory.ReportService.BusinessLayer.DTOs;
using TelephoneDirectory.ReportService.DataAccessLayer;

namespace TelephoneDirectory.ReportService.PresentationLayer.Events
{
    public class ReportConsumer : IConsumer<IReportConsumer>
    {
        public ReportConsumer()
        {
            var bus = BusConfigurator.ConfigureBus(factory =>
            {
                factory.ReceiveEndpoint(RabbitMqConstants.ConsumerQueue, endpoint =>
                {
                    endpoint.Consumer<ReportConsumer>();
                });
            });

            bus.StartAsync();
        }
        public async Task Consume(ConsumeContext<IReportConsumer> context)
        {
            using (var reportContext = new ReportServiceContext())
            {
                var report = reportContext.Reports.FirstOrDefault(x => x.ID == context.Message.ID);

                if (report != null)
                {
                    var client = new RestClient("https://localhost:44311/api/userservice/userservice/createReport");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", "application/json");
                    var data = new AddressDto();
                    data.Address = report.Location;
                    request.AddParameter("application/json", JsonConvert.SerializeObject(data), ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                    var responsedata = JsonConvert.DeserializeObject<ReportInfoFromUserDto>(response.Content);
                    report.PhoneCount = responsedata.PhoneCount;
                    report.PersonCount = responsedata.UserCount;
                    report.ReportFilePath = CreateReportDirectory();
                    report.Status = DataAccessLayer.Entitites.ReportState.Completed;
                    ExportToExcel(responsedata, report.ReportFilePath);
                    reportContext.Reports.Update(report);
                    await reportContext.SaveChangesAsync();
                }
            }
            Console.WriteLine($"The report has been saved in the database.");
        }

        public string CreateReportDirectory()
        {
            string reportPath = Path.Combine(Environment.CurrentDirectory, @"Reports");
            if (!Directory.Exists(reportPath))
            {
                Directory.CreateDirectory(reportPath);
            }
            return Path.Combine(reportPath, "ContactServiceData-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
        }

        public void ExportToExcel(ReportInfoFromUserDto responsedata, string reportPath)
        {
            XLWorkbook workbook = new XLWorkbook();
            DataTable dt = new DataTable() { TableName = "ContactData" };
            DataSet ds = new DataSet();
            //input data
            var columns = new[] { "Location", "Phone Count", "Person Count" };
            var rows = new object[][] { new object[] { responsedata.Location, responsedata.PhoneCount, responsedata.UserCount } };

            //Add columns
            dt.Columns.AddRange(columns.Select(c => new DataColumn(c)).ToArray());

            //Add rows
            foreach (var row in rows)
            {
                dt.Rows.Add(row);
            }

            //Convert datatable to dataset and add it to the workbook as worksheet
            ds.Tables.Add(dt);
            workbook.Worksheets.Add(ds);

            //save data to excel
            workbook.SaveAs(reportPath, false);
        }
    }
}
