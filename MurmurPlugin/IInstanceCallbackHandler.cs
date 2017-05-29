namespace MurmurPlugin
{
    public interface IInstanceCallbackHandler
    {
        void Started(IVirtualServer server);
        void Stopped(IVirtualServer server);
    }
}