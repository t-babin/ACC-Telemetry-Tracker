using System.IO;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using AccTelemetryTracker.Logic;
using Moq;
using NUnit.Framework;

namespace AccTelemetryTracker.Tests;
public class ParserLogicTests
{
    IParserLogic _parserLogic;

    [OneTimeSetUp]
    public void Setup()
    {
        var motecLogger = Mock.Of<ILogger<MotecParser>>();
        var logger = Mock.Of<ILogger<ParserLogic>>();
        _parserLogic = new ParserLogic(new MotecParser(motecLogger), logger);
    }

    [Test, TestCaseSource(nameof(FileNotFoundCases))]
    public void Test_ParserLogic_ParseFilesAsync_ThrowsException_When_FilesNotFound(string ldxPath, string ldPath)
    {
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _parserLogic.ParseFilesAsync(new [] { ldxPath, ldPath }));
    }

    [Test, TestCaseSource(nameof(MotecParseExceptions))]
    public void Test_ParserLogic_ParseFilesAsync_ThrowsException_When_InvalidLdx(string ldxPath, string ldPath)
    {
        Assert.ThrowsAsync<MotecParseException>(async () => await _parserLogic.ParseFilesAsync(new [] { ldxPath, ldPath }));
    }

    [Test, TestCaseSource(nameof(MoreFiles))]
    public void Test_ParserLogic_ThrowsException_When_MoreThanExpectedFiles(params string[] files)
    {
        Assert.ThrowsAsync<IOException>(async () => await _parserLogic.ParseFilesAsync(files));
    }

    [Test]
    public async Task Test_ParserLogic_ParseFilesAsync_ReturnsFileObject_When_ValidFiles()
    {
        var file = await _parserLogic.ParseFilesAsync(new [] { "Kyalami-porsche_991ii_gt3_r-3-2021.12.15-21.43.22.ldx", "Kyalami-porsche_991ii_gt3_r-1-2021.12.15-21.37.58.ld" });

        Assert.Multiple(() =>
        {
            Assert.AreEqual("Kyalami", file.Track);
            Assert.AreEqual("991ii GT3 R EVO", file.Car);
            CollectionAssert.IsNotEmpty(file.Laps);
            Assert.AreEqual(3, file.Laps.Count());
        });
    }

    static object[] FileNotFoundCases =
    {
        new string[] { null, null },
        new [] { null, string.Empty },
        new [] { string.Empty, null },
        new [] { "Invalid.ldx", null }
    };

    static object[] MotecParseExceptions =
    {
        new [] { "Invalid.ldx", "Kyalami-audi_r8_lms_evo-1-2021.12.07-17.59.12.ld" },
        new [] { "no-laps.ldx", "Kyalami-audi_r8_lms_evo-1-2021.12.07-17.59.12.ld" }
    };

    static object[] MoreFiles =
    {
        new [] { "one" },
        new [] { "one", "two", "three" },
        new [] { "one", "two", "three", "four" }
    };
}
