using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

#nullable enable

namespace UDA.InstructionScreen.Licensing;
public record GeneralToken(string CpuSerial)
{
    public string? ApplicationAssemblyName { get; set; }

    public string? MachineName { get; set; }

    public string? Domain { get; set; }

    public bool IsTimeLimited { get; set; }

    public DateTime ExpiryDate { get; set; } = DateTime.MinValue;

    public List<string> EnabledModules { get; set; } = new();

    public int NumberOfNamedUsers { get; set; } = -1;

    public int NumberOfConcurrentUsers { get; set; } = -1;

    public int NumberOfNamedDevices { get; set; } = -1;

    public int NumberOfConcurrentDevices { get; set; } = -1;

    public byte[]? CheckData { get; set; }

    public static GeneralToken Deserialize(byte[] bytes)
    {
        var jsonStr = Encoding.UTF8.GetString(bytes);
        if (JsonSerializer.Deserialize<GeneralToken>(jsonStr) is GeneralToken result)
            return result;
        throw new NullReferenceException();
    }

    public static byte[] Serialize(GeneralToken element)
    {
        var jsonStr = JsonSerializer.Serialize(element);
        return Encoding.UTF8.GetBytes(jsonStr);
    }
}
