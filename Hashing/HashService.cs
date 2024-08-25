using System.Security.Cryptography;
using System.Text;
using TNRD.Zeepkist.GTR.Backend.Comparers;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Hashing;

public interface IHashService
{
    string Hash(ZeepLevel zeepLevel);
}

public class HashService : IHashService
{
    private const int PresentBlockID = 2264;

    private readonly Vector3Comparer _vector3Comparer = new();
    private readonly SequenceComparer<int> _intSequenceComparer = new();
    private readonly SequenceComparer<float> _floatSequenceComparer = new();

    public string Hash(ZeepLevel zeepLevel)
    {
        StringBuilder inputBuilder = new();
        inputBuilder.AppendLine(zeepLevel.Skybox.ToString());
        inputBuilder.AppendLine(zeepLevel.Ground.ToString());

        List<ZeepBlock> orderedBlocks = zeepLevel.Blocks
            .Where(x => x.Id != PresentBlockID)
            .OrderBy(x => x.Id)
            .ThenBy(x => x.Position, _vector3Comparer)
            .ThenBy(x => x.Euler, _vector3Comparer)
            .ThenBy(x => x.Scale, _vector3Comparer)
            .ThenBy(x => x.Paints, _intSequenceComparer)
            .ThenBy(x => x.Options, _floatSequenceComparer)
            .ToList();

        foreach (ZeepBlock block in orderedBlocks)
        {
            inputBuilder.AppendLine(block.ToString());
        }

        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(inputBuilder.ToString()));
        StringBuilder hashBuilder = new(hash.Length * 2);

        foreach (byte b in hash)
        {
            hashBuilder.Append(b.ToString("X2"));
        }

        return hashBuilder.ToString();
    }
}
