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

            _invoice = invoice ?? throw new Exception("Invoice missing");
            _company = company ?? throw new Exception("Company missing");
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
                    .AlignCenter()
                    .Text($"{_company.Name} • Фактура {_invoice.InvoiceNumber}");
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
                            column.Item()
                                .Height(60)
                                .Image(File.ReadAllBytes(_company.LogoPath));
                        }
                    }
                    catch { }

                    column.Item()
                        .Text(_company.Name ?? "")
                        .Bold()
                        .FontSize(22);

                    column.Item().Text(_company.Address ?? "");
                    column.Item().Text($"{_company.PostalCode ?? ""} {_company.City ?? ""}");
                    column.Item().Text(_company.IBAN ?? "");
                    column.Item().Text(_company.BankName ?? "");
                });

                row.ConstantItem(220)
                    .Background(Colors.Blue.Lighten5)
                    .Padding(15)
                    .Column(c =>
                    {
                        c.Item().Text("ФАКТУРА")
                            .Bold()
                            .FontSize(20);

                        c.Item().Text($"Број: {_invoice.InvoiceNumber}");
                        c.Item().Text($"Датум: {_invoice.InvoiceDate:dd.MM.yyyy}");
                        c.Item().Text($"Рок: {_invoice.DueDate:dd.MM.yyyy}");
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
                .Padding(15)
                .Column(c =>
                {
                    c.Item().Text("Фактура за").Bold();

                    c.Item()
                        .Text(_invoice.Customer?.Name ?? "");

                    c.Item()
                        .Text(_invoice.Customer?.Address ?? "");

                    c.Item()
                        .Text($"{_invoice.Customer?.PostalCode ?? ""} {_invoice.Customer?.City ?? ""}");
                });
        }



        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.RelativeColumn();
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });


                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Опис");
                    h.Cell().Element(HeaderCell).Text("Кол.");
                    h.Cell().Element(HeaderCell).Text("Цена");
                    h.Cell().Element(HeaderCell).Text("Вкупно");
                });


                foreach (var item in _invoice.Items ?? new List<InvoiceItem>())
                {
                    var total =
                        item.TotalPrice > 0
                        ? item.TotalPrice
                        : item.Quantity * item.UnitPrice;


                    table.Cell()
                        .Element(Cell)
                        .Text(item.Description ?? "");


                    table.Cell()
                        .Element(Cell)
                        .Text($"{item.Quantity} {item.Unit}");


                    table.Cell()
                        .Element(Cell)
                        .Text($"{item.UnitPrice:0.00}");


                    table.Cell()
                        .Element(Cell)
                        .Text($"{total:0.00} МКД");
                }
            });
        }



        void ComposeTotals(IContainer container)
        {
            container
                .Width(280)
                .Background(Colors.Grey.Lighten5)
                .Padding(15)
                .Column(c =>
                {
                    c.Item()
                     .Text($"Нето: {_invoice.NetAmount:0.00} МКД");

                    c.Item()
                     .Text($"ДДВ: {CalculateInvoiceTax(_invoice):0.00} МКД");

                    c.Item()
                     .LineHorizontal(1);

                    c.Item()
                     .Text($"ВКУПНО: {_invoice.GrossAmount:0.00} МКД")
                     .Bold();
                });
        }



        void ComposePaymentInfo(IContainer container)
        {
            container
                .Background(Colors.Blue.Lighten5)
                .Padding(15)
                .Column(c =>
                {
                    c.Item()
                     .Text(_invoice.Description ?? "");

                    c.Item()
                     .PaddingTop(20)
                     .Text(_company.CEO ?? "");
                });
        }



        static IContainer HeaderCell(IContainer c)
        {
            return c
                .Background(Colors.Blue.Darken2)
                .DefaultTextStyle(x => x.FontColor(Colors.White))
                .Padding(8);
        }


        static IContainer Cell(IContainer c)
        {
            return c
                .Padding(8)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2);
        }



        decimal CalculateInvoiceTax(Invoice invoice)
        {
            if (invoice.TaxRate <= 0)
                return 0;

            return Math.Round(
                invoice.NetAmount *
                invoice.TaxRate / 100m,
                2);
        }
    }
}