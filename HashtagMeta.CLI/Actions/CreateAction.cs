using HashtagMeta.Core.Models;
using HashtagMeta.Core.Services;
using System.IO.Compression;
using System.Text.Json;

namespace HashtagMeta.CLI.Actions;

public class CreateAction : ActionBase<CreateActionOptions> {
    public override int Execute(CreateActionOptions options) {

        string tempFolderName = Path.Combine(Path.GetTempPath(), $"hashtag-{Guid.NewGuid().ToString("D")[..8]}");

        IEnumerable<string> hashtagFiles = [];
        var inputFileType = DetermineInputFileType(options.InputFileType, [.. options.InputFiles]);
        switch (inputFileType) {
            case InputFileType.Files:
                hashtagFiles = options.InputFiles;
                break;
            case InputFileType.Zip:
                //decompress to temp folder and list the files
                string? zipFile = null;
                try {
                    zipFile = options.InputFiles.FirstOrDefault();
                    var tempFolder = new DirectoryInfo(tempFolderName);
                    tempFolder.Create();

                    using var zip = ZipFile.OpenRead(zipFile ?? "");
                    zip.ExtractToDirectory(tempFolder.FullName, overwriteFiles: true);
                    //Get all files of current directory except
                    //hashtag metadata file to write to
                    //get the entries from the zip file, skip the folder entries ending with '/'
                    hashtagFiles = zip.Entries
                        .Where(e => !(e.FullName.EndsWith('/') || e.FullName.EndsWith('\\')))
                        .Select(e => Path.Combine(tempFolderName, e.FullName));
                } catch (Exception ex) {
                    Console.WriteLine($"Error opening zip file '{zipFile}':");
                    Console.WriteLine(ex.Message);
                    return 3;
                }
                break;
            case InputFileType.Folder:
                //List the files in the folder, except the metadata json file if it exists
                var folderName = options.InputFiles.FirstOrDefault() ?? Directory.GetCurrentDirectory();
                var di = new DirectoryInfo(folderName);
                if (di.Exists) {
                    //Get all files of current directory except
                    //hashtag metadata file to write to
                    hashtagFiles = di.GetFiles()
                        .Where(f => !f.Name.Equals(options.MetadataFile, StringComparison.OrdinalIgnoreCase))
                        .Select(f => f.FullName);
                }
                break;
            default:
                break;
        }

        if (hashtagFiles.Any()) {

            var calculator = new HashtagCalculator([.. hashtagFiles]);

            HashtagMetaJson? dataTemplate = null;

            if (!string.IsNullOrEmpty(options.Data)) {
                if (File.Exists(options.Data)) {
                    dataTemplate = JsonSerializer.Deserialize<HashtagMetaJson>(File.ReadAllBytes(options.Data));
                } else {
                    try {
                        dataTemplate = JsonSerializer.Deserialize<HashtagMetaJson>(options.Data ?? "{}");
                    } catch (Exception ex) {
                        Console.WriteLine($"Data template is not a valid file or valid JSON");
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            if (options.ZipOutputFile != null && options.PrivateKey != null && options.PublicKey != null) {
                //delete existing if overwriting
                if (File.Exists(options.ZipOutputFile) && options.Force) {
                    File.Delete(options.ZipOutputFile);
                }

                //Create a zip file with the signed hashtag metadata file and the input files to the file path specified
                var zipfile = calculator.CreateSignedHashtagZipfile(
                    options.ZipOutputFile,
                    options.PrivateKey,
                    options.PublicKey,
                    options.MetadataFile,
                    dataTemplate
                );
                Console.WriteLine($"Zip file with signed metadata created ({zipfile.FullName})");

            } else {
                var htMeta = options.PrivateKey != null && options.PublicKey != null
                    ? calculator.CreateSignedHashtagJson(options.PrivateKey, options.PublicKey, dataTemplate)
                    : new() { Data = calculator.CreateHashtagData() };

                var htMetaJson = htMeta.ToJson();
                Console.WriteLine($"New metadata file created [{options.MetadataFile}]:");
                Console.WriteLine();
                Console.WriteLine(htMetaJson);
                File.WriteAllText(options.MetadataFile, htMetaJson);
            }

            if (inputFileType == InputFileType.Zip && Directory.Exists(tempFolderName)) {
                try {
                    Directory.Delete(tempFolderName, true);
                } catch (Exception ex) {
                    Console.WriteLine($"Removing temp folder '{tempFolderName}' failed:");
                    Console.WriteLine(ex.Message);
                }
            }
            return 0;
        }
        Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
        return 2;
    }
}
