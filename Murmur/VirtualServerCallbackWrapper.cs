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
    public class VirtualServerCallbackWrapper : ServerCallbackDisp_
    {

        private VirtualServer server;
        private IVirtualServerCallbackHandler callbackHandler;

        public VirtualServerCallbackWrapper(VirtualServer vs, IVirtualServerCallbackHandler callbackHandler)
        {
            this.server = vs;
            this.callbackHandler = callbackHandler;
        }

        public override void channelCreated(Channel state, Current current__)
        {
            callbackHandler.ChannelCreated(VirtualServer.getChannel(state), server);
        }

        public override void channelRemoved(Channel state, Current current__)
        {
            callbackHandler.ChannelRemoved(VirtualServer.getChannel(state), server);
        }

        public override void channelStateChanged(Channel state, Current current__)
        {
            callbackHandler.ChannelStateChanged(VirtualServer.getChannel(state), server);
        }

        public override void userConnected(User state, Current current__)
        {
            callbackHandler.UserConnected(VirtualServer.getOnlineUser(state), server);
        }

        public override void userDisconnected(User state, Current current__)
        {
            callbackHandler.UserDisconnected(VirtualServer.getOnlineUser(state), server);
        }

        public override void userStateChanged(User state, Current current__)
        {
            callbackHandler.UserStateChanged(VirtualServer.getOnlineUser(state), server);
        }

#if MURMUR_123380
        public override void userTextMessage(User state, TextMessage message, Current current__)
        {
            // UNDONE: create wrapper for TextMessage, it has more context than just a message text
            callbackHandler.UserTextMessage(VirtualServer.getOnlineUser(state), message.text, server);
        }
#endif

    }
}
