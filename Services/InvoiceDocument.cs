namespace BauFlow.Services
{
    using BauFlow.Entities;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;

    public class InvoiceDocument : IDocument
    {
        private readonly Invoice _invoice;
        private string _companyName;
        public InvoiceDocument(Invoice invoice, string companyName)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _invoice = invoice;
            _companyName = companyName;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header().Element(ComposeHeader);

                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span(_companyName);
                    x.Span($"Rechnung {_invoice.InvoiceNumber}");
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

                    column.Item().Text(_companyName)
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Text("Digitale Bauverwaltung")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(220)
                    .Background(Colors.Grey.Lighten3)
                    .Padding(10)
                    .Column(column =>
                    {
                        column.Item().Text("RECHNUNG")
                            .Bold()
                            .FontSize(16);

                        column.Item().Text($"Nr: {_invoice.InvoiceNumber}");
                        column.Item().Text($"Datum: {_invoice.InvoiceDate:dd.MM.yyyy}");
                        column.Item().Text($"Fällig: {_invoice.DueDate:dd.MM.yyyy}");
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

                column.Item().AlignRight().Element(ComposeTotals);
            });
        }

        void ComposeCustomer(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Column(column =>
                {
                    column.Spacing(3);

                    column.Item().Text("Rechnung an")
                        .Bold()
                        .FontSize(12);

                    column.Item().Text(_invoice.Customer?.Name);
                    column.Item().Text(_invoice.Customer?.Address);
                    column.Item().Text($"{_invoice.Customer?.PostalCode} {_invoice.Customer?.City}");
                });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Beschreibung").Bold();
                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Menge").Bold();
                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).AlignRight().Text("Preis").Bold();
                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                });

                foreach (var item in _invoice.Items)
                {
                    table.Cell().Element(CellStyle).Text(item.Description);

                    table.Cell().Element(CellStyle).Text($"{item.Quantity} {item.Unit}");

                    table.Cell().Element(CellStyle).AlignRight()
                        .Text($"{item.UnitPrice:0.00} €");

                    table.Cell().Element(CellStyle).AlignRight()
                        .Text($"{item.TotalPrice:0.00} €");
                }
            });
        }

        void ComposeTotals(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Column(column =>
                {
                    column.Spacing(5);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Netto");
                        row.ConstantItem(100).AlignRight()
                            .Text($"{_invoice.NetAmount:0.00} €");
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("MwSt");
                        row.ConstantItem(100).AlignRight()
                            .Text($"{_invoice.TaxAmount:0.00} €");
                    });

                    column.Item().LineHorizontal(1);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Gesamt")
                            .Bold()
                            .FontSize(14);

                        row.ConstantItem(100).AlignRight()
                            .Text($"{_invoice.GrossAmount:0.00} €")
                            .Bold()
                            .FontSize(14)
                            .FontColor(Colors.Blue.Darken2);
                    });
                });
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .PaddingVertical(8)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2);
        }
    }
}
