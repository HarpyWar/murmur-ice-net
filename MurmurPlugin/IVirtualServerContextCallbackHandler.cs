namespace MurmurPlugin
{
    public interface IVirtualServerContextCallbackHandler
    {
        void ContextAction(string action, VirtualServerEntity.OnlineUser user, int session, int channelId, IVirtualServer server);
    }
}