using HashtagMeta.CLI.Actions;
using HashtagMeta.Core.DnProto;

namespace HashtagMeta.Test {
    [TestClass]
    public class HashtagCliActionTests {
        const string testFilesFolder = @"./Resources/Files/Test";
        private const string _testOutputPath = "../../../TestOutput";

        [TestMethod]
        public void CreateAndValidateActionValidZipTest() {
            var outputDir = new DirectoryInfo(Path.Combine(_testOutputPath, nameof(CreateAndValidateActionValidZipTest)));
            if (outputDir.Exists) {
                outputDir.Delete(true);
            }
            outputDir.Create();

            var zipOutputFile = Path.Combine(outputDir.FullName, "SignedHashtagFile.zip");
            var keyPair = KeyPair.Generate(KeyTypes.P256);

            var action = new CreateAction();
            var options = new CreateActionOptions {
                ZipOutputFile = zipOutputFile,
                InputFiles = [testFilesFolder],
                Data = """
                {"issuer":"did:web:farmmaps.eu","source":"www.farmmaps.eu"}
                """,
                PrivateKey = keyPair.PrivateKeyMultibase,
                PublicKey = keyPair.PublicKeyMultibase,
            };

            var result = action.Execute(options);
            Assert.AreEqual(0, result);

            //validate
            var validateAction = new VerifyAction();
            var validateOptions = new VerifyActionOptions {
                InputFiles = [zipOutputFile],
                PublicKey = keyPair.PublicKeyMultibase
            };

            result = validateAction.Execute(validateOptions);
            Assert.AreEqual(0, result);
        }
    }
}
