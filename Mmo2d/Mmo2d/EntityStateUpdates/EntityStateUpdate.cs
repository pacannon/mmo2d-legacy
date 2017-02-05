using Mmo2d.Entities;
using Newtonsoft.Json;
using OpenTK;

namespace Mmo2d.EntityStateUpdates
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
        public long? CastFireball { get; set; }

        public int? HpDelta { get; set; }
        public int? KillsDelta { get; set; }

        // Merge this into game state update
        public Fireball AddFireball { get; set; }

        public bool? RemoveFireball { get; set; }

        public long? SetTargetId { get; set; }
        public bool? DeselectTarget { get; set; }

        public long? StartCastFireball { get;  set; }
        public bool? AutoAttack { get; set; }

        [JsonIgnore]
        public bool ContainsInformation
        {
            get
            {
                return Displacement.HasValue || Died.HasValue || Jumped.HasValue || AttackInitiated.HasValue ||
                    HpDelta.HasValue || KillsDelta.HasValue || CastFireball.HasValue || AddFireball != null || RemoveFireball.HasValue || 
                    SetTargetId.HasValue || DeselectTarget.HasValue || StartCastFireball.HasValue || AutoAttack.HasValue;
            }
        }
    }
}
