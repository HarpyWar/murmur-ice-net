// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MurmurPlugin
{
    public enum Context
    {
        Server = 1, // Murmur.ContextServer
        Channel = 2, // Murmur.ContextChannel
        User = 4 // Murmur.ContextUser
    }
}
