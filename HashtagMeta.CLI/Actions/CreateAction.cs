using HashtagMeta.Core.Models;
using HashtagMeta.Core.Services;
using System.IO.Compression;

namespace HashtagMeta.CLI.Actions;

public class CreateAction : ActionBase<CreateActionOptions> {
    public override int Execute(CreateActionOptions options) {

        //check whether output file exists
        if (!options.Force && File.Exists(options.MetadataFile)) {
            Console.WriteLine($"Can't create new Hashtag metadata file '{options.MetadataFile}', it already exists.");
            return 1;
        }

        string tempFolderName = Path.Combine(Path.GetTempPath(), $"hashtag-{Guid.NewGuid().ToString("D")[..8]}");

        IEnumerable<string> hashtagFiles = [];
        switch (options.InputFileType) {
            case InputFileType.Files:
                hashtagFiles = options.InputFiles;
                break;
            case InputFileType.Zip:
                //decompress to temp folder and calculate file CIDs
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
                //Calculate file CIDs for files in the folder
                var folderName = options.InputFiles.FirstOrDefault() ?? Directory.GetCurrentDirectory();
                var fi = new DirectoryInfo(folderName);
                if (fi.Exists) {
                    //Get all files of current directory except
                    //hashtag metadata file to write to
                    hashtagFiles = fi.GetFiles()
                        .Where(f => !f.Name.Equals(options.MetadataFile, StringComparison.OrdinalIgnoreCase))
                        .Select(f => f.FullName);
                }
                break;
            default:
                break;
        }

        if (hashtagFiles.Any()) {
            var fi = new HashtagCalculator(hashtagFiles);

            var htMeta = new HashtagMetaJson { Data = fi.CreateHashtagData() };
            var htMetaJson = htMeta.ToJson();
            Console.WriteLine($"New metadata file created [{options.MetadataFile}]:");
            Console.WriteLine();
            Console.WriteLine(htMetaJson);
            File.WriteAllText(options.MetadataFile, htMetaJson);

            if (options.InputFileType == InputFileType.Zip && Directory.Exists(tempFolderName)) {
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
