// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MurmurPlugin;

namespace Murmur
{
    public class Instance : IInstance, IDisposable
    {
        private SerializableDictionary<int, IVirtualServer> _servers = new SerializableDictionary<int, IVirtualServer>();


        private IceClient _client;
        private MetaPrx _meta;
        private Ice.ObjectAdapter _adapter;

        internal MetaPrx Meta
        {
            get
            {
                return _meta;
            }
        }
        internal Ice.ObjectAdapter Adapter
        {
            get
            {
                return _adapter;
            }
        }



        public void Connect(string address, int port = 6502, string secret = "", string callbackAddress = "127.0.0.1", int callbackPort = 0, int timeout = 1000)
        {
            _address = address;
            _port = port;
            _callbackAddress = callbackAddress;
            _callbackPort = callbackPort;
            _client = new IceClient();
            _client.Connect(address, port, secret, callbackAddress, callbackPort, timeout);
            _meta = _client.Meta;
            _adapter = _client.Adapter;
        }


        public string Address
        { 
            get 
            {
                return _address;
            }
        }
        private string _address;
        public int Port
        { 
            get 
            {
                return _port;
            }
        }
        private int _port;

        public string CallbackAddress
        {
            get
            {
                return _callbackAddress;
            }
        }
        private string _callbackAddress;

        public int CallbackPort
        {
            get
            {
                return _callbackPort;
            }
        }
        private int _callbackPort;


        /// <summary>
        /// Return all servers
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public SerializableDictionary<int, IVirtualServer> GetAllServers(bool cache = false)
        {
            // clear 
            //  (it's needed to update one time to cache)
            if (_isNewAllServers || !cache)
            {
                _isNewAllServers = false;
            
                // add to cache
                foreach (var s in _meta.getAllServers())
                {
                    var vs = new VirtualServer(s, this);
                    if (!_servers.ContainsKey(vs.Id))
                        _servers.Add(vs.Id, vs);
                }
            }

            return _servers;
        }
        private bool _isNewAllServers = true;

        /// <summary>
        /// Get one server
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="cache"></param>
        /// <returns>throws KeyNotFoundException if server not found</returns>
        public IVirtualServer GetServer(int serverId, bool cache = false)
        {
            if (!cache || !_servers.ContainsKey(serverId))
            {
                var server = _meta.getServer(serverId);
                if (server == null)
                    throw new KeyNotFoundException();

                var vs = new VirtualServer(server, this);

                // add to cache
                if (!_servers.ContainsKey(serverId))
                    _servers.Add(serverId, vs);
                else
                    _servers[serverId] = vs;
            }

            return _servers[serverId];
        }

        /// <summary>
        /// Create new server
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="port">if port == 0 then random port will be used</param>
        /// <returns></returns>
        public IVirtualServer CreateServer(int slots = 10, int port = 0)
        {
            // add server on remote side
            var server = _meta.newServer();

            var vs = new VirtualServer(server, this);

            // setup server
            vs.Port = (port == 0) ? GetNextAvailablePort() : port;
            vs.Slots = slots;

            // add to cache
            _servers.Add(vs.Id, vs);

            return vs;
        }

        /// <summary>
        /// Delete the server
        /// If something wrong exception will be thrown
        /// </summary>
        public void DeleteServer(int serverId)
        {
            var server = _meta.getServer(serverId);

            // delete on remote side
            server.delete();
            
            // remove from cache
            if (_servers.ContainsKey(serverId))
                _servers.Remove(serverId);
        }


        /// <summary>
        /// Return version on Murmur
        /// </summary>
        /// <returns></returns>
        public string GetVersionString()
        {
            if (_version == null)
            {
                int major = 0, minor = 0, patch = 0;
                _meta.getVersion(out major, out minor, out patch, out _version);
                //return string.Format("{0}.{1}.{2}", major, minor, patch);
            }
            return _version;
        }
        private string _version;

        /// <summary>
        /// Return version string of Murmur122.ice
        /// </summary>
        /// <returns></returns>
        public static string GetIceVersionString()
        {
            // assembly file version   
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            return assemblyVersion.ToString();
        }

