using Mmo2d.Entities;
using Newtonsoft.Json;
using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace Mmo2d.EntityStateUpdates
{
    public class EntityStateUpdate
    {
        public long EntityId { get; set; }

        protected EntityStateUpdate()
        {
            HpDeltas = new List<int>();
        }

        public EntityStateUpdate(long entityId) : this()
        {
            EntityId = entityId;
        }

        public Vector2? Displacement { get; set; }
        public bool? Died { get; set; }
        public bool? Jumped { get; set; }
        public bool? AttackInitiated { get; set; }

        public List<int> HpDeltas { get; set; }
        public int? KillsDelta { get; set; }

        // Merge this into game state update
        public Projectile AddFireball { get; set; }

        public bool? RemoveProjectile { get; set; }

        public long? SetTargetId { get; set; }
        public bool? DeselectTarget { get; set; }

        public long? StartCastFireball { get; set; }
        public long? StartCastFrostbolt { get; set; }
        public bool? AutoAttack { get; set; }
        public bool? Remove { get; set; }
        public Entity Add { get; set; }

        [JsonIgnore]
        public bool ContainsInformation
        {
            get
            {
                return Displacement.HasValue || Died.HasValue || Jumped.HasValue || AttackInitiated.HasValue ||
                    HpDeltas.Any() || KillsDelta.HasValue || AddFireball != null || RemoveProjectile.HasValue || 
                    SetTargetId.HasValue || DeselectTarget.HasValue || StartCastFireball.HasValue || StartCastFrostbolt.HasValue || AutoAttack.HasValue || Remove.HasValue || Add != null;
            }
        }
    }
}
