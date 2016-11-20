using Mmo2d.Entities;
using Newtonsoft.Json;
using OpenTK;

namespace Mmo2d
{
    public class EntityStateUpdate
    {
        public long EntityId { get; set; }

        protected EntityStateUpdate()
        {
        }

        public EntityStateUpdate(long entityId) : this()
        {
            EntityId = entityId;
        }

        public Vector2? Displacement { get; set; }
        public bool? Died { get; set; }
        public bool? Jumped { get; set; }
        public bool? AttackInitiated { get; set; }
        public bool? CastFireball { get; set; }

        public int? HitsDelta { get; set; }
        public int? KillsDelta { get; set; }
        public Fireball AddFireball { get; set; }

        public bool? RemoveFireball { get; set; }

        [JsonIgnore]
        public bool ContainsInformation
        {
            get
            {
                return Displacement.HasValue || Died.HasValue || Jumped.HasValue || AttackInitiated.HasValue ||
                    HitsDelta.HasValue || KillsDelta.HasValue || CastFireball.HasValue || AddFireball != null || RemoveFireball.HasValue;
            }
        }
    }
}
