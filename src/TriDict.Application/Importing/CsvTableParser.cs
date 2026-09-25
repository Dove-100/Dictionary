using System.Text;

namespace TriDict.Importing;

public sealed record CsvRecord(int RowNumber, IReadOnlyList<string> Values);

public static class CsvTableParser
{
    public static IReadOnlyList<CsvRecord> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var records = new List<CsvRecord>();
        var values = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var rowNumber = 1;
        var recordStart = 1;

        for (var index = 0; index < content.Length; index++)
        {
            var current = content[index];
            if (inQuotes)
            {
                if (current == '"')
                {
                    if (index + 1 < content.Length && content[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    if (current == '\n') rowNumber++;
                    field.Append(current);
                }
                continue;
            }

            switch (current)
            {
                case '"' when field.Length == 0:
                    inQuotes = true;
                    break;
                case ',':
                    values.Add(field.ToString().Trim());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    values.Add(field.ToString().Trim());
                    field.Clear();
                    records.Add(new CsvRecord(recordStart, values.ToArray()));
                    values.Clear();
                    rowNumber++;
                    recordStart = rowNumber;
                    break;
                default:
                    field.Append(current);
                    break;
            }
        }

        if (inQuotes) throw new FormatException($"Unclosed quoted field starting at row {recordStart}.");
        if (field.Length > 0 || values.Count > 0)
        {
            values.Add(field.ToString().Trim());
            records.Add(new CsvRecord(recordStart, values.ToArray()));
        }

        return records.Where(x => x.Values.Any(value => !string.IsNullOrWhiteSpace(value))).ToList();
    }
}
