using TriDict.Importing;

namespace TriDict.Application.Tests;

public sealed class CsvTableParserTests
{
    [Fact]
    public void Parse_ShouldSupportQuotedCommaEscapedQuoteAndNewline()
    {
        const string csv = "Code,Definition\r\nA,\"金融, 银行\"\r\nB,\"第一行\n第二行 \"\"引文\"\"\"";

        var records = CsvTableParser.Parse(csv);

        Assert.Equal(3, records.Count);
        Assert.Equal("金融, 银行", records[1].Values[1]);
        Assert.Equal("第一行\n第二行 \"引文\"", records[2].Values[1]);
        Assert.Equal(3, records[2].RowNumber);
    }

    [Fact]
    public void Parse_ShouldRejectUnclosedQuotedField()
    {
        Assert.Throws<FormatException>(() => CsvTableParser.Parse("Code,Definition\nA,\"未闭合"));
    }
}
