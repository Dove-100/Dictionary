namespace TriDict.Domain.Tests;

public sealed class TriDictTextNormalizerTests
{
    [Theory]
    [InlineData("  Modelo   de  DATOS ", "modelo de datos")]
    [InlineData("ＣＬＯＵＤ", "cloud")]
    [InlineData("应用\t程序\n接口", "应用 程序 接口")]
    public void NormalizeForSearch_ShouldProduceStableSearchText(string input, string expected)
    {
        TriDictTextNormalizer.NormalizeForSearch(input).ShouldBe(expected);
    }
}

internal static class TestAssertions
{
    public static void ShouldBe<T>(this T actual, T expected) => Assert.Equal(expected, actual);
}
