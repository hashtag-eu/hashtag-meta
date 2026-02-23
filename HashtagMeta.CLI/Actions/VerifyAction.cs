using HashtagMeta.Core.Helpers;
using HashtagMeta.Core.Models;
using System.IO.Compression;

namespace HashtagMeta.CLI.Actions;

public class VerifyAction : ActionBase<VerifyActionOptions> {
    public override int Execute(VerifyActionOptions options) {
        try {
            string tempFolderName = Path.Combine(Path.GetTempPath(), $"hashtag-{Guid.NewGuid().ToString("D")[..8]}");

            DirectoryInfo? dataFolder = null;

            var inputFileType = DetermineInputFileType(options.InputFileType, [.. options.InputFiles]);
            switch (inputFileType) {
                case InputFileType.Zip:
                    //decompress to temp folder and calculate file CIDs
                    string? zipFile = null;
                    try {
                        zipFile = options.InputFiles.FirstOrDefault();
                        dataFolder = new DirectoryInfo(tempFolderName);
                        dataFolder.Create();

                        using var zip = ZipFile.OpenRead(zipFile ?? "");
                        zip.ExtractToDirectory(dataFolder.FullName, overwriteFiles: true);

                    } catch (Exception ex) {
                        Console.WriteLine($"Error opening zip file '{zipFile}':");
                        Console.WriteLine(ex.Message);
                        return 3;
                    }
                    break;
                case InputFileType.Files:
                case InputFileType.Folder:
                    //Calculate file CIDs for files in the folder
                    var folderName = options.InputFiles.FirstOrDefault() ?? Directory.GetCurrentDirectory();
                    dataFolder = new DirectoryInfo(folderName);
                    break;
                default:
                    break;
            }

            if (dataFolder != null && dataFolder.Exists) {
                //verify metadata json
                var inputFile = new FileInfo(Path.Combine(dataFolder.FullName, options.MetadataFile));
                var htJson = HashtagMetaJson.FromJsonFile(inputFile);
                if (htJson == null) {
                    Console.WriteLine($"'{inputFile.FullName}' is not a valid hashtag metadata file.");
                    return 2;
                }

                //verify folder
                var valid = htJson.ValidateAll(
                    options.PublicKey,
                    out var errors,
                    dataFolder.FullName
                );
                if (!valid) {
                    Console.WriteLine($"'{inputFile.FullName}' is not valid:");
                    Console.WriteLine(string.Join('\n', errors.Select(e => $"  - {e}")));
                    return 4;
                }
            }

            if (inputFileType == InputFileType.Zip && Directory.Exists(tempFolderName)) {
                try {
                    Directory.Delete(tempFolderName, true);
                } catch (Exception ex) {
                    Console.WriteLine($"Removing temp folder '{tempFolderName}' failed:");
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine($"Validation succeeded!");
            return 0;
        } catch (Exception ex) {
            Console.WriteLine($"Error verifying '{options.MetadataFile}':");
            Console.WriteLine(ex.Message);
            return 1;
        }
    }
}
