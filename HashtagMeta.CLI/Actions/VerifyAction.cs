using HashtagMeta.CLI.DnProto;
using HashtagMeta.CLI.Helpers;
using HashtagMeta.CLI.Models;

namespace HashtagMeta.CLI.Actions;

public class VerifyAction : ActionBase<VerifyActionOptions> {
    public override int Execute(VerifyActionOptions options) {
        var inputFile = new FileInfo(options.InputFile);

        if (inputFile.Exists) {
            var htData = HashtagMetaJson.FromJson(inputFile);

            if (htData != null) {
                //get the calculated hash code from the data element
                var hash = htData.Data.CalculateDagCborHash();

                var verified = Signer.ValidateHash(options.PublicKey, hash, htData.Signature);

                if (!verified) {
                    Console.WriteLine("The Hashtag data hash cannot be verified with the provided public key");
                    return 2;
                }

                var errors = new List<string>();
                //verify source and file CIDs
                //verify source CID
                if (htData.Data.Source != null) {
                    Console.Write("Checking source CID: ");
                    var sourceCid = HashtagFunctions.CreateCID(htData.Data.Source);

                    if (sourceCid == htData.Data.SourceCID) {
                        Console.WriteLine("Passed");
                    } else {
                        Console.WriteLine("Failed");
                        errors.Add($"Source: calculated CID does not match source CID in metadata.");
                    }
                }

                foreach (var f in htData.Data.Files) {
                    try {
                        var fi = new FileInfo(Path.Combine(inputFile.DirectoryName ?? "", f.Key));
                        Console.Write($"Checking file CID for {f.Key}: ");
                        if (fi.Exists) {
                            var cid = HashtagFunctions.CreateCID(fi);

                            if (cid == f.Value.FileCID) {
                                Console.WriteLine("Passed");
                            } else {
                                errors.Add($"File {fi.FullName}: calculated CID does not match file CID in metadata.");
                                Console.WriteLine("Failed");
                            }
                        } else {
                            errors.Add($"File {fi.FullName} does not exist.");
                            Console.WriteLine("Failed");
                        }
                    } catch (Exception ex) {
                        errors.Add($"File {f.Key}: exception verifying file CID:");
                        errors.Add(ex.Message);
                        Console.WriteLine("Failed");
                    }
                }

                if (errors.Count > 0) {
                    Console.WriteLine("Checking the CIDs of the metadata content failed:");
                    Console.WriteLine(string.Join(Environment.NewLine, errors));
                    return 3;
                }
                Console.WriteLine();
                Console.WriteLine($"File {options.InputFile} is valid!");

                return 0;
            }
        }
        Console.WriteLine($"Input file '{options.InputFile}' is not a valid Hashtag metadata input file.");
        return 1;
    }
}
