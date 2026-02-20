using HashtagMeta.Core.Services;

namespace HashtagMeta.Test;

[TestClass]
public sealed class HashtagCreateTests {
    [TestMethod]
    public void CreateFromFolderTest() {
        const string testFilesFolder = @"./Resources/Files/Test";
        var htc = new HashtagCalculator(testFilesFolder);

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
    [DataRow("file1.txt", "bafkreid422tw22w3jpfey6r2brvfe6xzjhm3wrixprhsj7mrinaxrcoohq")]
    [DataRow("icon.png", "bafkreib45zige2aedcl73valp6b574biv76weojeanzftvpi7g3pffxbre")]
    [DataRow("readme.md", "bafkreifvugi4salrlfbaepirfvsb7huvlgi4elqd3d34jsqlwurncthuxq")]
    [DataRow("test.bin", "bafkreigkmlshq7hsgssl3am4m5pt75qntfblrosjt52hrnbt5qowtl6fmy")]
    [DataRow("test.xml", "bafkreicyh46nqcxf2ughuom23vlmv7lc3azmh3xk4gyedjd5fcztiz74li")]
    [DataRow("testdata.json", "bafkreifwl54omwjd3vthx3dgckirjluwzmtbkq225vrw3lvhyy6iwq4aby")]
    public void CreateFromSingleFileTest(string file, string cid) {
        const string testFilesFolder = @"./Resources/Files/Test";

        var testFileName = Path.Combine(testFilesFolder, file);

        var htc = new HashtagCalculator([testFileName]);

        var htdata = htc.CreateHashtagData();

        Assert.IsNotNull(htdata);

        Assert.HasCount(1, htdata.Files);

        var files = htdata.Files;
        Assert.AreEqual(cid, files[file].FileCID);
    }


}
