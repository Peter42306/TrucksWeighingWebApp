using DocumentFormat.OpenXml.Spreadsheet;
using EllipticCurve.Utils;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TrucksWeighingWebApp.DTOs.Export;
using Colors = QuestPDF.Helpers.Colors;

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
                page.MarginTop(24);
                page.MarginBottom(24);
                page.MarginLeft(40);
                page.MarginRight(24);
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
            var vessel = _dto.Inspection.Vessel ?? "-";
            var place = _dto.Inspection.Place ?? "-";


            container.Column(col =>
            {
                if (_logoBytes is not null)
                {
                    col.Item()                        
                        .PaddingBottom(20)
                        .AlignLeft()
                        .Height(50)
                        .Image(_logoBytes)
                        .FitHeight();
                }



                col.Item().AlignCenter().Text("TALLY SHEET")
                    .SemiBold()
                    .FontSize(16);

                col.Item().Text(txt =>
                {
                    txt.Span("Vessel: ").SemiBold();
                    txt.Span(vessel.ToUpper() ?? "-");
                });

                col.Item().Text(txt =>
                {
                    txt.Span("Port: ").SemiBold();
                    txt.Span(place.ToUpper() ?? "-");
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

                col.Item().PaddingTop(10).LineHorizontal(0.25f).LineColor(Colors.Grey.Medium);
                

                //col.Item().LineHorizontal(0.25f).LineColor(Colors.Grey.Medium);
                //col.Item().Text("Some test text 2");

                //col.Item().LineHorizontal(0.5f).LineColor(Colors.Black); 
                //col.Item().Text("Some test text 3");

                //col.Item().LineHorizontal(0.25f).LineColor(Colors.Black);
                //col.Item().Text("Some test text 4");                
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
                    header.Cell().Element(CellHeader).AlignCenter().Text("#");
                    header.Cell().Element(CellHeader).AlignCenter().Text("TRUCKS");
                    header.Cell().ColumnSpan(2).Element(CellHeader).AlignCenter().Text("INITIAL WEIGHT, DATE & TIME");
                    //if (_dto.ShowTimes) header.Cell().Element(CellHeader).Text("DATE & TIME");
                    header.Cell().ColumnSpan(2).Element(CellHeader).AlignCenter().Text("FINAL WEIGHT, DATE & TIME");
                    //if (_dto.ShowTimes) header.Cell().Element(CellHeader).Text("DATE & TIME");
                    header.Cell().Element(CellHeader).AlignCenter().Text("NET, MT");
                    header.Cell().ColumnSpan(7).LineHorizontal(0.25f).LineColor(Colors.Grey.Medium);
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
                        table.Cell().ColumnSpan(7).LineHorizontal(0.25f).LineColor(Colors.Grey.Medium);


                }

                // helpers for cell
                IContainer CellHeader(IContainer c) => c
                    .PaddingVertical(4)
                    .PaddingHorizontal(6)
                    .DefaultTextStyle(x => x.SemiBold());

                IContainer CellBody(IContainer c) => c
                    .PaddingVertical(3)
                    .PaddingHorizontal(6);
                });
        }

        void ComposeSummary(IContainer container)
        {
            container                
                .PaddingTop(15)
                .ShowEntire()
                .Column(col =>
                {
                    //col.Item().PaddingBottom(10).LineHorizontal(0.25f).LineColor(Colors.Grey.Medium);
                    col.Item().Text(txt =>
                    {
                        txt.Span("B/L figure: ");
                        txt.Span($"{_dto.Inspection.DeclaredTotalWeight:F3} mt");
                    });

                    col.Item().Text(txt =>
                    {                        
                        txt.Span("Trucks weight control figure: ");
                        txt.Span($"{_dto.Inspection.WeighedTotalWeight:F3} mt");
                    });

                    if (_dto.Inspection.DeclaredTotalWeight.HasValue)
                    {
                        col.Item().Text($"Difference: {_dto.Inspection.DifferenceWeight:F3} mt or {_dto.Inspection.DifferencePercent:F3} %");
                    }

                    // Sugnatures
                    col.Item().PaddingTop(30).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        table.Cell().Text("For Terminal");
                        table.Cell().AlignRight().Text("Surveyor");                        
                    });

                    col.Item().PaddingTop(40).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(3);
                        });                        

                        table.Cell().LineHorizontal(0.25f).LineColor(Colors.Grey.Medium);
                        table.Cell().Text("");
                        table.Cell().LineHorizontal(0.25f).LineColor(Colors.Grey.Medium);
                    });
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
