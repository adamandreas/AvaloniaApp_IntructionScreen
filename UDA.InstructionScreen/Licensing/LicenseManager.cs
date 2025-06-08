using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

#nullable enable

namespace UDA.InstructionScreen.Licensing;
[SupportedOSPlatform("windows")]
internal class LicenseManager
{
    #region Variables
    private readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private readonly HashAlgorithm _hashAlgorithm = SHA256.Create();
    private readonly string _unifiedLicensingErrorMessage = $"UDA failed to load. Please refer to system administrator.";
    #endregion

    #region Singleton
    private static LicenseManager? instance;

    private LicenseManager()
    {
        ExtractLicense();
    }

    public static LicenseManager Instance
    {
        get
        {
            instance ??= new LicenseManager();
            return instance;
        }
    }
    #endregion

    #region Internal Methods

    internal GeneralToken? LicenseDetails
    {
        get;
        private set;
    }

    internal bool VerifyLicenseModule(string module, bool checkTime = true)
    {
        if (LicenseDetails is null)
        {
            //Theoretically, this should not happen
            throw new Exception($"#1: {_unifiedLicensingErrorMessage}");
        }

        if (!LicenseDetails.EnabledModules.Contains(module))
        {
            // Not enabled module (Ex. Remote is not part of license)
            throw new Exception($"#2: {_unifiedLicensingErrorMessage}");
        }
        if (checkTime && LicenseDetails.IsTimeLimited)
        {
            if (DateTime.Compare(DateTime.Now, LicenseDetails.ExpiryDate) > 0)
            {
                // Time expired
                throw new Exception($"#3: {_unifiedLicensingErrorMessage}");
            }
        }
        return true;
    }

    #endregion

    #region Private Support Methods
    private void ExtractLicense()
    {
        string REG_PATH = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
        using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(REG_PATH, false)
            ?? throw new Exception($"#4: {_unifiedLicensingErrorMessage}");

        string? assemblyName = (Assembly.GetEntryAssembly()?.GetName().Name)
            ?? throw new Exception($"#5: {_unifiedLicensingErrorMessage}");

        List<string>? list = new() { "ac58ae9ef5d5f6ba11788622301222eb63ecea6c18e3bcf167ea22c4befdf483", "f4f4e690bc314bffff348290e4c0e63682783b2d1506a9c32f2fe808e61c16ac" };
        string? subKeyName = registryKey.GetSubKeyNames().FirstOrDefault(subKeyName => EncryptString(subKeyName) == list?[0] || EncryptString(subKeyName) == list?[1])
            ?? throw new Exception($"#6: {_unifiedLicensingErrorMessage}");

        if (!registryKey.GetSubKeyNames().Contains(subKeyName))
            throw new Exception($"#7: {_unifiedLicensingErrorMessage}");

        if (registryKey.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.ReadSubTree) is not RegistryKey subKey)
            throw new Exception($"#8: {_unifiedLicensingErrorMessage}");

        byte[]? licData = (byte[]?)subKey.GetValue("LicData", RegistryValueKind.Binary)
            ?? throw new Exception($"#9: {_unifiedLicensingErrorMessage}");

        subKey.Close();
        LicenseDetails = GeneralToken.Deserialize(licData);

        if (!VerifyToken(LicenseDetails))
            throw new Exception($"#10: {_unifiedLicensingErrorMessage}");

        //System match validation
        //1. Check Machine
        if (LicenseDetails.MachineName != Environment.MachineName)
            throw new Exception($"#11: {_unifiedLicensingErrorMessage}");

        //2. Check Domain
        if (LicenseDetails.Domain != Environment.UserDomainName)
            throw new Exception($"#12: {_unifiedLicensingErrorMessage}");

