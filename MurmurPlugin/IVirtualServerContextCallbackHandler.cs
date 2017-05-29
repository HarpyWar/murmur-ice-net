// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

namespace MurmurPlugin
{
    public interface IVirtualServerContextCallbackHandler
    {
        void ContextAction(string action, VirtualServerEntity.OnlineUser user, int session, int channelId, IVirtualServer server);
    }
}