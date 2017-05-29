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
    public class VirtualServerContextCallbackWrapper : ServerContextCallbackDisp_
    {

        private VirtualServer server;
        private IVirtualServerContextCallbackHandler callbackHandler;

        public VirtualServerContextCallbackWrapper(VirtualServer vs, IVirtualServerContextCallbackHandler callbackHandler)
        {
            this.server = vs;
            this.callbackHandler = callbackHandler;
        }

        public override void contextAction(string action, User usr, int session, int channelid, Current current__)
        {
            callbackHandler.ContextAction(action, VirtualServer.getOnlineUser(usr), session, channelid, server);
        }
    }
}
