using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AccMotec.Logic;
using NUnit.Framework;

namespace AccMotec.Tests
{
    public class ParserLogicTests
    {
        IParserLogic _parserLogic;
        
        [SetUp]
        public void Setup()
        {
            _parserLogic = new ParserLogic(new MotecParser());
        }

        [Test, TestCaseSource(nameof(FileNotFoundCases))]
        public async Task Test_ParserLogic_ParseFilesAsync_ReturnsNull_WhenFilesNotFound(string ldxPath, string ldPath)
        {
            var file = await _parserLogic.ParseFilesAsync(ldxPath, ldPath);

            Assert.IsNull(file);
        }

        [Test]
        public async Task Test_ParserLogic_ParseFilesAsync_ReturnsNull_WhenNoLapsFound()
        {
            var file = await _parserLogic.ParseFilesAsync("no-laps.ldx", "Kyalami-audi_r8_lms_evo-1-2021.12.07-17.59.12.ld");

            Assert.IsNull(file);
        }

        [Test]
        public async Task Test_ParserLogic_ParseFilesAsync_ReturnsFileObject_WhenValid()
        {
            var file = await _parserLogic.ParseFilesAsync("Kyalami-porsche_991ii_gt3_r-3-2021.12.15-21.43.22.ldx", "Kyalami-porsche_991ii_gt3_r-1-2021.12.15-21.37.58.ld");

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
            new [] { "Invalid.ldx", null },
            new [] { "Invalid.ldx", "Kyalami-audi_r8_lms_evo-1-2021.12.07-17.59.12.ld" },
        };
    }
}