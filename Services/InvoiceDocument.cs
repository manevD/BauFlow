using BauFlow.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BauFlow.Services
{
    public class InvoiceDocument : IDocument
    {
        private readonly Invoice _invoice;
        private readonly Company _company;

        public InvoiceDocument(Invoice invoice, Company company)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseEnvironmentFonts = true;

            _invoice = invoice
                ?? throw new Exception("Invoice missing");

            _company = company
                ?? throw new Exception("Company missing");
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(35);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x =>
                    x.FontSize(10)
                     .FontFamily("Arial"));

                page.Header().Element(ComposeHeader);

                page.Content()
                    .PaddingVertical(20)
                    .Element(ComposeContent);

                page.Footer()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span(_company.Name);
                        text.Span($" • Фактура {_invoice.InvoiceNumber}");
                    });
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Spacing(5);

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_company.LogoPath)
                            && File.Exists(_company.LogoPath))
                        {
                            var logoBytes = File.ReadAllBytes(_company.LogoPath);

                            column.Item()
                                .Height(60)
                                .Image(logoBytes);
                        }
                    }
                    catch
                    {
                        // Live server: ignore bad logo
                    }

                    column.Item()
                        .Text(_company.Name)
                        .Bold()
                        .FontSize(22)
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Text(_company.Address ?? "");
                    column.Item()
                        .Text($"{_company.PostalCode ?? ""} {_company.City ?? ""}");
                    if (!string.IsNullOrWhiteSpace(_company.TaxNumber))
                        column.Item()
                            .Text($"Даночен број: {_company.TaxNumber}");

                    column.Item().Text(_company.IBAN ?? "");
                    column.Item().Text(_company.BankName ?? "");
                });

                row.ConstantItem(220)
                    .Background(Colors.Blue.Lighten5)
                    .CornerRadius(8)
                    .Padding(15)
                    .Column(column =>
                    {
                        column.Spacing(6);

                        column.Item()
                            .Text("ФАКТУРА")
                            .Bold()
                            .FontSize(20)
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().Text($"Број: {_invoice.InvoiceNumber}");
                        column.Item().Text($"Датум: {_invoice.InvoiceDate:dd.MM.yyyy}");
                        column.Item().Text($"Рок: {_invoice.DueDate:dd.MM.yyyy}");
                    });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(20);

                column.Item().Element(ComposeCustomer);

                column.Item().Element(ComposeTable);

                column.Item()
                    .AlignRight()
                    .Element(ComposeTotals);

                column.Item()
                    .Element(ComposePaymentInfo);
            });
        }

        void ComposeCustomer(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .CornerRadius(8)
                .Padding(15)
                .Column(column =>
                {
                    column.Spacing(5);

                    column.Item()
                        .Text("Фактура за")
                        .Bold()
                        .FontSize(12);

                    column.Item().Text(_invoice.Customer?.Name);

                    column.Item().Text(_invoice.Customer?.Address);

                    column.Item()
                        .Text($"{_invoice.Customer?.PostalCode} {_invoice.Customer?.City}");
                });
        }

        void ComposeTable(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .CornerRadius(6)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); // опис
                        columns.RelativeColumn(1); // количина
                        columns.RelativeColumn(2); // цена
                        columns.RelativeColumn(2); // ДДВ
                        columns.RelativeColumn(2); // вкупно
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle)
                            .Text("Опис").Bold();

                        header.Cell().Element(HeaderCellStyle)
                            .AlignCenter()
                            .Text("Кол.");

                        header.Cell().Element(HeaderCellStyle)
                            .AlignRight()
                            .Text("Цена");

                        header.Cell().Element(HeaderCellStyle)
                            .AlignRight()
                            .Text("ДДВ");

                        header.Cell().Element(HeaderCellStyle)
                            .AlignRight()
                            .Text("Вкупно");
                    });

                    int rowIndex = 0;

                    foreach (var item in _invoice.Items ?? new List<InvoiceItem>())
                    {
                        var lineTotal = item.TotalPrice > 0
                            ? item.TotalPrice
                            : item.Quantity * item.UnitPrice;

                        var itemTax = lineTotal *
                                      (_invoice.TaxRate / 100m);

                        var bg = rowIndex % 2 == 0
                            ? Colors.White
                            : Colors.Grey.Lighten5;

                        table.Cell()
                            .Background(bg)
                            .Element(CellStyle)
                            .Text(item.Description ?? "");

                        table.Cell()
                            .Background(bg)
                            .Element(CellStyle)
                            .AlignCenter()
                            .Text($"{item.Quantity} {item.Unit}");

                        table.Cell()
                            .Background(bg)
                            .Element(CellStyle)
                            .AlignRight()
                            .Text($"{item.UnitPrice:0.00}");

                        table.Cell()
                            .Background(bg)
                            .Element(CellStyle)
                            .AlignRight()
                            .DefaultTextStyle(x => x.FontColor(Colors.Blue.Darken2))
                            .Text($"{itemTax:0.00}");

                        table.Cell()
                            .Background(bg)
                            .Element(CellStyle)
                            .AlignRight()
                            .DefaultTextStyle(x => x.Bold())
                            .Text($"{lineTotal:0.00} МКД");

                        rowIndex++;
                    }
                });
        }

        void ComposeTotals(IContainer container)
        {
            var tax = CalculateInvoiceTax(_invoice);

            container
                .Width(280)
                .Background(Colors.Grey.Lighten5)
                .CornerRadius(8)
                .Padding(15)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Column(column =>
                {
                    column.Spacing(8);

                    AddTotalRow(column, "Нето", _invoice.NetAmount);

                    AddTotalRow(column, "ДДВ", tax);

                    column.Item().LineHorizontal(1);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Text("ВКУПНО")
                            .Bold()
                            .FontSize(16);

                        row.ConstantItem(120)
                            .AlignRight()
                            .Text($"{_invoice.GrossAmount:0.00} МКД")
                            .Bold()
                            .FontSize(16)
                            .FontColor(Colors.Blue.Darken2);
                    });
                });
        }

        void AddTotalRow(
            ColumnDescriptor column,
            string title,
            decimal amount)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(title);

                row.ConstantItem(120)
                    .AlignRight()
                    .Text($"{amount:0.00} МКД");
            });
        }

        void ComposePaymentInfo(IContainer container)
        {
            container
                .Background(Colors.Blue.Lighten5)
                .CornerRadius(8)
                .Padding(15)
                .Row(row =>
                {
                    // Лево - опис
                    row.RelativeItem()
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item()
                                .Text("Опис")
                                .Bold()
                                .FontSize(12);

                            column.Item()
                                .Text(_invoice.Description ?? "");
                        });

                    row.ConstantItem(40);

                    // Десно - потпис
                    row.RelativeItem()
                        .AlignRight()
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item()
                                .AlignCenter()
                                .Text("Овластено лице")
                                .Bold();

                            column.Item()
                                .PaddingTop(20)
                                .AlignCenter()
                                .Width(160)
                                .Column(signature =>
                                {
                                    signature.Item()
                                        .AlignCenter()
                                        .Text(_company.CEO ?? "")
                                        .Bold();

                                    signature.Item()
                                        .LineHorizontal(1);

                                    signature.Item()
                                        .PaddingTop(3)
                                        .AlignCenter()
                                        .Text("Име и презиме")
                                        .FontSize(9)
                                        .FontColor(Colors.Grey.Darken1);
                                });
                        });
                });
        }

        static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Background(Colors.Blue.Darken2)
                .DefaultTextStyle(x => x.FontColor(Colors.White))
                .PaddingVertical(10)
                .PaddingHorizontal(8);
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .PaddingVertical(10)
                .PaddingHorizontal(8)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2);
        }

        public decimal CalculateInvoiceTax(
            Invoice invoice,
            bool pricesIncludeTax = false)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            if (invoice.Items == null || !invoice.Items.Any())
                return 0m;

            if (invoice.TaxRate <= 0)
                return 0m;

            decimal taxRate = invoice.TaxRate / 100m;

            var totalTax = invoice.Items.Sum(item =>
            {
                decimal lineTotal =
                    item.TotalPrice > 0
                    ? item.TotalPrice
                    : item.Quantity * item.UnitPrice;

                if (lineTotal <= 0)
                    return 0m;

                if (pricesIncludeTax)
                {
                    return lineTotal -
                           (lineTotal / (1 + taxRate));
                }

                return lineTotal * taxRate;
            });

            return Math.Round(
                totalTax,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}