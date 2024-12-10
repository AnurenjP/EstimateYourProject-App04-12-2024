using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ProjectEstimationApp.Models;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading;
//using Project_Est_App.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.Globalization;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace ProjectEstimationApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitForm(ProjectEstimation model)
        {
            // Store the form data in TempData to pass it to the next page
            TempData["ProjectEstimation"] = model;

            // Redirect to the next page
            return RedirectToAction("Estpage");
        }

        public IActionResult Estpage()
        {
            var model = TempData["ProjectEstimation"] as ProjectEstimation;
            return View(model);
        }

        public IActionResult Resource()
        {
            return View();
        }

        public IActionResult Sampletimeline()
        {
            return View();
        }

        public IActionResult Smbud()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Homepage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerateFiles([FromBody] ProjectData projectData)
        {
            var projectName = projectData.ProjectName ?? "ProjectEstimation"; // Use project name or default

            var excelFilePath = GenerateExcel(projectData);
            var pptFilePath = GeneratePowerPoint(projectData);
            var zipFilePath = CreateZipFile(excelFilePath, pptFilePath);

            byte[] fileBytes = System.IO.File.ReadAllBytes(zipFilePath);
            System.IO.File.Delete(excelFilePath);
            System.IO.File.Delete(pptFilePath);
            System.IO.File.Delete(zipFilePath);

            return File(fileBytes, "application/zip", "ProjectEstimation.zip");
        }

        private string GenerateExcel(ProjectData projectData)
        {
            // Set the license context for EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var tempPath = Path.GetTempPath();
            var filePath = Path.Combine(tempPath, "ProjectEstimation.xlsx");

            //edit
            DateTime projectStartDate = DateTime.ParseExact(projectData.AnalysisStartDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);


            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Budget Details");
                    worksheet.Cells[1, 1].Value = "Resource Type";
                    worksheet.Cells[1, 2].Value = "Cost";
                    worksheet.Cells[1, 3].Value = "Number of Resources";
                    worksheet.Cells[1, 4].Value = "Total";

                    int row = 2;
                    foreach (var resource in projectData.Resources.Take(11))
                    {
                        worksheet.Cells[row, 1].Value = resource.Name;
                        worksheet.Cells[row, 2].Value = resource.Cost;
                        worksheet.Cells[row, 3].Value = resource.NumberOfResources;
                        worksheet.Cells[row, 4].Value = resource.Total;
                        row++;
                    }
                    int totalRow = row;

                    worksheet.Cells[totalRow, 3].Value = "Total Cost";

                    worksheet.Cells[totalRow, 4].Formula = $"SUM(D2:D{row - 1})";

                    worksheet.Cells[totalRow, 3, totalRow, 4].Style.Font.Bold = true;

                    row++;


                    //worksheet.Cells[row, 1].Value = "Project Start Date";
                    //worksheet.Cells[row, 2].Value = FormatDate(projectData.ProjectStartDate);

                    //worksheet.Cells[row + 1, 1].Value = "Project End Date";
                    //worksheet.Cells[row + 1, 2].Value = FormatDate(projectData.ProjectEndDate);


                    worksheet.Column(1).AutoFit(); // 
                    worksheet.Column(2).AutoFit(); // 
                    worksheet.Column(3).AutoFit(); // 
                    worksheet.Column(4).AutoFit(); // 

                    var additionalCostsSheet = package.Workbook.Worksheets.Add("Additional Costs");
                    additionalCostsSheet.Cells[1, 1].Value = "Name";
                    additionalCostsSheet.Cells[1, 2].Value = "Cost";
                    additionalCostsSheet.Cells[1, 3].Value = "Number of Resources";
                    additionalCostsSheet.Cells[1, 4].Value = "Total";

                    int additionalRow = 2;
                    foreach (var cost in projectData.AdditionalCosts)
                    {
                        additionalCostsSheet.Cells[additionalRow, 1].Value = cost.Name;
                        additionalCostsSheet.Cells[additionalRow, 2].Value = cost.Cost;
                        additionalCostsSheet.Cells[additionalRow, 3].Value = cost.NumberOfResources;
                        additionalCostsSheet.Cells[additionalRow, 4].Value = cost.Total;
                        additionalRow++;
                    }
                    additionalCostsSheet.Cells[additionalRow, 3].Value = "Total";
                    additionalCostsSheet.Cells[additionalRow, 4].Formula = $"SUM(D2:D{additionalRow - 1})";
                    additionalCostsSheet.Cells[additionalRow, 3, additionalRow, 4].Style.Font.Bold = true;

                    additionalCostsSheet.Column(1).AutoFit(); // 
                    additionalCostsSheet.Column(2).AutoFit(); // 
                    additionalCostsSheet.Column(3).AutoFit(); // 
                    additionalCostsSheet.Column(4).AutoFit(); // 

                    var EstimationSheet = package.Workbook.Worksheets.Add("Estimation Details");
                    EstimationSheet.Cells[1, 1].Value = "Task";
                    EstimationSheet.Cells[1, 2].Value = "Planed Effort (Hours)";
                    //EstimationSheet.Cells[1, 3].Value = "Number of Resources";
                    //EstimationSheet.Cells[1, 4].Value = "Total";

                    int newRow = 2;
                  
                        EstimationSheet.Cells[newRow, 1].Value = "Analysis and requirement signoff";
                        EstimationSheet.Cells[newRow, 2].Value = projectData.Analysisandrequirementsignoff;

                        newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Functional Design";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.FunctionalDesign;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Technical Design";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.TechnicalDesign;

                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "Analysis and Design";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.AnalysisandDesign1;
                    EstimationSheet.Cells[newRow, 1, newRow, 2].Style.Font.Bold = true;
                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Frontend changes";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.Frontendchanges;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Integration Changes";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.IntegrationChanges;

                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "Backend Changes";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.BackendChanges;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Coding";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.Coding;
                    EstimationSheet.Cells[newRow, 1, newRow, 2].Style.Font.Bold = true;
                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "UnitTestCase Preparation";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.UnitTestCasePreparation;

                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "Unittestlogs and DefectFix";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.UnittestlogsandDefectFix;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Code Review";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.CodeReview;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Unit Test Case Review";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.UnitTestCaseReview;

                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "Unit test Result Review";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.UnittestResultReview;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Unit Testing";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.UnitTesting;
                    EstimationSheet.Cells[newRow, 1, newRow, 2].Style.Font.Bold = true;
                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "QA and Test Result Review";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.QAandTestResultReview;

                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "QA and UAT Support";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.QAandUATSupport;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "QA Test Case Preparation";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.QATestCasePreparation;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "QATesting and DefectFix";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.QATestingandDefectFix;

                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "Integration Testing";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.IntegrationTesting;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "UATTesting and DefectFix";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.UATTestingandDefectFix;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "QA and UAT Testing";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.QAandUATTesting;
                    EstimationSheet.Cells[newRow, 1, newRow, 2].Style.Font.Bold = true;
                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "Release Management";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.Releasemanagement;
                    EstimationSheet.Cells[newRow, 1, newRow, 2].Style.Font.Bold = true;
                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Deployment Support";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.DeploymentSupport;

                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Warranty Support";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.WarrantySupport;

                    newRow++;

                    EstimationSheet.Cells[newRow, 1].Value = "Support";
                    EstimationSheet.Cells[newRow, 2].Value = projectData.Support;
                    EstimationSheet.Cells[newRow, 1, newRow, 2].Style.Font.Bold = true;
                    newRow++;
                    EstimationSheet.Cells[newRow, 1].Value = "Total";
                    EstimationSheet.Cells[newRow, 2].Formula = $"SUMIF(A2:A{newRow - 1}, \"Analysis and Design\", B2:B{newRow - 1}) + " +
                                                                $"SUMIF(A2:A{newRow - 1}, \"Coding\", B2:B{newRow - 1}) + " +
                                                                $"SUMIF(A2:A{newRow - 1}, \"Unit Testing\", B2:B{newRow - 1}) + " +
                                                                $"SUMIF(A2:A{newRow - 1}, \"QA and UAT Testing\", B2:B{newRow - 1}) + " +
                                                                $"SUMIF(A2:A{newRow - 1}, \"Release Management\", B2:B{newRow - 1}) + " +
                                                                $"SUMIF(A2:A{newRow - 1}, \"Support\", B2:B{newRow - 1})";
                    EstimationSheet.Cells[newRow, 1, newRow, 2].Style.Font.Bold = true;



                    EstimationSheet.Column(1).AutoFit(); // 
                    EstimationSheet.Column(2).AutoFit(); // 
                    EstimationSheet.Column(3).AutoFit(); // 


                    // New sheet for project dates
                    var datesSheet = package.Workbook.Worksheets.Add("Project Dates");


                    // Add headers
                    datesSheet.Cells[1, 1].Value = "Task";
                    datesSheet.Cells[1, 2].Value = "Start Date";
                    datesSheet.Cells[1, 3].Value = "End Date";

                    // Assuming the task data is already populated in the worksheet
                    // Find the project start date (assuming it's the earliest start date)
                    //DateTime projectStartDate = DateTime.MaxValue;
                    int lastRow = datesSheet.Dimension.End.Row;

                    for (int roww = 2; row <= lastRow; roww++)
                    {
                        if (DateTime.TryParseExact(datesSheet.Cells[roww, 2].Text, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                        {
                            if (startDate < projectStartDate)
                            {
                                projectStartDate = startDate;
                            }
                        }
                    }


                    int currentMonth = projectStartDate.Month;
                    int mergeStartColumn = 4;
                    for (int i = 0; i < 120; i++) // 120 days for example
                    {
                        DateTime currentDate = projectStartDate.AddDays(i);
                        if (currentDate.Month != currentMonth || i == 119)
                        {
                            int mergeEndColumn = 4 + i - 1;
                            if (i == 119) mergeEndColumn = 4 + i; // Include the last day
                            datesSheet.Cells[1, mergeStartColumn, 1, mergeEndColumn].Merge = true;
                            datesSheet.Cells[1, mergeStartColumn, 1, mergeEndColumn].Value = currentDate.AddDays(-1).ToString("MMMM");
                            datesSheet.Cells[1, mergeStartColumn, 1, mergeEndColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            mergeStartColumn = 4 + i;
                            currentMonth = currentDate.Month;
                        }
                        datesSheet.Cells[4, 4 + i].Value = currentDate.DayOfWeek.ToString().Substring(0, 3); // Add day of the week
                        datesSheet.Cells[4, 4 + i].Style.Font.Size = 8; // Reduce font size for days
                        datesSheet.Cells[3, 4 + i].Value = currentDate.Day; // Add day of the month
                    }

                    //// Add calendar months to the top row and merge cells
                    //int currentMonth = projectStartDate.Month;
                    //int mergeStartColumn = 4;
                    //for (int i = 0; i < 120; i++) // 120 days for example
                    //{
                    //    DateTime currentDate = projectStartDate.AddDays(i);
                    //    if (currentDate.Month != currentMonth || i == 119)
                    //    {
                    //        int mergeEndColumn = 4 + i - 1;
                    //        if (i == 119) mergeEndColumn = 4 + i; // Include the last day
                    //        datesSheet.Cells[1, mergeStartColumn, 1, mergeEndColumn].Merge = true;
                    //        datesSheet.Cells[1, mergeStartColumn, 1, mergeEndColumn].Value = currentDate.AddDays(-1).ToString("MMMM");
                    //        datesSheet.Cells[1, mergeStartColumn, 1, mergeEndColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //        mergeStartColumn = 4 + i;
                    //        currentMonth = currentDate.Month;
                    //    }
                    //    datesSheet.Cells[4, 4 + i].Value = currentDate.DayOfWeek.ToString().Substring(0, 3); // Add day of the week
                    //    datesSheet.Cells[4, 4 + i].Style.Font.Size = 8; // Reduce font size for days
                    //    datesSheet.Cells[3, 4 + i].Value = currentDate.Day; // Add day of the month
                    //}


                    int dateRow = 5;

                    datesSheet.Cells[dateRow, 1].Value = "Analysis";
                    datesSheet.Cells[dateRow, 2].Value = FormatDate(projectData.AnalysisStartDate);
                    datesSheet.Cells[dateRow, 3].Value = FormatDate(projectData.AnalysisEndDate);

                    dateRow++;
                    datesSheet.Cells[dateRow, 1].Value = "Design";
                    datesSheet.Cells[dateRow, 2].Value = FormatDate(projectData.DesignStartDate);
                    datesSheet.Cells[dateRow, 3].Value = FormatDate(projectData.DesignEndDate);


                    dateRow++;
                    datesSheet.Cells[dateRow, 1].Value = "Development";
                    datesSheet.Cells[dateRow, 2].Value = FormatDate(projectData.DevStart);
                    datesSheet.Cells[dateRow, 3].Value = FormatDate(projectData.DevEnd);

                    dateRow++;
                    datesSheet.Cells[dateRow, 1].Value = "Testing";
                    datesSheet.Cells[dateRow, 2].Value = FormatDate(projectData.TestStart);
                    datesSheet.Cells[dateRow, 3].Value = FormatDate(projectData.TestEnd);

                    dateRow++;
                    datesSheet.Cells[dateRow, 1].Value = "UAT";
                    datesSheet.Cells[dateRow, 2].Value = FormatDate(projectData.UATStart);
                    datesSheet.Cells[dateRow, 3].Value = FormatDate(projectData.UATEnd);

                    dateRow++;
                    datesSheet.Cells[dateRow, 1].Value = "Production";
                    datesSheet.Cells[dateRow, 2].Value = FormatDate(projectData.PRODdates);
                    datesSheet.Cells[dateRow, 3].Value = FormatDate(projectData.PRODdates);
                    dateRow++;
                    datesSheet.Cells[dateRow, 1].Value = "BC Date";
                    datesSheet.Cells[dateRow, 2].Value = FormatDate(projectData.BCdates);
                    datesSheet.Cells[dateRow, 3].Value = FormatDate(projectData.BCdates);

                    dateRow++;
                    ApplyConditionalFormatting(datesSheet, dateRow, projectStartDate);

                    // Format the cell sizes
                    datesSheet.Column(1).AutoFit(); // Task column
                    datesSheet.Column(2).AutoFit(); // Start Date column
                    datesSheet.Column(3).AutoFit(); // End Date column

                    for (int i = 4; i < 124; i++) // Adjust the width of the date columns
                    {
                        datesSheet.Column(i).Width = 4; // Minimize the width of each cell
                    }



                    //resource info
                    var resourceSheet = package.Workbook.Worksheets.Add("Resource Details");
                    resourceSheet.Cells[1, 1].Value = "Resource Type";
                    resourceSheet.Cells[1, 2].Value = "Name";

                    int addRow = 2;

                    resourceSheet.Cells[addRow, 1].Value = "Delivery Manager";
                    resourceSheet.Cells[addRow, 2].Value = projectData.DeliveryManager;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Senior Manager";
                    resourceSheet.Cells[addRow, 2].Value = projectData.SeniorManager;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Manager";
                    resourceSheet.Cells[addRow, 2].Value = projectData.Manager;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Project Lead";
                    resourceSheet.Cells[addRow, 2].Value = projectData.ProjectLead;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Dev TeamLead";
                    resourceSheet.Cells[addRow, 2].Value = projectData.DevTeamLead;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Senior Developer";
                    resourceSheet.Cells[addRow, 2].Value = projectData.SeniorDeveloper;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Developer";
                    resourceSheet.Cells[addRow, 2].Value = projectData.Developer;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "QA TeamLead";
                    resourceSheet.Cells[addRow, 2].Value = projectData.QaTeamLead;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Senior Tester";
                    resourceSheet.Cells[addRow, 2].Value = projectData.SeniorTester;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Tester";
                    resourceSheet.Cells[addRow, 2].Value = projectData.Tester;

                    addRow++;

                    resourceSheet.Cells[addRow, 1].Value = "Deployment Team";
                    resourceSheet.Cells[addRow, 2].Value = projectData.DeploymentTeam;

                    addRow++;
                    resourceSheet.Column(1).AutoFit(); //
                    resourceSheet.Column(2).AutoFit(); // 
                    resourceSheet.Column(3).AutoFit(); // 

                    package.SaveAs(new FileInfo(filePath));


                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Excel file");
                throw;
            }

            return filePath;
        }

        //edit
        private void ApplyConditionalFormatting(ExcelWorksheet sheet, int lastRow, DateTime projectStartDate)
        {
            for (int row = 5; row < lastRow; row++)
            {
                string taskName = sheet.Cells[row, 1].Text;
                if (DateTime.TryParse(sheet.Cells[row, 2].Text, out DateTime startDate) &&
                    DateTime.TryParse(sheet.Cells[row, 3].Text, out DateTime endDate))
                {
                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        int col = (date - projectStartDate).Days + 4;
                        var cell = sheet.Cells[row, col];
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(GetTaskColor(taskName));
                    }
                }
            }
        }

        ////edit

        //private void ApplyConditionalFormatting(ExcelWorksheet sheet, int lastRow, DateTime projectStartDate)
        //{
        //    for (int row = 3; row < lastRow; row++)
        //    {
        //        string taskName = sheet.Cells[row, 1].Text;
        //        DateTime startDate = DateTime.Parse(sheet.Cells[row, 2].Text);
        //        DateTime endDate = DateTime.Parse(sheet.Cells[row, 3].Text);

        //        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        //        {
        //            int col = (date - projectStartDate).Days + 4;
        //            var cell = sheet.Cells[row, col];
        //            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //            cell.Style.Fill.BackgroundColor.SetColor(GetTaskColor(taskName));
        //        }
        //    }
        //}
        private System.Drawing.Color GetTaskColor(string taskName)
        {
            return taskName switch
            {
                "Analysis" => System.Drawing.Color.LightBlue,
                "Design" => System.Drawing.Color.LightGreen,
                "Development" => System.Drawing.Color.LightCoral,
                "Testing" => System.Drawing.Color.LightGoldenrodYellow,
                "UAT" => System.Drawing.Color.LightPink,
                "Production" => System.Drawing.Color.LightSkyBlue,
                "BC Date" => System.Drawing.Color.LightSalmon,
                _ => System.Drawing.Color.LightGray,
            };
        }

        private string GeneratePowerPoint(ProjectData projectData)
        {
            var tempPath = Path.GetTempPath();
            var filePath = Path.Combine(tempPath, "ProjectEstimation.pptx");

            try
            {
                // Retry logic for handling file in use scenario
                int retryCount = 3;
                while (retryCount > 0)
                {
                    try
                    {
                        using (PresentationDocument presentationDocument = PresentationDocument.Create(filePath, DocumentFormat.OpenXml.PresentationDocumentType.Presentation))
                        {
                            PresentationPart presentationPart = presentationDocument.AddPresentationPart();
                            presentationPart.Presentation = new Presentation();

                            SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
                            slidePart.Slide = new Slide(new CommonSlideData(new ShapeTree()));

                            SlideLayoutPart slideLayoutPart = slidePart.AddNewPart<SlideLayoutPart>();
                            slideLayoutPart.SlideLayout = new SlideLayout(new CommonSlideData(new ShapeTree()));

                            SlideMasterPart slideMasterPart = slideLayoutPart.AddNewPart<SlideMasterPart>();
                            slideMasterPart.SlideMaster = new SlideMaster(new CommonSlideData(new ShapeTree()));

                            SlideIdList slideIdList = presentationPart.Presentation.AppendChild(new SlideIdList());
                            uint slideId = 256;
                            SlideId slideIdElement = slideIdList.AppendChild(new SlideId());
                            slideIdElement.Id = slideId;
                            slideIdElement.RelationshipId = presentationPart.GetIdOfPart(slidePart);

                            Shape titleShape = slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(new Shape());
                            titleShape.NonVisualShapeProperties = new NonVisualShapeProperties(
                                new NonVisualDrawingProperties() { Id = 1, Name = "Title" },
                                new NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
                                new ApplicationNonVisualDrawingProperties(new PlaceholderShape() { Type = PlaceholderValues.Title }));

                            titleShape.ShapeProperties = new ShapeProperties();
                            titleShape.TextBody = new TextBody(new A.BodyProperties(), new A.ListStyle(),
                                new A.Paragraph(new A.Run(new A.Text("Project Estimation"))));

                            Shape contentShape = slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(new Shape());
                            contentShape.NonVisualShapeProperties = new NonVisualShapeProperties(
                                new NonVisualDrawingProperties() { Id = 2, Name = "Content" },
                                new NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
                                new ApplicationNonVisualDrawingProperties(new PlaceholderShape() { Index = 1 }));

                            contentShape.ShapeProperties = new ShapeProperties();
                            contentShape.TextBody = new TextBody(new A.BodyProperties(), new A.ListStyle(),
                                new A.Paragraph(new A.Run(new A.Text($"Project Start Date: {projectData.ProjectStartDate}"))),
                                new A.Paragraph(new A.Run(new A.Text($"Project End Date: {projectData.ProjectEndDate}"))));

                            foreach (var resource in projectData.Resources)
                            {
                                contentShape.TextBody.AppendChild(new A.Paragraph(new A.Run(new A.Text($"{resource.Name}: {resource.Total}"))));
                            }

                            foreach (var cost in projectData.AdditionalCosts)
                            {
                                contentShape.TextBody.AppendChild(new A.Paragraph(new A.Run(new A.Text($"{cost.Name}: {cost.Total}"))));
                            }

                            presentationPart.Presentation.Save();
                        }
                        break; // Exit the retry loop if successful
                    }
                    catch (IOException ex) when (retryCount > 0)
                    {
                        _logger.LogWarning(ex, "File in use, retrying...");
                        retryCount--;
                        Thread.Sleep(1000); // Wait for 1 second before retrying
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving PowerPoint file");
                throw;
            }

            return filePath;
        }

        private string CreateZipFile(string excelFilePath, string pptFilePath)
        {
            var tempPath = Path.GetTempPath();
            var zipFilePath = Path.Combine(tempPath, $"ProjectEstimation_{DateTime.Now:yyyyMMddHHmmss}.zip");

            try
            {
                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(excelFilePath, Path.GetFileName(excelFilePath));
                    zip.CreateEntryFromFile(pptFilePath, Path.GetFileName(pptFilePath));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZIP file");
                throw;
            }

            return zipFilePath;
        }
        //formatting date
            public static string FormatDate(string? date)
            {
                if (string.IsNullOrEmpty(date))
                {
                    return string.Empty;
                }

                string[] formats = new[]
                {
            "yyyy-MM-dd",
            "dd-MM-yyyy",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "ddd MMM dd yyyy HH:mm:ss 'GMT'K (zzz)",
            "MM/dd/yyyy",
            "dd/MM/yyyy",
            "dd-MM-yyyy HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "dd MMM yyyy",
            "MMM dd, yyyy",
            "dd MMM yyyy HH:mm:ss 'GMT'K",
            "ddd MMM dd yyyy HH:mm:ss 'GMT'K (zzz)",
            "ddd MMM dd yyyy HH:mm: ss 'GMT'K(zzz)"
        };

                if (DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                return date;
            }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using ProjectEstimationApp.Models;
//using OfficeOpenXml;
//using DocumentFormat.OpenXml.Presentation;
//using DocumentFormat.OpenXml.Packaging;
//using A = DocumentFormat.OpenXml.Drawing;
//using System.IO;
//using System.IO.Compression;
//using System.Threading;

//namespace ProjectEstimationApp.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;

//        public HomeController(ILogger<HomeController> logger)
//        {
//            _logger = logger;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult SubmitForm(ProjectEstimation model)
//        {
//            // Store the form data in TempData to pass it to the next page
//            TempData["ProjectEstimation"] = model;

//            // Redirect to the next page
//            return RedirectToAction("Estpage");
//        }

//        public IActionResult Estpage()
//        {
//            var model = TempData["ProjectEstimation"] as ProjectEstimation;
//            return View(model);
//        }

//        public IActionResult Resource()
//        {
//            return View();
//        }

//        public IActionResult Sampletimeline()
//        {
//            return View();
//        }

//        public IActionResult Smbud()
//        {
//            return View();
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        public IActionResult Homepage()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult GenerateFiles([FromBody] ProjectData projectData)
//        {
//            var excelFilePath = GenerateExcel(projectData);
//            var pptFilePath = GeneratePowerPoint(projectData);
//            var zipFilePath = CreateZipFile(excelFilePath, pptFilePath);

//            byte[] fileBytes = System.IO.File.ReadAllBytes(zipFilePath);
//            System.IO.File.Delete(excelFilePath);
//            System.IO.File.Delete(pptFilePath);
//            System.IO.File.Delete(zipFilePath);

//            return File(fileBytes, "application/zip", "ProjectEstimation.zip");
//        }

//        private string GenerateExcel(ProjectData projectData)
//        {
//            // Set the license context for EPPlus
//            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

//            var tempPath = Path.GetTempPath();
//            var filePath = Path.Combine(tempPath, "ProjectEstimation.xlsx");

//            try
//            {
//                using (var package = new ExcelPackage())
//                {
//                    var worksheet = package.Workbook.Worksheets.Add("Project Estimation");
//                    worksheet.Cells[1, 1].Value = "Resource Type";
//                    worksheet.Cells[1, 2].Value = "Cost";
//                    worksheet.Cells[1, 3].Value = "Number of Resources";
//                    worksheet.Cells[1, 4].Value = "Total";

//                    int row = 2;
//                    foreach (var resource in projectData.Resources)
//                    {
//                        worksheet.Cells[row, 1].Value = resource.Name;
//                        worksheet.Cells[row, 2].Value = resource.Cost;
//                        worksheet.Cells[row, 3].Value = resource.NumberOfResources;
//                        worksheet.Cells[row, 4].Value = resource.Total;
//                        row++;
//                    }

//                    worksheet.Cells[row, 1].Value = "Project Start Date";
//                    worksheet.Cells[row, 2].Value = projectData.ProjectStartDate;

//                    worksheet.Cells[row + 1, 1].Value = "Project End Date";
//                    worksheet.Cells[row + 1, 2].Value = projectData.ProjectEndDate;

//                    var additionalCostsSheet = package.Workbook.Worksheets.Add("Additional Costs");
//                    additionalCostsSheet.Cells[1, 1].Value = "Name";
//                    additionalCostsSheet.Cells[1, 2].Value = "Cost";
//                    additionalCostsSheet.Cells[1, 3].Value = "Number of Resources";
//                    additionalCostsSheet.Cells[1, 4].Value = "Total";

//                    int additionalRow = 2;
//                    foreach (var cost in projectData.AdditionalCosts)
//                    {
//                        additionalCostsSheet.Cells[additionalRow, 1].Value = cost.Name;
//                        additionalCostsSheet.Cells[additionalRow, 2].Value = cost.Cost;
//                        additionalCostsSheet.Cells[additionalRow, 3].Value = cost.NumberOfResources;
//                        additionalCostsSheet.Cells[additionalRow, 4].Value = cost.Total;
//                        additionalRow++;
//                    }

//                    package.SaveAs(new FileInfo(filePath));
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error saving Excel file");
//                throw;
//            }

//            return filePath;
//        }

//        private string GeneratePowerPoint(ProjectData projectData)
//        {
//            var tempPath = Path.GetTempPath();
//            var filePath = Path.Combine(tempPath, "ProjectEstimation.pptx");

//            try
//            {
//                // Retry logic for handling file in use scenario
//                int retryCount = 3;
//                while (retryCount > 0)
//                {
//                    try
//                    {
//                        using (PresentationDocument presentationDocument = PresentationDocument.Create(filePath, DocumentFormat.OpenXml.PresentationDocumentType.Presentation))
//                        {
//                            PresentationPart presentationPart = presentationDocument.AddPresentationPart();
//                            presentationPart.Presentation = new Presentation();

//                            SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
//                            slidePart.Slide = new Slide(new CommonSlideData(new ShapeTree()));

//                            SlideLayoutPart slideLayoutPart = slidePart.AddNewPart<SlideLayoutPart>();
//                            slideLayoutPart.SlideLayout = new SlideLayout(new CommonSlideData(new ShapeTree()));

//                            SlideMasterPart slideMasterPart = slideLayoutPart.AddNewPart<SlideMasterPart>();
//                            slideMasterPart.SlideMaster = new SlideMaster(new CommonSlideData(new ShapeTree()));

//                            SlideIdList slideIdList = presentationPart.Presentation.AppendChild(new SlideIdList());
//                            uint slideId = 256;
//                            SlideId slideIdElement = slideIdList.AppendChild(new SlideId());
//                            slideIdElement.Id = slideId;
//                            slideIdElement.RelationshipId = presentationPart.GetIdOfPart(slidePart);

//                            Shape titleShape = slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(new Shape());
//                            titleShape.NonVisualShapeProperties = new NonVisualShapeProperties(
//                                new NonVisualDrawingProperties() { Id = 1, Name = "Title" },
//                                new NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
//                                new ApplicationNonVisualDrawingProperties(new PlaceholderShape() { Type = PlaceholderValues.Title }));

//                            titleShape.ShapeProperties = new ShapeProperties();
//                            titleShape.TextBody = new TextBody(new A.BodyProperties(), new A.ListStyle(),
//                                new A.Paragraph(new A.Run(new A.Text("Project Estimation"))));

//                            Shape contentShape = slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(new Shape());
//                            contentShape.NonVisualShapeProperties = new NonVisualShapeProperties(
//                                new NonVisualDrawingProperties() { Id = 2, Name = "Content" },
//                                new NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
//                                new ApplicationNonVisualDrawingProperties(new PlaceholderShape() { Index = 1 }));

//                            contentShape.ShapeProperties = new ShapeProperties();
//                            contentShape.TextBody = new TextBody(new A.BodyProperties(), new A.ListStyle(),
//                                new A.Paragraph(new A.Run(new A.Text($"Project Start Date: {projectData.ProjectStartDate}"))),
//                                new A.Paragraph(new A.Run(new A.Text($"Project End Date: {projectData.ProjectEndDate}"))));

//                            foreach (var resource in projectData.Resources)
//                            {
//                                contentShape.TextBody.AppendChild(new A.Paragraph(new A.Run(new A.Text($"{resource.Name}: {resource.Total}"))));
//                            }

//                            foreach (var cost in projectData.AdditionalCosts)
//                            {
//                                contentShape.TextBody.AppendChild(new A.Paragraph(new A.Run(new A.Text($"{cost.Name}: {cost.Total}"))));
//                            }

//                            presentationPart.Presentation.Save();
//                        }
//                        break; // Exit the retry loop if successful
//                    }
//                    catch (IOException ex) when (retryCount > 0)
//                    {
//                        _logger.LogWarning(ex, "File in use, retrying...");
//                        retryCount--;
//                        Thread.Sleep(1000); // Wait for 1 second before retrying
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error saving PowerPoint file");
//                throw;
//            }

//            return filePath;
//        }

//        private string CreateZipFile(string excelFilePath, string pptFilePath)
//        {
//            var tempPath = Path.GetTempPath();
//            var zipFilePath = Path.Combine(tempPath, $"ProjectEstimation_{DateTime.Now:yyyyMMddHHmmss}.zip");

//            try
//            {
//                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
//                {
//                    zip.CreateEntryFromFile(excelFilePath, Path.GetFileName(excelFilePath));
//                    zip.CreateEntryFromFile(pptFilePath, Path.GetFileName(pptFilePath));
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating ZIP file");
//                throw;
//            }

//            return zipFilePath;
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}