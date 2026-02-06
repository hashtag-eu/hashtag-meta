using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace HashtagMeta
{
    internal class Program
    {
        static int Main(string[] args)
        {
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

            switch (option) {
                case "init":
                    //need a folder name or array of file names as rest of parameters
                    if (args.Length > 1) {
                        foreach(var fname in args[1..]) {
                            try {
                                var finfo = new FileInfo(fname);
                                if (finfo.Exists) {
                                } else {
                                    Console.WriteLine($"File '{fname}' does not exist, exiting...");
                                    return 1;
                                }
                            } catch {
                                Console.WriteLine($"File '{fname}' is not a valid filename, exiting...");
                                return 1;
                            }
                        }
                        return 0;
                    } else {
                        Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
                        return 1;
                    }
                case "sign":
                    //need a folder name as 2nd parameter
                    if (args.Length > 1) {
                        return 0;
                    } else {
                        Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
                        return 1;
                    }
                case "verify":
                    //need a database name as 2nd parameter
                    if (args.Length > 1) {
                        return 0;
                    } else {
                        Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
                        return 1;
                    }
                case "deleteunittestdb":
                    //need a database name as 2nd parameter
                    if (args.Length > 1) {
                        return true ? 0 : 1;
                    } else {
                        Console.WriteLine("Incorrect number of arguments given to create the database,\n" +
                                          "please specify the database name.");
                        return 1;
                    }
                case "help":
                default:
                    Console.WriteLine("Usage:");
                    Console.WriteLine(" - htmeta init <folder | [file]>");
                    Console.WriteLine(" - htmeta verify <folder");
                    Console.WriteLine(" - htmeta sign <file>");
                    return 0;
            }
        }
    }
}
