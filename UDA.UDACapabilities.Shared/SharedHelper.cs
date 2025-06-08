using Microsoft.Extensions.Logging;
using UDA.Shared;

namespace UDA.UDACapabilities.Shared;

public class SharedHelper
{
    public static LogLevel UdaLogType_To_LogLevel(LogType logType)
    {
        return logType switch
        {
            LogType.Error => LogLevel.Error,
            LogType.Debug => LogLevel.Debug,
            LogType.Information => LogLevel.Information,
            LogType.Warning => LogLevel.Warning,
            _ => throw new NotImplementedException()
        };
    }

    public static T GetValueFromDictionary<T>(Dictionary<string, string>? dictionary, string key)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary), "Dictionary cannot be null.");

        if (dictionary.TryGetValue(key, out string? value))
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), $"Value for key '{key}' is null.");

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (FormatException)
            {
                throw new FormatException($"Value '{value}' for key '{key}' cannot be converted to type {typeof(T).Name}");
            }
        }
        else
            throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");
    }
}
