using Domain.Interfaces.Interfaces.Infrastructure;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using iText.Layout.Element;
using System.Drawing;
using System.Drawing.Imaging;
using Infrastructure.Services.PdfHelpers;
using Image = iText.Layout.Element.Image;

namespace Infrastructure.Services.Services
{
    public class PdfProcessingService : IPdfProcessingService
    {
        public void SignAndSavePdf(Stream sourceStream, string outputPath, Bitmap qrImage, string publicUrl, string fontPath)
        {
            sourceStream.Position = 0;

            float pageHeight;
            float lowestY;
            PageSize pageSize;

            using (var reader = new PdfReader(sourceStream))
            {
                reader.SetCloseStream(false);
                using (var pdfDoc = new PdfDocument(reader))
                {
                    var lastPage = pdfDoc.GetLastPage();
                    pageSize = new PageSize(lastPage.GetPageSizeWithRotation());
                    pageHeight = pageSize.GetHeight();
                    lowestY = MeasureLowestY(lastPage, pdfDoc);
                }
            }

            sourceStream.Position = 0;

            float overlayHeight = 130;
            float bottomMargin = 120;
            bool enoughSpace = (pageHeight - lowestY - bottomMargin) >= overlayHeight;

            using var writer = new PdfWriter(outputPath);
            using var resultDoc = new PdfDocument(writer);

            using (var reader = new PdfReader(sourceStream))
            using (var originalDoc = new PdfDocument(reader))
            {
                for (int i = 1; i <= originalDoc.GetNumberOfPages(); i++)
                    resultDoc.AddPage(originalDoc.GetPage(i).CopyTo(resultDoc));
            }

            PdfPage targetPage = enoughSpace && lowestY > bottomMargin + 50
                ? resultDoc.GetLastPage()
                : resultDoc.AddNewPage(pageSize);

            GenerateOverlayPdf(targetPage, qrImage, publicUrl, pageHeight, enoughSpace, fontPath);
        }

        private float MeasureLowestY(PdfPage page, PdfDocument doc)
        {
            float lowestY = page.GetPageSizeWithRotation().GetTop();
            bool contentFound = false;
            float pageBottom = page.GetPageSizeWithRotation().GetBottom();
            float pageTop = page.GetPageSizeWithRotation().GetTop();

            var textStrategy = new CustomTextExtractionStrategy();
            new PdfCanvasProcessor(textStrategy).ProcessPageContent(page);
            foreach (var bbox in textStrategy.TextBboxes)
            {
                float y = bbox.GetBottom();
                if (y > pageBottom && y < pageTop - 10 && y < lowestY)
                {
                    lowestY = y;
                    contentFound = true;
                }
            }

            var imageStrategy = new ImageRenderListener();
            new PdfCanvasProcessor(imageStrategy).ProcessPageContent(page);
            foreach (var bbox in imageStrategy.ImageBboxes)
            {
                float y = bbox.GetBottom();
                if (y > pageBottom && y < pageTop - 10 && y < lowestY)
                {
                    lowestY = y;
                    contentFound = true;
                }
            }

            foreach (var annotation in page.GetAnnotations())
            {
                var rect = annotation.GetRectangle()?.ToRectangle();
                if (rect != null)
                {
                    float y = rect.GetBottom();
                    if (y > pageBottom && y < pageTop - 10 && y < lowestY)
                    {
                        lowestY = y;
                        contentFound = true;
                    }
                }
            }

            var form = doc.GetCatalog().GetPdfObject().GetAsDictionary(PdfName.AcroForm);
            var fields = form?.GetAsArray(PdfName.Fields);
            if (fields != null)
            {
                for (int i = 0; i < fields.Size(); i++)
                {
                    var field = fields.GetAsDictionary(i);
                    var rect = field?.GetAsArray(PdfName.Rect);
                    if (rect != null && rect.Size() == 4)
                    {
                        float y = rect.GetAsNumber(1).FloatValue();
                        if (y > pageBottom && y < pageTop - 10 && y < lowestY)
                        {
                            lowestY = y;
                            contentFound = true;
                        }
                    }
                }
            }

            return contentFound && lowestY > pageBottom ? lowestY : pageBottom + 50;
        }

        private void GenerateOverlayPdf(PdfPage page, Bitmap qrImage, string publicUrl, float height, bool placeAtBottom, string fontPath)
        {
            var canvas = new Canvas(page, page.GetPageSize());
            var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

            float width = page.GetPageSize().GetWidth();
            float tableWidth = width * 0.85f;
            float leftMargin = (width - tableWidth) / 2;
            float[] colWidths = { tableWidth * 0.15f, tableWidth * 0.7f, tableWidth * 0.15f };

            var table = new Table(colWidths);
            table.AddCell(new Cell().Add(new Paragraph("Потпишува:\nДатум и време:\nВерификација:").SetFont(font).SetFontSize(8)));

            string details = $"Факултет за информатички науки и компјутерско инженерство\n{DateTime.Now:dd.MM.yyyy HH:mm}\nИнформации за верификација на автентичноста на овој документ се достапни со користење на кодот за верификација (QR-кодот) односно на линкот подолу.";
            table.AddCell(new Cell().Add(new Paragraph(details).SetFont(font).SetFontSize(8)));

            using var qrStream = new MemoryStream();
            qrImage.Save(qrStream, ImageFormat.Png);
            var qrItext = new Image(ImageDataFactory.Create(qrStream.ToArray())).SetWidth(80).SetHeight(80);
            table.AddCell(new Cell().Add(qrItext).SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER));

            table.AddCell(new Cell(1, 3).Add(new Paragraph(publicUrl).SetFont(font).SetFontSize(8).SetFontColor(iText.Kernel.Colors.ColorConstants.BLUE)));
            table.AddCell(new Cell(1, 3).Add(new Paragraph("Овој документ е официјално потпишан со електронски печат и електронски временски жиг. Автентичноста на печатените копии од овој документ можат да бидат електронски верификувани.").SetFont(font).SetFontSize(8)));

            float y = placeAtBottom ? 20 : height - 130;
            canvas.Add(table.SetMarginLeft(leftMargin).SetFixedPosition(leftMargin, y, tableWidth));
            canvas.Close();
        }
    }
}