        /// <summary>
        /// Check connection is estabilished or not
        /// </summary>
        public bool IsConnected()
        {
            try
            {
                _meta.ice_ping();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Default config values (always cached)
        /// </summary>
        /// <returns></returns>
        public SerializableDictionary<string, string> GetDefaultConf()
        {
            if (_defaultConf != null)
                return _defaultConf;

            _defaultConf = new SerializableDictionary<string, string>();
            foreach (var c in _meta.getDefaultConf())
            {
                _defaultConf.Add(c.Key, c.Value);
            }
            return _defaultConf;
        }
        private SerializableDictionary<string, string> _defaultConf = null;


        /// <summary>
        /// Return next available server port
        /// </summary>
        /// <returns></returns>
        public int GetNextAvailablePort()
        {
            int defaultPort = int.Parse(GetDefaultConf()["port"]);
            var lastId = GetAllServers().Last().Key;

            int port = 0;
            // countdown lastId value while it not found 
            while (port == 0)
            {
                // increase port to (base port + max sever id)
                int newPort = (defaultPort + lastId);

                // if port greater then 65535, then move port number in the opposite direction
                if (newPort > ushort.MaxValue)
                    newPort = ushort.MaxValue - (newPort - defaultPort);

                // check port for availability (it should be closed)
                if (!Helper.IsPortOpened(Address, newPort, 2))
                {
                    port = newPort;
                    break;
                }
                lastId++;
            }
            return port;
        }

        #region CALLBACKS

        private Dictionary<string, MetaCallbackPrx> callbacks = new Dictionary<string, MetaCallbackPrx>();

        public string AddCallback(IInstanceCallbackHandler callback)
        {
            if (_client == null)
            {
                throw new InsufficientExecutionStackException("You have to Connect() before add AddCallback()");
            }
            var callbackWrapper = new InstanceCallbackWrapper(this, callback);

            // Create identity and callback for Metaserver
            var key = Guid.NewGuid().ToString();
            var meta_callback = MetaCallbackPrxHelper.checkedCast(_adapter.add(callbackWrapper, new Ice.Identity(key, "")));
            _meta.addCallback(meta_callback);

            callbacks.Add(key, meta_callback);
            return key;
        }


        internal KeyValuePair<string, ServerCallbackPrx> AddVirtualServerCallback(IVirtualServerCallbackHandler callback, VirtualServer vs)
        {
            var callbackWrapper = new VirtualServerCallbackWrapper(vs, callback);

            // Create identity and callback for Virtual server
            var key = Guid.NewGuid().ToString();
            var server_callback = ServerCallbackPrxHelper.checkedCast(_adapter.add(callbackWrapper, new Ice.Identity(key, "")));
            vs.Server.addCallback(server_callback);
            return new KeyValuePair<string, ServerCallbackPrx>(key, server_callback);
        }

        internal KeyValuePair<string, ServerContextCallbackPrx> AddVirtualServerContextCallback(int session, string action, string title, IVirtualServerContextCallbackHandler callback, int ctx, VirtualServer vs)
        {
            var callbackWrapper = new VirtualServerContextCallbackWrapper(vs, callback);

            // Create identity and callback for Virtual server
            var key = Guid.NewGuid().ToString();
            var server_context_callback = ServerContextCallbackPrxHelper.checkedCast(_adapter.add(callbackWrapper, new Ice.Identity(key, "")));
            vs.Server.addContextCallback(session, action, title, server_context_callback, ctx);
            return new KeyValuePair<string, ServerContextCallbackPrx>(key, server_context_callback);
        }


        /// <summary>
        /// Remove instance callback by id
        /// </summary>
        /// <param name="id">If id == null then remove all callbacks</param>
        public void RemoveCallback(string id = null)
        {
            foreach (var c in callbacks)
            {
                if (id != null && id != c.Key)
                {
                    continue;
                }
                _meta.removeCallback(c.Value);
            }
        }

        #endregion



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // clear all callbacks
                    RemoveCallback(null);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VirtualServer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}