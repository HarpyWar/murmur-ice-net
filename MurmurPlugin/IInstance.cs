// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

namespace MurmurPlugin
{
    public interface IInstance
    {
        string Address { get; }
        int Port { get; }


        void Connect(string address, int port, string secret, string callbackAddress = "127.0.0.1", int callbackPort = 0, int timeout = 1000);

        /// <summary>
        /// Return all servers
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        SerializableDictionary<int, IVirtualServer> GetAllServers(bool cache = false);

        /// <summary>
        /// Get one server
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="cache"></param>
        /// <returns>throws KeyNotFoundException if server not found</returns>
        IVirtualServer GetServer(int serverId, bool cache = false);

        /// <summary>
        /// Create new server
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="port">if port == 0 then random port will be used</param>
        /// <returns></returns>
        IVirtualServer CreateServer(int slots = 10, int port = 0);

        /// <summary>
        /// Delete the server
        /// If something wrong exception will be thrown
        /// </summary>
        void DeleteServer(int serverId);

        /// <summary>
        /// Check connection is estabilished or not
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// Default config values (always cached)
        /// </summary>
        /// <returns></returns>
        SerializableDictionary<string, string> GetDefaultConf();

        /// <summary>
        /// Return next available server port
        /// </summary>
        /// <returns></returns>
        int GetNextAvailablePort();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>callback id</returns>
        string AddCallback(IInstanceCallbackHandler callback);

        /// <summary>
        /// Remove instance callback by id
        /// </summary>
        /// <param name="id">If id == null then remove all callbacks</param>
        void RemoveCallback(string id = null);
    }
}