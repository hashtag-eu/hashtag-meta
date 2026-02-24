using HashtagMeta.Core.DnProto;
using HashtagMeta.Core.Helpers;
using HashtagMeta.Core.Models;
using HashtagMeta.Core.Services;
using System.IO.Compression;

namespace HashtagMeta.Test;

[TestClass]
public sealed class HashtagCreateTests {
    const string testFilesFolder = @"./Resources/Files/Test";
    private const string _testOutputPath = "../../../TestOutput";

    [TestMethod]
    public void CreateFromFolderTest() {
        var htc = new HashtagCalculator(new DirectoryInfo(testFilesFolder));

        var htdata = htc.CreateHashtagData();

        Assert.IsNotNull(htdata);

        Assert.HasCount(6, htdata.Files);

        var files = htdata.Files;
        Assert.AreEqual("bafkreid422tw22w3jpfey6r2brvfe6xzjhm3wrixprhsj7mrinaxrcoohq", files["file1.txt"].FileCID);
        Assert.AreEqual("bafkreib45zige2aedcl73valp6b574biv76weojeanzftvpi7g3pffxbre", files["icon.png"].FileCID);
        Assert.AreEqual("bafkreifvugi4salrlfbaepirfvsb7huvlgi4elqd3d34jsqlwurncthuxq", files["readme.md"].FileCID);
        Assert.AreEqual("bafkreigkmlshq7hsgssl3am4m5pt75qntfblrosjt52hrnbt5qowtl6fmy", files["test.bin"].FileCID);
        Assert.AreEqual("bafkreicyh46nqcxf2ughuom23vlmv7lc3azmh3xk4gyedjd5fcztiz74li", files["test.xml"].FileCID);
        Assert.AreEqual("bafkreifwl54omwjd3vthx3dgckirjluwzmtbkq225vrw3lvhyy6iwq4aby", files["testdata.json"].FileCID);
    }

    [TestMethod]
    public void CreateSignedHashtagZipfileTest() {
        var htc = new HashtagCalculator(new DirectoryInfo(testFilesFolder));

        var keyPair = KeyPair.Generate(KeyTypes.P256);
        var privateKey = keyPair.PrivateKeyMultibase;
        var publicKey = keyPair.PublicKeyMultibase;

        var template = new HashtagMetaJson {
            Data = new() {
                Issuer = "did:web:farmmaps.eu",
                Source = "https://www.farmmaps.eu",
                Tags = new() { { "key1", "value1" }, { "key2", "value2" } }
            }
        };

        var outputDir = new DirectoryInfo(Path.Combine(_testOutputPath, nameof(CreateSignedHashtagZipfileTest)));
        outputDir.Create();

        var outputFileName = Path.Combine(outputDir.FullName, "testhashtag.zip");
        if(File.Exists(outputFileName)) {
            File.Delete(outputFileName);
        }
        const string htmetaJsonName = "HASHTAG1.TXT";

        var inputJson = htc.CreateSignedHashtagJson(privateKey, publicKey, template).ToJson();

        var zipFile = htc.CreateSignedHashtagZipfile(
            outputFileName,
            privateKey,
            publicKey,
            htmetaJsonName,
            template
        );

        Assert.IsTrue(zipFile.Exists);

        //check
        var checkFolder = new DirectoryInfo(Path.Combine(outputDir.FullName, "checkzip"));
        if(checkFolder.Exists) {
            checkFolder.Delete(true);
        }
        checkFolder.Create();
        ZipFile.ExtractToDirectory(zipFile.FullName, checkFolder.FullName);

        var htmetaCheckFile = Path.Combine(checkFolder.FullName, htmetaJsonName);
        Assert.IsTrue(File.Exists(htmetaCheckFile));

        var htMetaCheck = HashtagMetaJson.FromJsonFile(htmetaCheckFile);
        var valid = htMetaCheck.ValidateAll(publicKey, out var errors, checkFolder.FullName);
        Assert.IsTrue(valid, string.Join('\n', errors));
    }

    [TestMethod]
    public void CreateFromFilesWithInitJsonTest() {
        var htc = new HashtagCalculator([
            Path.Combine(testFilesFolder, "file1.txt"),
            Path.Combine(testFilesFolder, "icon.png")
        ]);

        var template = new HashtagData {
            Issuer = "did:web:farmmaps.eu",
            Source = "https://www.farmmaps.eu",
            Tags = new() { { "key1", "value1" }, { "key2", "value2" } }
        };
        var htdata = htc.CreateHashtagData(new() { Data = template });

        Assert.IsNotNull(htdata);

        Assert.AreEqual("did:web:farmmaps.eu", htdata.Issuer);
        Assert.AreEqual("https://www.farmmaps.eu", htdata.Source);
        Assert.AreEqual("key1,key2", string.Join(',', htdata.Tags.Keys));
        Assert.AreEqual("value1,value2", string.Join(',', htdata.Tags.Values));

        Assert.HasCount(2, htdata.Files);

        var files = htdata.Files;
        Assert.AreEqual("bafkreid422tw22w3jpfey6r2brvfe6xzjhm3wrixprhsj7mrinaxrcoohq", files["file1.txt"].FileCID);
        Assert.AreEqual("bafkreib45zige2aedcl73valp6b574biv76weojeanzftvpi7g3pffxbre", files["icon.png"].FileCID);
    }

