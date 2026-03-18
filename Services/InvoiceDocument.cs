using BauFlow.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace BauFlow.Services
{
    using BauFlow.Entities;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;

    public class InvoiceDocument : IDocument
    {
        private readonly Invoice _invoice;

        public InvoiceDocument(Invoice invoice)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _invoice = invoice;
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
                    x.Span("BauFlow Rechnungssystem • ");
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
                    column.Item().Text("BauFlow")
                        .FontSize(20)
                        .Bold();

                    column.Item().Text("Digitale Bauverwaltung");
                });

                row.ConstantItem(200).AlignRight().Column(column =>
                {
                    column.Item().Text("Rechnung")
                        .FontSize(18)
                        .Bold();

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
            container.Column(column =>
            {
                column.Item().Text("Rechnung an:")
                    .Bold();

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
                    header.Cell().Element(CellStyle).Text("Beschreibung").Bold();
                    header.Cell().Element(CellStyle).Text("Menge").Bold();
                    header.Cell().Element(CellStyle).Text("Preis").Bold();
                    header.Cell().Element(CellStyle).Text("Total").Bold();
                });

                foreach (var item in _invoice.Items)
                {
                    table.Cell().Element(CellStyle).Text(item.Description);

                    table.Cell().Element(CellStyle).Text($"{item.Quantity} {item.Unit}");

                    table.Cell().Element(CellStyle)
                        .AlignRight()
                        .Text($"{item.UnitPrice:0.00} €");

                    table.Cell().Element(CellStyle)
                        .AlignRight()
                        .Text($"{item.TotalPrice:0.00} €");
                }
            });
        }

        void ComposeTotals(IContainer container)
        {
            container.Column(column =>
            {
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

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Gesamt")
                        .Bold();

                    row.ConstantItem(100).AlignRight()
                        .Text($"{_invoice.GrossAmount:0.00} €")
                        .Bold();
                });
            });
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .PaddingVertical(5)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2);
        }
    }
}
