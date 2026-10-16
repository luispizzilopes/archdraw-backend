using System.Security.Cryptography;
using System.Text;

namespace Archdraw.Application.Common;

public static class DeterministicGuid
{
    public static Guid Create(Guid namespaceId, string name)
    {
        var ns = namespaceId.ToByteArray();
        SwapByteOrder(ns);
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var data = new byte[ns.Length + nameBytes.Length];
        Buffer.BlockCopy(ns, 0, data, 0, ns.Length);
        Buffer.BlockCopy(nameBytes, 0, data, ns.Length, nameBytes.Length);

        var hash = SHA1.HashData(data);
        var result = new byte[16];
        Array.Copy(hash, 0, result, 0, 16);
        result[6] = (byte)((result[6] & 0x0F) | 0x50);
        result[8] = (byte)((result[8] & 0x3F) | 0x80);
        SwapByteOrder(result);
        return new Guid(result);
    }

    private static void SwapByteOrder(byte[] guid)
    {
        (guid[0], guid[3]) = (guid[3], guid[0]);
        (guid[1], guid[2]) = (guid[2], guid[1]);
        (guid[4], guid[5]) = (guid[5], guid[4]);
        (guid[6], guid[7]) = (guid[7], guid[6]);
    }
}