    [TestMethod]
    public void CreateSignedFromFilesWithInitJsonTest() {
        var htc = new HashtagCalculator([
            Path.Combine(testFilesFolder, "file1.txt"),
            Path.Combine(testFilesFolder, "icon.png")
        ]);

        var keyPair = KeyPair.Generate(KeyTypes.P256);
        var template = new HashtagData {
            Issuer = "did:web:farmmaps.eu",
            Source = "https://www.farmmaps.eu",
            Tags = new() { { "key1", "value1" }, { "key2", "value2" } }
        };

        var htjson = htc.CreateSignedHashtagJson(
            keyPair.PrivateKeyMultibase,
            keyPair.PublicKeyMultibase,
            new() { Data = template }
        );

        var htdata = htjson.Data;
        Assert.IsNotNull(htdata);

        Assert.AreEqual("did:web:farmmaps.eu", htdata.Issuer);
        Assert.AreEqual("https://www.farmmaps.eu", htdata.Source);
        Assert.AreEqual("key1,key2", string.Join(',', htdata.Tags.Keys));
        Assert.AreEqual("value1,value2", string.Join(',', htdata.Tags.Values));

        Assert.HasCount(2, htdata.Files);

        var files = htdata.Files;
        Assert.AreEqual("bafkreid422tw22w3jpfey6r2brvfe6xzjhm3wrixprhsj7mrinaxrcoohq", files["file1.txt"].FileCID);
        Assert.AreEqual("bafkreib45zige2aedcl73valp6b574biv76weojeanzftvpi7g3pffxbre", files["icon.png"].FileCID);

        //check signature validity
        var valid = htjson.ValidateSignature(keyPair.PublicKeyMultibase);
        Assert.IsTrue(valid);

        //recreate tags with same values
        htjson.Data.Tags = new() { { "key2", "value2" }, { "key1", "value1" } };
        valid = htjson.ValidateSignature(keyPair.PublicKeyMultibase);
        Assert.IsTrue(valid);

        //'tamper' with the json
        htjson.Data.Issuer = "did:web:far-maps.eu";
        valid = htjson.ValidateSignature(keyPair.PublicKeyMultibase);
        Assert.IsFalse(valid);
    }

    [TestMethod]
    [DataRow("file1.txt", "bafkreid422tw22w3jpfey6r2brvfe6xzjhm3wrixprhsj7mrinaxrcoohq")]
    [DataRow("icon.png", "bafkreib45zige2aedcl73valp6b574biv76weojeanzftvpi7g3pffxbre")]
    [DataRow("readme.md", "bafkreifvugi4salrlfbaepirfvsb7huvlgi4elqd3d34jsqlwurncthuxq")]
    [DataRow("test.bin", "bafkreigkmlshq7hsgssl3am4m5pt75qntfblrosjt52hrnbt5qowtl6fmy")]
    [DataRow("test.xml", "bafkreicyh46nqcxf2ughuom23vlmv7lc3azmh3xk4gyedjd5fcztiz74li")]
    [DataRow("testdata.json", "bafkreifwl54omwjd3vthx3dgckirjluwzmtbkq225vrw3lvhyy6iwq4aby")]
    public void CreateFromSingleFileTest(string file, string cid) {
        var testFileName = Path.Combine(testFilesFolder, file);

        var htc = new HashtagCalculator(testFileName);

        var htdata = htc.CreateHashtagData();

        Assert.IsNotNull(htdata);

        Assert.HasCount(1, htdata.Files);

        var files = htdata.Files;
        Assert.AreEqual(cid, files[file].FileCID);
    }

    [TestMethod]
    public void CalculateMetadataJsonHashTest() {
        const string validHash = "ygaVx53zYxmfxsZRbOYJNXbO8DJ2TNS+p2PPQXlVMt8=";

        string[] jsonLiterals = [
           """{"data":{"issuer":"diddfdf","source":"sffdsffs fa","tags":{"key1":"value1","key2":"value 2"}}}""",
           """
            {"data": { "source": "sffdsffs fa", "issuer": "diddfdf",
            "tags": { "key1": "value1", "key2": "value 2"} } }
            """,
           """
            {
            "data": {
              "tags": {
               "key2": "value 2",
               "key1": "value1"
               },
              "issuer" : "diddfdf",
              "source" : "sffdsffs fa"
            } }
            """
        ];

        foreach(var json in jsonLiterals) {
            var htjson = HashtagMetaJson.FromJsonString(json);

            var hash = Convert.ToBase64String(htjson.Data.CalculateDagCborHash());

            Assert.AreEqual(validHash, hash);
        }
    }
}
