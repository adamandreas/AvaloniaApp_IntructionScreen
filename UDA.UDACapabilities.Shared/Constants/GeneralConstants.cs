using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UDA.UDACapabilities.Shared.Constants;
public static class GeneralConstants
{
    public static readonly string DUMP_PATH = GetDumpPathFromConfig();

    public const int FINGERPRINT_MATCHING_ROUND_VALUE = 4;
    public const int DPI_ROUND_VALUE = 4;
    public const int LIVENESS_ROUND_VALUE = 4;
    public const int ISO_QUALITY_SCORE_ROUND_VALUE = 4;

    private static string GetDumpPathFromConfig()
    {
        string? configDumpsPath = null;
        string dumpsPath;

        string? exeDir = Path.GetDirectoryName(Environment.ProcessPath!);
        string exePath = exeDir is not null
            ? Path.Combine(exeDir, "appsettings.json")
            : string.Empty;

        string extractedPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");   // old behaviour
        string cwdPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        string? jsonFilePath = new[] { exePath, extractedPath, cwdPath }
                               .FirstOrDefault(File.Exists);

        if (jsonFilePath is null)
        {
            string defaultDumpPath = Path.Combine(Path.GetTempPath(), "UDA.Diagnostics", "Dumps");
            Directory.CreateDirectory(defaultDumpPath);
            return defaultDumpPath;
        }

        JObject jsonObject = JObject.Parse(File.ReadAllText(jsonFilePath));
        JToken? udaConfig = jsonObject["UDA_Config"];
        if (udaConfig != null)
        {
            JToken? cfg = udaConfig["DumpsPath"];
            configDumpsPath = cfg?.ToString();
        }
        else
            Console.WriteLine("UDA_Config not found in the JSON file.");

        if (configDumpsPath is null)
        {
            string defaultDumpPath = "UDA.Diagnostics/Dumps";
            Directory.CreateDirectory(defaultDumpPath);
            return defaultDumpPath;
        }

        Regex envReplace = new(@"([%][^%]+[%])");
        dumpsPath = envReplace.Replace(
            configDumpsPath,
            m => Environment.GetFolderPath(
                (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder),
                                                      m.Value[1..^1], ignoreCase: true)));

        dumpsPath = Path.GetFullPath(dumpsPath);
        Directory.CreateDirectory(dumpsPath);
        return dumpsPath;
    }
}
