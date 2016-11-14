namespace Mmo2d.AuthoritativePackets
{
    public class AuthoritativePacket
    {
        public long? IdIssuance { get; set; }
        public GameState GameState { get; set; }
        public GameStateDelta GameStateDelta { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
