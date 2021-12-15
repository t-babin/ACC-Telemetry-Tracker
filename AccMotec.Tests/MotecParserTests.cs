using NUnit.Framework;
using AccMotec.Logic;
using System.IO;

namespace AccMotec.Tests;

public class Tests
{
    IMotecParser _motecParser;

    [SetUp]
    public void Setup()
    {
        _motecParser = new MotecParser();
    }

    [Test, TestCaseSource(nameof(ErrorFileCases))]
    public void Test_MotecParser_ParseFile_ThrowsException_When_FileNotFound(string path)
    {
        Assert.Throws<FileNotFoundException>(() => _motecParser.ParseFile(path));
    }

    [Test, TestCaseSource(nameof(ErrorFileCases))]
    public void Test_MotecParser_IsValidLdx_ThrowsException_When_FileNotFound(string path)
    {
        Assert.Throws<FileNotFoundException>(() => _motecParser.IsValidLdx(path));
    }

    [Test]
    public void Test_MotecParser_IsValidLdx_ReturnsFalse_When_NotLdxFile()
    {
        var isValid = _motecParser.IsValidLdx(@"C:\Users\tbabi\Documents\Assetto Corsa Competizione\MoTeC\Kyalami-audi_r8_lms_evo-14-2021.12.09-20.33.58.ld");

        Assert.False(isValid);
    }

    [Test]
    public void Test_MotecParser_IsValidLdx_ReturnsFalse_When_NotValidLdxSchema()
    {
        var isValid = _motecParser.IsValidLdx("Invalid.ldx");

        Assert.False(isValid);
    }

    [Test]
    public void Test_MotecParser_IsValidLdx_ReturnsTrue_When_ValidLdxSchema()
    {
        var isValid = _motecParser.IsValidLdx("Valid.ldx");

        Assert.True(isValid);
    }

    static object[] ErrorFileCases =
    {
        null,
        string.Empty,
        "test file"
    };
}