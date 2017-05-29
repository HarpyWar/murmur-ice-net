// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

namespace MurmurPlugin
{
    public interface IVirtualServerCallbackHandler
    {
        void ChannelCreated(VirtualServerEntity.Channel channel, IVirtualServer server);
        void ChannelRemoved(VirtualServerEntity.Channel channel, IVirtualServer server);
        void ChannelStateChanged(VirtualServerEntity.Channel channel, IVirtualServer server);
        void UserConnected(VirtualServerEntity.OnlineUser user, IVirtualServer server);
        void UserDisconnected(VirtualServerEntity.OnlineUser user, IVirtualServer server);
        void UserStateChanged(VirtualServerEntity.OnlineUser user, IVirtualServer server);
        void UserTextMessage(VirtualServerEntity.OnlineUser user, string message, IVirtualServer server);

    }
}