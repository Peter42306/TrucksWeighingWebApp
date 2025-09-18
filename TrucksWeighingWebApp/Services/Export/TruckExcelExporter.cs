using ClosedXML.Excel;
using TrucksWeighingWebApp.DTOs.Export;

namespace TrucksWeighingWebApp.Services.Export
{
    public class TruckExcelExporter : ITruckExcelExporter
    {
        public byte[] BuildTruckWorkbook(TrucksExcelDto dto)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Tally Sheet");

            var showTimes = dto.ShowTimes;
            int colCount = showTimes ? 7 : 5; // if show times 7 cols, if not 5 cols
            int row = 1;

            // Head
            ws.Cell(row, 1).Value = "TALLY SHEET";
            ws.Range(row, 1, row, colCount).Merge()
                .Style.Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row += 2;

            int headerRow = row;
            int cell = 1;
            ws.Cell(headerRow, cell++).Value = "#";
            ws.Cell(headerRow, cell++).Value = "TRUCKS";
            ws.Cell(headerRow, cell++).Value = "INITIAL WEIGHT, MT";
            if (showTimes) ws.Cell(headerRow, cell++).Value = "TIME (INITIAL)";
            ws.Cell(headerRow, cell++).Value = "FINAL WEIGHT, MT";
            if (showTimes) ws.Cell(headerRow, cell++).Value = "TIME (FINAL)";
            ws.Cell(headerRow, cell++).Value = "NET, MT";

            ws.Range(headerRow, 1, headerRow, colCount).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#dbe5f1"))
                .Border.SetBottomBorder(XLBorderStyleValues.Thin);

            row++;


            // Data
            int dataStart = row;
            foreach (var item in dto.RowsDto)
            {
                cell = 1;
                ws.Cell(row, cell++).Value = item.SerialNumber;
                ws.Cell(row, cell++).Value = item.PlateNumber;

                ws.Cell(row, cell++).Value = item.InitialWeight;
                ws.Cell(row, cell++).Style.NumberFormat.Format = "0.000";
                cell++;

                if (showTimes)
                {
                    if (item.InitialWeighingLocal.HasValue)
                    {
                        ws.Cell(row, cell).Value = item.InitialWeighingLocal.Value;
                        ws.Cell(row, cell).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                    }
                    cell++;
                }

                ws.Cell(row, cell++).Value = item.FinalWeight;
                ws.Cell(row, cell++).Style.NumberFormat.Format = "0.000";
                cell++;

                if (showTimes)
                {
                    if (item.FinalWeighingLocal.HasValue)
                    {
                        ws.Cell(row, cell).Value = item.FinalWeighingLocal.Value;
                        ws.Cell(row, cell).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                    }
                    cell++;
                }

                ws.Cell(row, cell++).Value = item.NetWeight;
                ws.Cell(row, cell++).Style.NumberFormat.Format = "0.000";

                row++;
            }

            // Result row 
            ws.Cell(row, 1).Value = "TOTAL, MT";
            ws.Range(row, 1, row, colCount - 1).Merge().Style
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            int netCol = colCount; // последняя колонка — Net
            if (row > dataStart)
            {
                var startAddr = ws.Cell(dataStart, netCol).Address.ToString();
                var endAddr = ws.Cell(row - 1, netCol).Address.ToString();
                ws.Cell(row, netCol).FormulaA1 = $"SUM({startAddr}:{endAddr})";
            }

            ws.Cell(row, netCol).Style.NumberFormat.Format = "0.000";
            ws.Row(row).Style.Border.TopBorder = XLBorderStyleValues.Thin;

            // General view
            ws.SheetView.FreezeRows(headerRow);
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 5;                 // #
            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 12); // Plate                                                                   
            ws.Column(colCount).Width = Math.Max(ws.Column(colCount).Width, 12);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            return ms.ToArray();
        }

    }
}
