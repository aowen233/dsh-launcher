using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace DshLauncher;

internal static class DpapiSecret
{
    private const int CryptProtectUiForbidden = 0x1;

    public static string Protect(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        byte[] plainText = Encoding.UTF8.GetBytes(value);
        try
        {
            return Convert.ToBase64String(ProtectBytes(plainText));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainText);
        }
    }

    public static string Unprotect(string protectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedValue);
        byte[] cipherText = Convert.FromBase64String(protectedValue);
        byte[] plainText = UnprotectBytes(cipherText);
        try
        {
            return Encoding.UTF8.GetString(plainText);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainText);
            CryptographicOperations.ZeroMemory(cipherText);
        }
    }

    private static byte[] ProtectBytes(byte[] value)
    {
        DataBlob input = CreateBlob(value);
        try
        {
            if (!CryptProtectData(
                    ref input,
                    "DSH Launcher local access URL",
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    CryptProtectUiForbidden,
                    out DataBlob output))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not protect the launcher state.");
            }

            return CopyAndFree(output);
        }
        finally
        {
            FreeBlob(input);
        }
    }

    private static byte[] UnprotectBytes(byte[] value)
    {
        DataBlob input = CreateBlob(value);
        try
        {
            if (!CryptUnprotectData(
                    ref input,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    CryptProtectUiForbidden,
                    out DataBlob output))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not unlock the launcher state.");
            }

            return CopyAndFree(output);
        }
        finally
        {
            FreeBlob(input);
        }
    }

    private static DataBlob CreateBlob(byte[] value)
    {
        IntPtr pointer = Marshal.AllocHGlobal(value.Length);
        Marshal.Copy(value, 0, pointer, value.Length);
        return new DataBlob { Length = value.Length, Data = pointer };
    }

    private static byte[] CopyAndFree(DataBlob blob)
    {
        try
        {
            var value = new byte[blob.Length];
            Marshal.Copy(blob.Data, value, 0, blob.Length);
            return value;
        }
        finally
        {
            if (blob.Data != IntPtr.Zero)
            {
                Marshal.Copy(new byte[blob.Length], 0, blob.Data, blob.Length);
                LocalFree(blob.Data);
            }
        }
    }

    private static void FreeBlob(DataBlob blob)
    {
        if (blob.Data == IntPtr.Zero) return;
        Marshal.Copy(new byte[blob.Length], 0, blob.Data, blob.Length);
        Marshal.FreeHGlobal(blob.Data);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int Length;
        public IntPtr Data;
    }

    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptProtectData(
        ref DataBlob dataIn,
        string? description,
        IntPtr optionalEntropy,
        IntPtr reserved,
        IntPtr prompt,
        int flags,
        out DataBlob dataOut);

    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptUnprotectData(
        ref DataBlob dataIn,
        IntPtr description,
        IntPtr optionalEntropy,
        IntPtr reserved,
        IntPtr prompt,
        int flags,
        out DataBlob dataOut);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);
}
