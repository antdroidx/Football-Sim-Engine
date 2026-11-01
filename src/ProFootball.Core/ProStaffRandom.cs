using System.Security.Cryptography;
using System.Text;
namespace ProFootball.Core;

/// <summary>Stable non-crypto seed source for Pro staff lifecycles and candidate pools.</summary>
public static class ProStaffRandom
{
    public static int Salt(Guid id) => BitConverter.ToInt32(id.ToByteArray(), 0) & int.MaxValue;
    public static int Create(Guid franchiseId, int year, int salt, string first, string second)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{franchiseId:N}|{year}|{salt}|{first}|{second}"));
        return BitConverter.ToInt32(bytes, 0) & int.MaxValue;
    }
}
