using HashtagMeta.Core.DnProto;
using HashtagMeta.Test.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HashtagMeta.Test {
    [TestClass]
    public class HashtagSignatureTests {
        private static JsonSerializerOptions DeserOptions = new() {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        [TestMethod]
        public void SignatureValidityTest() {
            using FileStream fs = File.OpenRead("./Resources/SignTests/signature-fixtures.json");

            var testData = JsonSerializer.Deserialize<List<SignatureTestModel>>(fs, DeserOptions);

            foreach (var test in testData) {
                System.Diagnostics.Debug.WriteLine($"Testing: {test.Comment}");

                var hash = Convert.FromBase64String(test.MessageBase64);
                var signature = Convert.FromBase64String(test.SignatureBase64);

                var valid = Signer.ValidateHash(test.PublicKeyMultibase, hash, signature);

                //Assert.AreEqual(test.ValidSignature, valid);
            }
        }

        [TestMethod]
        public void CreatePrivatePublicKeypairTest() {
            var keyPair = KeyPair.Generate(KeyTypes.P256);

            Assert.AreEqual(KeyTypes.P256.ToString(), keyPair.KeyType);



        }
    }
}
