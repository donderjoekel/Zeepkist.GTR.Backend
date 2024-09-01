using System.Numerics;

namespace TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;

public class ZeepLevel
{
    public ZeepLevel()
    {
        Blocks = new List<ZeepBlock>();
    }

    public string SceneName { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
    public string UniqueId { get; set; } = null!;

    public Vector3 CameraPosition { get; set; }
    public Vector3 CameraEuler { get; set; }
    public Vector2 CameraRotation { get; set; }

    public bool IsValidated { get; set; }
    public float ValidationTime { get; set; }
    public float GoldTime { get; set; }
    public float SilverTime { get; set; }
    public float BronzeTime { get; set; }

    public int Skybox { get; set; }
    public int Ground { get; set; }

    public List<ZeepBlock> Blocks { get; set; }
}
