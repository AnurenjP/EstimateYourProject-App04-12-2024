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

            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Project Estimation");
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

                    worksheet.Cells[row, 1].Value = "Project Start Date";
                    worksheet.Cells[row, 2].Value = projectData.ProjectStartDate;

                    worksheet.Cells[row + 1, 1].Value = "Project End Date";
                    worksheet.Cells[row + 1, 2].Value = projectData.ProjectEndDate;

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