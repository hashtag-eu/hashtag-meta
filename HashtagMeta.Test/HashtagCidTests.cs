using HashtagMeta.Core.Helpers;

namespace HashtagMeta.Test {
    [TestClass]
    public class HashtagCidTests {

        [TestMethod]
        public void ValidateCidCalculationTest() {
            var testFile = new FileInfo(Path.GetTempFileName());
            const string testData = "B6fb X6hz8tGM8X fowif;#!$%$^*#U6aj;gaoigjds%@$&@%$&UH@$UJHTtwhtrhhh";

            File.WriteAllText(testFile.FullName, testData);
            var fileCid = HashtagFunctions.CreateCID(testFile);
            var textCid = HashtagFunctions.CreateCID(testData);
            Assert.AreEqual(fileCid, textCid);

            testFile.Delete();
        }
    }
}
