using beta.Models.API;
using beta.Models.Enums;
using DevExpress.Office.Utils;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    internal class ExportService : Interfaces.IExportService
    {
        public async Task ExportMaps(ApiMapData[] maps)
        {
            string fileName = "Exported maps.docx";
            using (var wordProcessor = new RichEditDocumentServer())
            {
                Document document = wordProcessor.Document;
                document.Sections[0].Page.Landscape = true;
                // Create a new table
                Table table = document.Tables.Create(document.Range.End, 1 + maps.Length, 7);
                table.BeginUpdate();

                table.TableAlignment = TableRowAlignment.Both;
                table.TableLayout = TableLayoutType.Autofit;
                //table.PreferredWidthType = WidthType.Fixed;
                //table.PreferredWidth = Units.InchesToDocumentsF(1f);
                // Set the width of the first column
                TableCell cellOne = table.Rows[0].FirstCell;
                //firstCell.PreferredWidthType = WidthType.Fixed;
                //firstCell.PreferredWidth = Units.InchesToDocumentsF(0.8f);


                // Set the second column width and cell height
                TableCell cellTwo = table.Rows[1].FirstCell;
                //firstColumnCell.PreferredWidthType = WidthType.Fixed;
                //firstColumnCell.PreferredWidth = Units.InchesToDocumentsF(5f);
                //firstColumnCell.HeightType = HeightType.Exact;
                //firstColumnCell.Height = Units.InchesToDocumentsF(0.5f);

                // Set the third column width
                TableCell cellThree = table.Rows[2].FirstCell;
                //lastCell.PreferredWidthType = WidthType.Fixed;
                //lastCell.PreferredWidth = Units.InchesToDocumentsF(0.8f);

                TableCell cellFour = table.Rows[3].FirstCell;
                TableCell cellFive = table.Rows[4].FirstCell;
                TableCell cellSix = table.Rows[5].FirstCell;
                TableCell cellSeven = table.Rows[6].FirstCell;

                document.InsertSingleLineText(table[0, 0].Range.Start, "Изображение");
                document.InsertSingleLineText(table[0, 1].Range.Start, "Наименование");
                document.InsertSingleLineText(table[0, 2].Range.Start, "Количество позиций");
                document.InsertSingleLineText(table[0, 3].Range.Start, "Размер");
                document.InsertSingleLineText(table[0, 4].Range.Start, "Автор");
                document.InsertSingleLineText(table[0, 5].Range.Start, "Рейтинг");
                document.InsertSingleLineText(table[0, 6].Range.Start, "Количество игр");

                var pathToCache = App.GetPathToFolder(Folder.MapsLargePreviews);

                for (int i = 0; i < maps.Length; i++)
                {
                    var map = maps[i];
                    var rowIndex = i + 1;

                    var pathToFile = pathToCache + map.FolderName + ".png";

                    if (map.MapLargePreview is BitmapImage img)
                    {
                        Shape picture = null;
                        if (File.Exists(img.UriSource.LocalPath))
                        {
                            picture = document.Shapes.InsertPicture(table[rowIndex, 0].Range.Start,
                                DocumentImageSource.FromFile(img.UriSource.LocalPath));
                            picture.TextWrapping = TextWrappingType.InLineWithText;
                            picture.Width = 256;
                            picture.Height = 256;
                        }
                        else
                        {
                            //picture = document.Shapes.InsertPicture(table[rowIndex, 0].Range.Start,
                            //    DocumentImageSource.FromFile(pathToFile));
                        }
                    }

                    document.InsertSingleLineText(table[rowIndex, 1].Range.Start, map.DisplayedName);
                    document.InsertSingleLineText(table[rowIndex, 2].Range.Start, map.MaxPlayers);
                    document.InsertSingleLineText(table[rowIndex, 3].Range.Start, map.MapSize);
                    document.InsertSingleLineText(table[rowIndex, 4].Range.Start, map.AuthorLogin);
                    document.InsertSingleLineText(table[rowIndex, 5].Range.Start, Math.Round(map.SummaryFiveRate == -1 ? 0
                        : map.SummaryFiveRate, 1).ToString());
                    document.InsertSingleLineText(table[rowIndex, 6].Range.Start, map.GamesPlayed);
                }
                table.EndUpdate();
                wordProcessor.SaveDocument(fileName, DocumentFormat.OpenXml);
            }
            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
        }
    }
}
