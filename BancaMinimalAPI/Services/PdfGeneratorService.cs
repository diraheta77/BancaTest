using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using BancaMinimalAPI.Features.CreditCards.DTOs;

namespace BancaMinimalAPI.Services
{
    public class PdfGeneratorService
    {
        public byte[] GenerateStatementPdf(CreditCardStatementDTO statement)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Encabezado
            document.Add(new Paragraph("Estado de Cuenta")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20));

            // Información de la tarjeta
            document.Add(new Paragraph($"Tarjeta: {statement.CardNumber}")
                .SetFontSize(12));
            document.Add(new Paragraph($"Titular: {statement.HolderName}")
                .SetFontSize(12));
            document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                .SetFontSize(12));

            // Resumen
            document.Add(new Paragraph("\nResumen de la Cuenta")
                .SetFontSize(14)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));
            
            document.Add(new Paragraph($"Límite de Crédito: ${statement.CreditLimit:N2}"));
            document.Add(new Paragraph($"Saldo Actual: ${statement.TotalBalance:N2}"));
            document.Add(new Paragraph($"Saldo Disponible: ${statement.AvailableBalance:N2}"));
            document.Add(new Paragraph($"Interés Bonificable: ${statement.BonusInterest:N2}"));
            document.Add(new Paragraph($"Pago Mínimo: ${statement.MinimumPayment:N2}"));
            document.Add(new Paragraph($"Pago Total: ${statement.TotalAmount:N2}"));

            // Transacciones
            document.Add(new Paragraph("\nTransacciones del Mes")
                .SetFontSize(14)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

            var table = new Table(4).UseAllAvailableWidth();
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Monto").SetFont(boldFont)));

            foreach (var transaction in statement.Transactions)
            {
                table.AddCell(transaction.Date.ToString("dd/MM/yyyy"));
                table.AddCell(transaction.Description);
                table.AddCell(transaction.Type.ToString());
                table.AddCell($"${transaction.Amount:N2}");
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
    }
}