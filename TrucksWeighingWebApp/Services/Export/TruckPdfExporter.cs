using DocumentFormat.OpenXml.Spreadsheet;
using EllipticCurve.Utils;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TrucksWeighingWebApp.DTOs.Export;

namespace TrucksWeighingWebApp.Services.Export
{
    public class TruckPdfExporter : IDocument
    {
        private readonly TrucksExcelDto _dto;
        private readonly byte[]? _logoBytes;

        public TruckPdfExporter(TrucksExcelDto dto, byte[]? logoBytes)
        {
            _dto = dto;
            _logoBytes = logoBytes;
        }

        // Global styles
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        // Header
                        //col.Item().ShowEntire().Element(ComposeHeader);



                        col.Item().Decoration(dec =>
                        {
                            dec.Before().ShowOnce().Element(ComposeHeader);
                            dec.Content().Element(ComposeContent);
                            
                        });

                        col.Item().ShowOnce().Element(ComposeSummary);
                    });
                });               
                
                page.Footer().Element(ComposePageNumbers);

                
            });
        }

        

        void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                if (_logoBytes is not null)
                {
                    col.Item()                        
                        .PaddingBottom(15)
                        .AlignLeft()
                        .Height(50)
                        .Image(_logoBytes)
                        .FitHeight();
                }



                col.Item().Text("TALLY SHEET")
                    .SemiBold()
                    .FontSize(16);

                col.Item().Text(txt =>
                {
                    txt.Span("Vessel: ").SemiBold();
                    txt.Span(_dto.Inspection.Vessel ?? "-");
                });

                col.Item().Text(txt =>
                {
                    txt.Span("Port: ").SemiBold();
                    txt.Span(_dto.Inspection.Place ?? "-");
                });

                if (_dto.Inspection.DeclaredTotalWeight.HasValue)
                {
                    col.Item().Text(txt =>
                    {
                        txt.Span("B/L figure: ").SemiBold();
                        txt.Span($"{_dto.Inspection.DeclaredTotalWeight.Value:F3}");
                        txt.Span(" mt");
                    });
                }
            });            
        }

        void ComposeContent(IContainer container)
        {
            container
                .PaddingTop(20)
                .Table(table =>
                {
                // cols
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(28); // #
                    cols.RelativeColumn(3); // PlateNumber
                    cols.RelativeColumn(2); // Initial Weighing
                    if (_dto.ShowTimes) cols.RelativeColumn(3); // Initial Weighing Time
                    cols.RelativeColumn(2); // Final Weighing
                    if (_dto.ShowTimes) cols.RelativeColumn(3); // Final Weighing Time
                    cols.RelativeColumn(2);
                });

                // header
                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("#");
                    header.Cell().Element(CellHeader).Text("TRUCKS");
                    header.Cell().Element(CellHeader).Text("INITIAL WEIGHING, MT");
                    if (_dto.ShowTimes) header.Cell().Element(CellHeader).Text("DATE & TIME");
                    header.Cell().Element(CellHeader).Text("FINAL WEIGHING, MT");
                    if (_dto.ShowTimes) header.Cell().Element(CellHeader).Text("DATE & TIME");
                    header.Cell().Element(CellHeader).Text("NET, MT");
                });

                // rows
                foreach (var item in _dto.RowsDto)
                {

                    table.Cell().Element(CellBody).AlignCenter().Text(item.SerialNumber.ToString());
                    table.Cell().Element(CellBody).AlignCenter().Text(item.PlateNumber.ToString());
                    table.Cell().Element(CellBody).AlignCenter().Text(item.InitialWeight?.ToString("F3") ?? "-");
                    if (_dto.ShowTimes) table.Cell().Element(CellBody).AlignCenter().Text(item.InitialWeighingLocal?.ToString("yyyy-MM-dd HH:mm") ?? "-");
                    table.Cell().Element(CellBody).AlignCenter().Text(item.FinalWeight?.ToString("F3") ?? "-");
                    if (_dto.ShowTimes) table.Cell().Element(CellBody).AlignCenter().Text(item.FinalWeighingLocal?.ToString("yyyy-MM-dd HH:mm") ?? "-");
                    table.Cell().Element(CellBody).AlignCenter().Text(item.NetWeight.ToString("F3") ?? "-");

                }

                // helpers for cell
                IContainer CellHeader(IContainer c) => c
                    .PaddingVertical(4)
                    .PaddingHorizontal(6)
                    .DefaultTextStyle(x => x.SemiBold());

                IContainer CellBody(IContainer c) => c
                    .PaddingVertical(3)
                    .PaddingHorizontal(6)
                    .BorderBottom(0.5f);
                });
        }

        void ComposeSummary(IContainer container)
        {
            container                
                .PaddingTop(15)
                .Column(col =>
                {                    
                    col.Item().Text(txt =>
                    {
                        txt.Span("Trucks weight control figure: ").SemiBold();
                        txt.Span($"{_dto.Inspection.WeighedTotalWeight:F3} mt");
                    });
                    if (_dto.Inspection.DeclaredTotalWeight.HasValue)
                    {
                        col.Item().Text($"Difference: {_dto.Inspection.DifferenceWeight:F3} mt or {_dto.Inspection.DifferencePercent:F3} %");
                    }
                });
        }

        void ComposePageNumbers(IContainer container)
        {
            container.AlignRight().Text(t =>
            {
                t.Span("Page ");
                t.CurrentPageNumber();
                t.Span(" / ");
                t.TotalPages();
            });
        }
    }
}
