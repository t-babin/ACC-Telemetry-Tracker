using NUnit.Framework;
using AccTelemetryTracker.Logic;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using Moq;
using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Tests;

public class Tests
{
    IMotecParser _motecParser;

    [SetUp]
    public void Setup()
    {
        var motecLogger = Mock.Of<ILogger<MotecParser>>();
        _motecParser = new MotecParser(motecLogger);
    }

    [Test, TestCaseSource(nameof(ErrorFileCases))]
    public void Test_MotecParser_ParseLaps_ThrowsException_When_FileNotFound(string path)
    {
        Assert.Throws<FileNotFoundException>(() => _motecParser.ParseLaps(path));
    }

    [Test]
    public void Test_MotecParser_ParseLaps_ReturnsNonEmptyList_When_Laps()
    {
        var laps = _motecParser.ParseLaps("Valid.ldx");

        CollectionAssert.IsNotEmpty(laps);
    }

    [Test]
    public void Test_MotecParser_ParseLaps_ReturnsEmptyList_When_NoLaps()
    {
        var laps = _motecParser.ParseLaps("no-laps.ldx");

        CollectionAssert.IsEmpty(laps);
    }

    [Test, TestCaseSource(nameof(ErrorFileCases))]
    public void Test_MotecParser_IsValidLdx_ThrowsException_When_FileNotFound(string path)
    {
        Assert.Throws<FileNotFoundException>(() => _motecParser.IsValidLdx(path));
    }

    [Test]
    public void Test_MotecParser_IsValidLdx_ReturnsFalse_When_NotValidLdxSchema()
    {
        var isValid = _motecParser.IsValidLdx("Invalid.ldx");

        Assert.False(isValid);
    }

    [Test, TestCaseSource(nameof(ValidFiles))]
    public void Test_MotecParser_IsValidLdx_ReturnsTrue_When_ValidLdxSchema(string path)
    {
        var isValid = _motecParser.IsValidLdx(path);

        Assert.True(isValid);
    }

    [Test]
    public void Test_MotecParser_ParseMotecFileAsync_ThrowsException_When_InvalidFileExtension()
    {
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _motecParser.ParseMotecFileAsync("invalid.ldx"));
    }

    [Test, TestCaseSource(nameof(ValidLdFiles))]
    public async Task Test_MotecParser_ParseMotecFileAsync_ReturnsMotecFile_When_ValidFileExtension(string path, string car, string track, string date)
    {
        var file = await _motecParser.ParseMotecFileAsync(path);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(file);
            Assert.AreEqual(file.Car, car);
            Assert.AreEqual(file.Track, track);
            Assert.AreEqual(file.Date.ToString("dd/MM/yyyyHH:mm:ss", CultureInfo.InvariantCulture), date);
        });
    }

    static object[] ErrorFileCases =
    {
        null,
        string.Empty,
        "test file",
        "Valid copy.ld"
    };

    static object[] ValidFiles =
    {
        "Valid.ldx",
        "Kyalami-porsche_991ii_gt3_r-3-2021.12.15-21.43.22.ldx"
    };

    static object[] ValidLdFiles =
    {
        new [] { "Imola-porsche_991ii_gt3_r-2-2021.12.02-21.13.58.ld", "991ii GT3 R EVO", "Imola", "02/12/202121:13:58" },
        new [] { "Kyalami-audi_r8_lms_evo-1-2021.12.07-17.59.12.ld", "R8 LMS EVO", "Kyalami", "07/12/202117:59:12" },
        new [] { "Kyalami-porsche_991ii_gt3_r-1-2021.12.15-21.37.58.ld", "991ii GT3 R EVO", "Kyalami", "15/12/202121:37:58" }
    };
}