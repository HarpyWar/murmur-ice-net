// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using Ice;
using MurmurPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Murmur
{
    public class InstanceCallbackWrapper : MetaCallbackDisp_
    {
        private Instance instance;
        private IInstanceCallbackHandler callbackHandler;

        public InstanceCallbackWrapper(Instance instance, IInstanceCallbackHandler callbackHandler)
        {
            this.instance = instance;
            this.callbackHandler = callbackHandler;
        }

        public override void started(ServerPrx srv, Current current__)
        {
            callbackHandler.Started(new VirtualServer(srv, instance));
        }

        public override void stopped(ServerPrx srv, Current current__)
        {
            callbackHandler.Stopped(new VirtualServer(srv, instance));
        }
    }
}
