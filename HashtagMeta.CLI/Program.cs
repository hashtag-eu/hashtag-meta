using HashtagMeta.CLI.Services;
using Microsoft.Extensions.Configuration;

namespace HashtagMeta.CLI;

public class Program {
    static int Main(string[] args) {
        //args
        //verbs:
        //help
        //verify [folder|zipfile]
        //init [folder|[files]]
        //
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{env}.json", true, true)
            .AddJsonFile($"appsettings.user.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.user.{env}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        var config = configBuilder.Build();

        var option = args.Length > 0 ? args[0].ToLower() : string.Empty;

        var exitCode = 0;
        switch (option) {
            case "init":
                //need a folder name or array of file names as rest of parameters
                if (args.Length > 1) {
                    var fi = new HashtagFileInitializer(args[1..]);
                    var jsonString= fi.GetHashtagMetaJson();
                    File.WriteAllText("hashtag_meta.json", jsonString);
                } else {
                    Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
                    exitCode = 1;
                }
                break;
            case "sign":
                //need a folder name as 2nd parameter
                if (args.Length > 1) {
                } else {
                    Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
                    exitCode = 1;
                }
                break;
            case "verify":
                //need a database name as 2nd parameter
                if (args.Length > 1) {
                } else {
                    Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
                    exitCode = 1;
                }
                break;
            case "help":
            default:
                Console.WriteLine("Usage:");
                Console.WriteLine(" - htmeta init <folder | [file]>");
                Console.WriteLine(" - htmeta verify <folder");
                Console.WriteLine(" - htmeta sign <file>");
                break;
        }
        #if DEBUG
        Console.WriteLine("Press key to exit...");
        Console.ReadKey();
        #endif
        return exitCode;
    }
}
