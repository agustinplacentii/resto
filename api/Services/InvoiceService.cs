using System.Globalization;
using System.Text;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public class InvoiceService : IInvoiceService
{
    public byte[] BuildInvoicePdf(Order order)
    {
        var lines = new List<string>
        {
            "Saoko",
            $"Factura pedido #{order.Id}",
            $"Fecha: {order.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm}",
            $"Mesa/cliente: {OrderDestination(order)}",
            ""
        };

        foreach (var item in order.Items)
        {
            var subtotal = item.UnitPrice * item.Quantity;
            lines.Add($"{item.Quantity} x {item.ProductName} ({item.Measure})  {Money(item.UnitPrice)}  {Money(subtotal)}");
        }

        lines.Add("");
        lines.Add($"TOTAL: {Money(order.Total)}");
        if (!string.IsNullOrWhiteSpace(order.Notes))
        {
            lines.Add("");
            lines.Add($"Notas: {order.Notes}");
        }

        return SimplePdf.Create(lines);
    }

    private static string Money(decimal value)
    {
        return value.ToString("C0", CultureInfo.CreateSpecificCulture("es-AR"));
    }

    private static string OrderDestination(Order order)
    {
        if (!string.IsNullOrWhiteSpace(order.CustomerName))
        {
            return $"Nombre: {order.CustomerName}";
        }

        return string.IsNullOrWhiteSpace(order.TableName) ? "Sin mesa" : $"Mesa: {TableLabel(order.TableName)}";
    }

    private static string TableLabel(string tableName)
    {
        return tableName.Trim().StartsWith("Mesa ", StringComparison.OrdinalIgnoreCase)
            ? tableName.Trim()["Mesa ".Length..].Trim()
            : tableName.Trim();
    }

    private static class SimplePdf
    {
        public static byte[] Create(IReadOnlyList<string> lines)
        {
            var content = BuildContent(lines);
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream"
            };

            var builder = new StringBuilder();
            builder.Append("%PDF-1.4\n");
            var offsets = new List<int> { 0 };

            for (var index = 0; index < objects.Count; index++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
                builder.Append(index + 1).Append(" 0 obj\n");
                builder.Append(objects[index]).Append("\n");
                builder.Append("endobj\n");
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
            builder.Append("xref\n");
            builder.Append("0 ").Append(objects.Count + 1).Append("\n");
            builder.Append("0000000000 65535 f \n");

            foreach (var offset in offsets.Skip(1))
            {
                builder.Append(offset.ToString("0000000000", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            }

            builder.Append("trailer\n");
            builder.Append("<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\n");
            builder.Append("startxref\n");
            builder.Append(xrefOffset).Append("\n%%EOF");

            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static string BuildContent(IReadOnlyList<string> lines)
        {
            var builder = new StringBuilder();
            builder.Append("BT\n/F1 12 Tf\n50 790 Td\n");

            foreach (var line in lines)
            {
                builder.Append("(").Append(Escape(line)).Append(") Tj\n0 -20 Td\n");
            }

            builder.Append("ET");
            return builder.ToString();
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal)
                .Normalize(NormalizationForm.FormD)
                .Where(character => character < 128)
                .Aggregate(new StringBuilder(), (builder, character) => builder.Append(character))
                .ToString();
        }
    }
}
