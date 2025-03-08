using System.CommandLine;
using System.Text.Json;
using M2Lib.m2;

Dictionary<string, int> VersionMap = new()
{
    { "classic", 256 },
    { "burningcrusade", 260 },
    { "lateburningcrusade", 263 },
    { "lichking", 264 },
    { "cataclysm", 272 },
    { "pandaria", 272 },
    { "draenor", 272 },
    { "legion", 274 },
};

Console.WriteLine("M2 Converter v1.0.0");
Console.WriteLine("----------------------");

// Setup command line options
var inputOption = new Option<FileInfo>(["-i", "--in", "--input"], "Path to input file")
{
    IsRequired = true,
};

var outputOption = new Option<FileInfo>(["-o", "--out", "--output"], "Path to output file")
{
    IsRequired = true,
};

var targetOption = new Option<string>(
    ["-t", "--target"],
    $"Target version to convert to, possible values:\n - {string.Join(", ", VersionMap.Keys)}"
);

var rootCommand = new RootCommand("M2 Model Converter") { inputOption, outputOption, targetOption };

rootCommand.SetHandler(
    (input, output, target) =>
    {
        Console.WriteLine($"Loading \"{input.FullName}\"");
        try
        {
            var model = new M2();
            using (var reader = new BinaryReader(new FileStream(input.FullName, FileMode.Open)))
                model.Load(reader);

            Console.WriteLine($"Model \"{model.Name}\" opened");

            var oldVersion = (int)model.Version;
            Console.WriteLine(
                $"Detected version \"{VersionMap.First(v => v.Value == oldVersion).Key ?? "Unknown"}\" opened"
            );

            // TODO: Convert model to target version

            var newVersion = (int)model.Version;
            Console.WriteLine(
                $"Converted to version \"{VersionMap.First(v => v.Value == newVersion).Key ?? "Unknown"}\""
            );

            if (output.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(model, options);
                File.WriteAllText(output.FullName, jsonString);
            }
            else
            {
                using var writer = new BinaryWriter(
                    new FileStream(output.FullName, FileMode.Create)
                );
                model.Save(writer);
            }
            Console.WriteLine($"Saved into \"{output.FullName}\"");

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return Task.FromResult(1);
        }
        finally
        {
            Console.WriteLine("Finished...");
        }
    },
    inputOption,
    outputOption,
    targetOption
);

// Execute the command
return await rootCommand.InvokeAsync(args);
