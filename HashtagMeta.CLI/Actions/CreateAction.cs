using HashtagMeta.CLI.Services;

namespace HashtagMeta.CLI.Actions;

public class CreateAction : ActionBase<CreateActionOptions> {
    public override int Execute(CreateActionOptions options) {

        //check whether output file exists
        if(!options.Force && File.Exists(options.MetadataFile)) {
            Console.WriteLine($"Can't create new Hashtag metadata file '{options.MetadataFile}', it already exists.");
            return 1;
        }

        IEnumerable<string> hashtagFiles = [];
        switch (options.InputFileType) {
            case InputFileType.Files:
                hashtagFiles = options.InputFiles;
                break;
            case InputFileType.CompressedFile:
                break;
            case InputFileType.Folder:
                //decompress to temp folder and calculate file CIDs
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

            var jsonString = fi.CreateHashtagMetaJson();
            Console.WriteLine("New metadata file:");
            Console.WriteLine();
            Console.WriteLine(jsonString);
            File.WriteAllText(options.MetadataFile, jsonString);
            return 0;
        }
        Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
        return 2;
    }
}
