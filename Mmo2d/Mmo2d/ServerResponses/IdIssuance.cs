﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d.ServerResponses
{
    class IdIssuance : IServerResponse
    {
        public Guid Id { get; set; }
    }
}
