using Newtonsoft.Json;
using OpenTK;
using System;

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
        public TimeSpan? TimeSinceDeathDelta { get; set; }
        public TimeSpan? TimeSinceJumpDelta { get; set; }
        public TimeSpan? TimeSinceAttackInitiatedDelta { get; set; }

        public bool? NullOutTimeSinceJump { get; set; }
        public bool? NullOutTimeSinceAttackInitiated { get; set; }
        public int? HitsDelta { get; set; }
        public int? KillsDelta { get; set; }

        [JsonIgnore]
        public bool ContainsInformation
        {
            get
            {
                return Displacement.HasValue || TimeSinceDeathDelta.HasValue || TimeSinceJumpDelta.HasValue || TimeSinceAttackInitiatedDelta.HasValue ||
                    NullOutTimeSinceJump.HasValue || NullOutTimeSinceAttackInitiated.HasValue || HitsDelta.HasValue || KillsDelta.HasValue;
            }
        }
    }
}
