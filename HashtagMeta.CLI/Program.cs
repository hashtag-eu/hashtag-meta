using CommandLine;
using CommandLine.Text;
using HashtagMeta.CLI.Actions;
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

        int result;
        bool wait = false;
        try {
            var parserResult = new Parser(c => {
                c.HelpWriter = null;
                c.CaseInsensitiveEnumValues = true;
            }).ParseArguments<CreateActionOptions, CreateKeyPairActionOptions, SignActionOptions, VerifyActionOptions>(args);

            var val = parserResult.Value as ActionOptionsBase;
            wait = val?.Wait ?? false;

            result = parserResult.MapResult(
                (CreateActionOptions options) => runCreate(options),
                (CreateKeyPairActionOptions options) => runCreateKeyPair(options),
                (SignActionOptions options) => runSign(options),
                (VerifyActionOptions options) => runVerify(options),
                errors => showCommandHelp(parserResult, errors)
            );

        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
            result = 1;
        }

        if (wait) {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        return result;
    }

    private static int runCreate(CreateActionOptions options) {
        Console.WriteLine();
        var action = new CreateAction();
        return action.Execute(options);
    }

    private static int runCreateKeyPair(CreateKeyPairActionOptions options) {
        Console.WriteLine();
        var action = new CreateKeyPairAction();
        return action.Execute(options);
    }

    private static int runSign(SignActionOptions options) {
        Console.WriteLine();
        var action = new SignAction();
        return action.Execute(options);
    }

    private static int runVerify(VerifyActionOptions options) {
        Console.WriteLine();
        var action = new VerifyAction();
        return action.Execute(options);
    }

    private static int showCommandHelp(ParserResult<object> parserResult, IEnumerable<Error> errs) {
        var text = HelpText.AutoBuild(parserResult, h => {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = string.Empty;
            return h;
        });
        Console.WriteLine(text);

        var result = -2;
        if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError)) {
            result = -1;
        }
        return result;
    }
}
