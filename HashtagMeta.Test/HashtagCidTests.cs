using HashtagMeta.Core.DnProto;
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

        [TestMethod]
        public void ValidateFileCidCalculationTest() {

            var fi = new FileInfo(@"./Resources/Files/hashtag_eo_raster.tif");

            var cidV1 = CidV1.GenerateForBlobBytes(File.ReadAllBytes(fi.FullName));
            var cidV1String = cidV1.Base32;

            var fileCid = HashtagFunctions.CreateCID(fi);

            Assert.AreEqual(cidV1String, fileCid);

            Assert.AreEqual("bafkreif5ksalqe362rge3h4aikxbnslzibzihzlwsry26dyvgeolgznbcm", cidV1String);
            Assert.AreEqual("bafkreif5ksalqe362rge3h4aikxbnslzibzihzlwsry26dyvgeolgznbcm", fileCid);

        }
    }
}
