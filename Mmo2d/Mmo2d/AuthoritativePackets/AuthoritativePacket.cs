namespace Mmo2d.AuthoritativePackets
{
    public class AuthoritativePacket
    {
        public long? IdIssuance { get; set; }
        public GameState State { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