        //3. Check Domain
        if (LicenseDetails.CpuSerial != GetCPUSerial())
            throw new Exception($"#13: {_unifiedLicensingErrorMessage}");
    }

    private bool VerifyToken(GeneralToken token)
    {
        if (token == null)
        {
            throw new Exception($"#14: {_unifiedLicensingErrorMessage}");
        }

        if (token.CheckData == null)
            return false;
        byte[] objCheckData = token.CheckData;
        token.CheckData = null;

        byte[] tempObj = GeneralToken.Serialize(token);
        return VerifyBytes(tempObj, objCheckData);
    }

    private bool VerifyBytes(byte[] bytesToVerify, byte[] signature)
    {
        X509Certificate2 verifyCert = new(Convert.FromBase64String(@"
MIIDVjCCAj6gAwIBAgIQQukjgYs+tzgKa1B2ZcbZ0zANBgkqhkiG9w0BAQsFADA4
MQwwCgYDVQQLDANESVMxDTALBgNVBAoMBEFCSVMxGTAXBgNVBAMMEEFCSVMgVURB
IExpY2Vuc2UwHhcNMjMwMjIwMDkwMzMyWhcNMzMwMjIwMDkwMzMyWjA4MQwwCgYD
VQQLDANESVMxDTALBgNVBAoMBEFCSVMxGTAXBgNVBAMMEEFCSVMgVURBIExpY2Vu
c2UwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCXYXZWMddNYD6XzWlm
NnhaME71vi3rMY5CcH9c6iL4Hwg+IC/Zv13BhsZZ2PooxtxF5o4aV8+jnGIvGMf1
JyhoWJuJYGNcIaV1RAUxn8QFLBshf+LhB7A3eIpb7ZgSNE5AT2doFNPizY1qLkki
Deqcqk/Er1YwoPGzBimzhqZKcneygpoghdyoB8QckHupDOhK16tFkjfnRrAevslN
SuGNmSgMxVNS7aC4ZBRDgQRLEKIdojVu1TNt9T5k+WYSzu4hkIEOoY2D16bClOgF
sUIdifx88crLwgWJJ3tIEkIlNg7pho0iSvinX++OOWxmmssjFUl8nbUyU1fSO/k8
zqGBAgMBAAGjXDBaMAsGA1UdDwQEAwIE8DAsBgNVHSUBAf8EIjAgBggrBgEFBQcD
BAYIKwYBBQUHAwIGCisGAQQBgjcKAwwwHQYDVR0OBBYEFBMGCXmSChx+QCeyV5cY
64AUh+XcMA0GCSqGSIb3DQEBCwUAA4IBAQAqzCJwt3ssZYeqy7TpJ7RRLtvHrAsq
gnffzxxgQ+DT9BKNHQJb9JVvs1hwd1R2CkvfpujUcAmhjM7koLGaloDXKVO2/CZi
waUjXDYHJsrZxnEmxa0FwrIhXU51YOaj0AeHFtgRXTU215CM+g31a1gK01TCL2NQ
sViP5NQVEJl0ValeG9o5VIwN/Ox2wQX1yephtvq/7H2ZEVdCjWgMJlcHQq1SNKxh
Nwx9UAHd0Pikcn5l4gxUq3go6G1BV+rAwhml8VRZTez5bWLn0CWaSl4YSV+oD2Cv
EJdMLrm7hDtTufOMHFNxSFgUmims5k+fGRXAD6TgX8zQXphdGJWLwu5h
"));
        if (verifyCert.GetRSAPublicKey() is not RSA rsa)
        {
            throw new Exception($"#15: {_unifiedLicensingErrorMessage}");
        }
        byte[] hash = _hashAlgorithm.ComputeHash(bytesToVerify);
        return rsa.VerifyHash(hash, signature, _hashAlgorithmName, RSASignaturePadding.Pkcs1);
    }

    private string GetCPUSerial()
    {
        using ManagementObjectCollection.ManagementObjectEnumerator enumerator = new ManagementClass("win32_processor").GetInstances().GetEnumerator();
        if (enumerator.MoveNext())
            return enumerator.Current.Properties["processorID"].Value.ToString() ?? string.Empty;

        throw new Exception($"#16: {_unifiedLicensingErrorMessage}");
    }

    public static string EncryptString(string input)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = sha256.ComputeHash(inputBytes);

        StringBuilder builder = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            builder.Append(hashBytes[i].ToString("x2"));
        }

        return builder.ToString();
    }
    #endregion
}
