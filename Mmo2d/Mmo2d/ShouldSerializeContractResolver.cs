using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(Vector2) &&
                (property.PropertyName == "PerpendicularLeft" ||
                 property.PropertyName == "PerpendicularRight" ||
                 property.PropertyName == "Length" ||
                 property.PropertyName == "LengthFast" ||
                 property.PropertyName == "LengthSquared" ||
                 property.PropertyName == "Yx"))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        return false;
                    };
            }

            return property;
        }
    }
}
