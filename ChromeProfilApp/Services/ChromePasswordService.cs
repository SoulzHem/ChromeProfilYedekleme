using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace ChromeProfilApp.Services;

public sealed class ChromePasswordService
{
    private byte[]? _standardKey;
    private byte[]? _appBoundKey;
    private bool _appBoundAttempted;

    public List<Models.SavedPassword> GetPasswords(string profilePath, string chromeUserData)
    {
        var loginDataPath = Path.Combine(profilePath, "Login Data");
        if (!File.Exists(loginDataPath))
            return [];

        var localStatePath = Path.Combine(chromeUserData, "Local State");
        if (!File.Exists(localStatePath))
            return [];

        _standardKey = TryGetStandardKey(localStatePath);
        _appBoundKey = null;
        _appBoundAttempted = false;

        var tempDb = ChromeFileHelper.CopyToTemp(loginDataPath, "chrome_login");
        try
        {
            using var conn = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT origin_url, username_value, password_value
                FROM logins
                WHERE (origin_url IS NOT NULL AND origin_url <> '')
                   OR (username_value IS NOT NULL AND username_value <> '')
                ORDER BY origin_url
                """;

            var results = new List<Models.SavedPassword>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var site = reader.IsDBNull(0) ? "" : reader.GetString(0);
                var username = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var encrypted = ReadBlob(reader, 2);

                var password = DecryptPassword(localStatePath, encrypted);
                results.Add(new Models.SavedPassword
                {
                    Site = site,
                    Username = username,
                    Password = password
                });
            }

            return results;
        }
        finally
        {
            TryDelete(tempDb);
        }
    }

    public int GetPasswordCount(string profilePath)
    {
        var loginDataPath = Path.Combine(profilePath, "Login Data");
        if (!File.Exists(loginDataPath)) return 0;

        var tempDb = ChromeFileHelper.CopyToTemp(loginDataPath, "chrome_login");
        try
        {
            using var conn = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM logins";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch
        {
            return 0;
        }
        finally
        {
            TryDelete(tempDb);
        }
    }

    private string DecryptPassword(string localStatePath, byte[] encrypted)
    {
        if (encrypted.Length == 0) return "";

        var prefix = encrypted.Length >= 3
            ? Encoding.ASCII.GetString(encrypted, 0, 3)
            : "";

        if (prefix is "v10" or "v11")
        {
            if (_standardKey != null)
            {
                var plain = TryAesGcm(_standardKey, encrypted);
                if (plain != null) return plain;
            }
            return "(v10 sifre cozulemedi)";
        }

        if (prefix == "v20")
        {
            if (!_appBoundAttempted)
            {
                _appBoundAttempted = true;
                _appBoundKey = ChromeAppBoundKeyService.TryGetAppBoundKey(localStatePath);
            }

            if (_appBoundKey != null)
            {
                var plain = TryAesGcm(_appBoundKey, encrypted);
                if (plain != null) return plain;
            }

            return "(Chrome v20 korumasi - sifre goruntulenemiyor)";
        }

        try
        {
            var legacy = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(legacy);
        }
        catch
        {
            return "(sifre cozulemedi)";
        }
    }

    private static byte[]? TryGetStandardKey(string localStatePath)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(localStatePath));
            if (!doc.RootElement.TryGetProperty("os_crypt", out var osCrypt)) return null;
            if (!osCrypt.TryGetProperty("encrypted_key", out var keyProp)) return null;

            var encryptedKey = Convert.FromBase64String(keyProp.GetString()!);
            var dpapiBlob = encryptedKey.AsSpan(5).ToArray();
            return ProtectedData.Unprotect(dpapiBlob, null, DataProtectionScope.CurrentUser);
        }
        catch
        {
            return null;
        }
    }

    private static string? TryAesGcm(byte[] key, byte[] encrypted)
    {
        try
        {
            var cipherLen = encrypted.Length - 3 - 12 - 16;
            if (cipherLen <= 0) return null;

            var nonce = encrypted.AsSpan(3, 12);
            var ciphertext = encrypted.AsSpan(15, cipherLen);
            var tag = encrypted.AsSpan(encrypted.Length - 16, 16);
            var plaintext = new byte[cipherLen];

            using var aes = new AesGcm(key, 16);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch
        {
            return null;
        }
    }

    private static byte[] ReadBlob(SqliteDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal)) return [];

        if (reader.GetFieldType(ordinal) == typeof(byte[]))
            return (byte[])reader.GetValue(ordinal);

        var length = reader.GetBytes(ordinal, 0, null, 0, 0);
        var buffer = new byte[length];
        reader.GetBytes(ordinal, 0, buffer, 0, (int)length);
        return buffer;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
    }
}

internal static class ChromeAppBoundKeyService
{
    private static readonly Guid ClsidChromeElevator = new("708860E0-F641-4611-8891-548BFAFA886");
    private static readonly Guid IidIElevatorChrome = new("463ABECF-410D-407F-8AF5-0DF35A031CC");
    private static readonly Guid IidIElevator2Chrome = new("1BF5208B-295F-4992-B5F4-3A9BB649483A");

    public static byte[]? TryGetAppBoundKey(string localStatePath)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(localStatePath));
            if (!doc.RootElement.TryGetProperty("os_crypt", out var osCrypt)) return null;
            if (!osCrypt.TryGetProperty("app_bound_encrypted_key", out var keyProp)) return null;

            var encrypted = Convert.FromBase64String(keyProp.GetString()!);
            if (encrypted.Length <= 4) return null;

            var payload = encrypted.AsSpan(4).ToArray();
            return TryElevatorDecrypt(payload);
        }
        catch
        {
            return null;
        }
    }

    private static byte[]? TryElevatorDecrypt(byte[] ciphertext)
    {
        var result = TryElevatorDecryptWithIid(IidIElevator2Chrome, ciphertext);
        return result ?? TryElevatorDecryptWithIid(IidIElevatorChrome, ciphertext);
    }

    private static byte[]? TryElevatorDecryptWithIid(Guid iid, byte[] ciphertext)
    {
        try
        {
            var hr = CoCreateInstance(ClsidChromeElevator, IntPtr.Zero, 1, iid, out var elevatorPtr);
            if (hr != 0 || elevatorPtr == IntPtr.Zero) return null;

            try
            {
                var bstrIn = SysAllocStringByteLen(ciphertext, (uint)ciphertext.Length);
                try
                {
                    if (bstrIn == IntPtr.Zero) return null;

                    var vtable = Marshal.ReadIntPtr(elevatorPtr);
                    var decryptPtr = Marshal.ReadIntPtr(vtable, 5 * IntPtr.Size);
                    var decrypt = Marshal.GetDelegateForFunctionPointer<DecryptDataDelegate>(decryptPtr);

                    var hrDecrypt = decrypt(elevatorPtr, bstrIn, out var bstrOut, out _);
                    if (hrDecrypt != 0 || bstrOut == IntPtr.Zero) return null;

                    var len = SysStringByteLen(bstrOut);
                    if (len <= 0) return null;

                    var bytes = new byte[len];
                    Marshal.Copy(bstrOut, bytes, 0, len);
                    SysFreeString(bstrOut);
                    return bytes is { Length: >= 32 } ? bytes.AsSpan(0, 32).ToArray() : bytes;
                }
                finally
                {
                    if (bstrIn != IntPtr.Zero) SysFreeString(bstrIn);
                }
            }
            finally
            {
                Marshal.Release(elevatorPtr);
            }
        }
        catch
        {
            return null;
        }
    }

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
        out IntPtr ppv);

    [DllImport("oleaut32.dll")]
    private static extern int SysStringByteLen(IntPtr bstr);

    [DllImport("oleaut32.dll")]
    private static extern IntPtr SysAllocStringByteLen(byte[] str, uint len);

    [DllImport("oleaut32.dll")]
    private static extern void SysFreeString(IntPtr bstr);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int DecryptDataDelegate(
        IntPtr thisPtr,
        IntPtr ciphertext,
        out IntPtr plaintext,
        out uint lastError);
}
