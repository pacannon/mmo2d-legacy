﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d.ServerMessages
{
    public class KeyPress : ServerMessage
    {
        public char TypedCharacter { get; set; }
    }
}
