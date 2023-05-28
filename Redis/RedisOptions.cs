namespace TNRD.Zeepkist.GTR.Backend.Redis;

internal class RedisOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Password { get; set; } = null!;
}
