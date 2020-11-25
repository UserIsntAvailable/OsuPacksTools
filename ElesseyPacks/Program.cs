using System.CommandLine;
using System.CommandLine.Invocation;

var rootCommand
    = new RootCommand("Downloader and unpacker of Elessey beatmaps packs") {

        new Option(
            new string[] { "-c", "--add-to-collection" },
            "Add each beatmap to a collection depending on which year-month it was ranked"
            ) { Argument = new Argument<bool>() },

        new Option(
            new string[] { "-o", "--open-osu" },
            "Open osu when all the maps are unpacked"
            ) { Argument = new Argument<bool>() },

        new Option(
            new string[] { "-a", "--api-key" },
            "Your google api key, see Readme for more information"
            ) { Argument = new Argument<string>() { Name = "string" } },

        new Option(
            new string[] { "-s", "--osu-folder" },
            "The path of your osu folder"
            ) { Argument = new Argument<string>() { Name = "string" } },

        new Option(
            new string[] { "-r", "--regex-pattern" },
            "A regex pattern that you want to match with every pack ( month pack ), see Readme for more information"
            ) { Argument = new Argument<string>() { Name = "string" } },

        new Option(
            new string[] { "-m", "--osu-modes" },
            "The osu modes packs that you want to download"
            ) { Argument = new Argument<string>() { Name = "string array" } },

        new Option(
            new string[] { "-d", "--packs-to-download" },
            "The year packs that you want to download"
            ) { Argument = new Argument<string[]>() { Name = "string array" } },
    };

rootCommand.Handler = CommandHandler.Create(async 
    (bool addToCollection, bool openOsu, string apiKey, string osuFolder, string regexPattern, string[] osuModes, string[] packsToDownload) 
        => await new ElesseyPacks.ElesseyPacks(apiKey, osuFolder).Start(addToCollection, openOsu, regexPattern, osuModes, packsToDownload));

return args.Length != 0
    ? rootCommand.Invoke(args)
    : rootCommand.Invoke("-h");