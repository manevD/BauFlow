using BauFlow.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BauFlow.Services
{
    public class QuotesDocument : IDocument
    {
        private readonly Quote _quote;
        private readonly Company _company;

        public QuotesDocument(Quote quote, Company company)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            _quote = quote;
            _company = company;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span(_company.Name);
                        text.Span($" • Понуда {_quote.QuoteNumber}");
                        text.Span(" • Ова не е фактура");
                    });
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Spacing(4);

                    if (!string.IsNullOrWhiteSpace(_company.LogoPath) && File.Exists(_company.LogoPath))
                    {
                        column.Item()
                            .Height(60)
                            .Image(_company.LogoPath);
                    }

                    column.Item()
                        .Text(_company.Name)
                        .Bold()
                        .FontSize(22)
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Text(_company.Address);
                    column.Item().Text($"{_company.PostalCode} {_company.City}");
                    column.Item().Text(_company.Country);

                    if (!string.IsNullOrWhiteSpace(_company.TaxNumber))
                        column.Item().Text($"Даночен број: {_company.TaxNumber}");
                });

                row.ConstantItem(220)
                    .Background(Colors.Grey.Lighten3)
                    .Padding(15)
                    .Column(column =>
                    {
                        column.Spacing(5);

                        column.Item()
                            .Text("ПОНУДА")
                            .Bold()
                            .FontSize(18);

                        column.Item().Text($"Број: {_quote.QuoteNumber}");
                        column.Item().Text($"Датум: {_quote.QuoteDate:dd.MM.yyyy}");

                        if (_quote.ValidUntil.HasValue)
                            column.Item().Text($"Важи до: {_quote.ValidUntil:dd.MM.yyyy}");

                        column.Item().Text($"Статус: {_quote.Status}");
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
                column.Item().Element(ComposePaymentInfo);
            });
        }

        void ComposeCustomer(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .Padding(12)
                .Column(column =>
                {
                    column.Spacing(4);

                    column.Item()
                        .Text("Понуда за")
                        .Bold()
                        .FontSize(12);

                    column.Item().Text(_quote.Customer?.Name);
                    column.Item().Text(_quote.Customer?.Address);
                    column.Item().Text($"{_quote.Customer?.PostalCode} {_quote.Customer?.City}");
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
                    header.Cell().Element(HeaderCellStyle).Text("Опис").Bold();
                    header.Cell().Element(HeaderCellStyle).Text("Кол.").Bold();
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Цена").Bold();
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Вкупно").Bold();
                });

                foreach (var item in _quote.Items)
                {
                    table.Cell().Element(CellStyle).Text(item.Description);

                    table.Cell().Element(CellStyle)
                        .Text($"{item.Quantity} {item.Unit}");

                    table.Cell().Element(CellStyle)
                        .AlignRight()
                        .Text($"{item.UnitPrice:0.00} МКД");

                    table.Cell().Element(CellStyle)
                        .AlignRight()
                        .Text($"{item.TotalPrice:0.00} МКД");
                }
            });
        }

        void ComposeTotals(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .Padding(12)
                .Width(260)
                .Column(column =>
                {
                    column.Spacing(6);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Нето");

                        row.ConstantItem(100)
                            .AlignRight()
                            .Text($"{_quote.NetAmount:0.00} МКД");
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("ДДВ" + " " + _quote.TaxRate);

                        row.ConstantItem(100)
                            .AlignRight()
                            .Text($"{_quote.TaxAmount:0.00} МКД");
                    });

                    column.Item().LineHorizontal(1);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Вкупно")
                            .Bold()
                            .FontSize(14);

                        row.ConstantItem(100)
                            .AlignRight()
                            .Text($"{_quote.GrossAmount:0.00} МКД")
                            .Bold()
                            .FontSize(14)
                            .FontColor(Colors.Blue.Darken2);
                    });
                });
        }

        void ComposePaymentInfo(IContainer container)
        {
            container
                .PaddingTop(10)
                .Background(Colors.Blue.Lighten5)
                .Padding(12)
                .Column(column =>
                {
                    column.Spacing(5);

                    //column.Item()
                    //    .Text("Податоци за плаќање")
                    //    .Bold()
                    //    .FontSize(12);

                    //column.Item().Text($"Примач: {_company.Name}");
                    //column.Item().Text($"Сметка: {_company.IBAN}");

                    //if (!string.IsNullOrWhiteSpace(_company.TaxNumber))
                    //    column.Item().Text($"Даночен број: {_company.TaxNumber}");

                    if (_quote.TaxRate == 0)
                    {
                        column.Item()
                            .PaddingTop(5)
                            .Text("Компанијата не е ДДВ обврзник.")
                            .Italic()
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });
        }

        static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Background(Colors.Blue.Lighten3)
                .Padding(6)
                .BorderBottom(1)
                .BorderColor(Colors.White);
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