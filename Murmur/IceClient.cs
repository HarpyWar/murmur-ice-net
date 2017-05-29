// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using Ice;
using System.Threading;
using MurmurPlugin;

namespace Murmur
{
    internal class IceClient : Ice.Application
    {
        private Ice.InitializationData _data;
        private Ice.Communicator ic;
        private int timeout;

        public MetaPrx Meta
        {
            get
            {
                return _meta;
            }
        }
        private MetaPrx _meta;

        public Ice.ObjectAdapter Adapter
        {
            get
            {
                return _adapter;
            }
        }
        private Ice.ObjectAdapter _adapter;

        /// <summary>
        /// Initialize data (do not use it, it is execute before Connect)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override int run(String[] args)
        {
            //
            // Terminate cleanly on receipt of a signal.
            //
            shutdownOnInterrupt();

            _data = new Ice.InitializationData();
            _data.properties = Ice.Util.createProperties();
            _data.properties.setProperty("Ice.ImplicitContext", "Shared");
            _data.properties.setProperty("Ice.MessageSizeMax", "65535");
            //_data.properties.setProperty("Ice.Override.Timeout", "1000"); // https://doc.zeroc.com/pages/viewpage.action?pageId=2523191
            _data.logger = new ConsoleLoggerI("ice_");

#if COMPACT
                        //
                        // When using Ice for .NET Compact Framework, we need to specify
                        // the assembly so that Ice can locate classes and exceptions.
                        //
                        _data.properties.setProperty("Ice.FactoryAssemblies", "client,version=1.0.0.0");
#endif

            return 0;
        }

        /// <summary>
        /// Connect to ICE interface on running Murmur server
        /// </summary>
        /// <param name="address">murmur server address</param>
        /// <param name="port">ice port</param>
        /// <param name="secret">ice password</param>
        /// <param name="timeout"></param>
        /// <returns>object of murmur server</returns>
        public MetaPrx Connect(string address, int port, string secret, string callbackAddress = "127.0.0.1", int callbackPort = 0, int timeout = 1000)
        {
            this.timeout = timeout;

            // initialize
            run(new string[] { });

            ic = Ice.Util.initialize(_data);

            // set ice secret
            if (!string.IsNullOrEmpty(secret))
                ic.getImplicitContext().put("secret", secret);
            
            try
            {
                var connectionString = string.Format("Meta:tcp -h {0} -p {1} -t {2}", address, port, timeout);
                
                // Create a proxy
                var obj = ic.stringToProxy(connectionString);
       
                // Tests whether the target object of this proxy can be reached.
                obj.ice_ping();
                
                // initialize metadata of murmur server
                _meta = MetaPrxHelper.uncheckedCast(obj);


                var portString = (callbackPort > 0) ? string.Format("-p {0}", callbackPort) : "";
                var connectionStringCB = string.Format("tcp -h {0} {1} -t {2}", callbackAddress, portString, timeout);

                // Create the callback adapter
                ic.getProperties().setProperty("Ice.PrintAdapterReady", "0");
                _adapter = ic.createObjectAdapterWithEndpoints("Callback.Client", connectionStringCB);
                _adapter.activate();

                _meta.ice_ping();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new MurmurPlugin.ConnectionRefusedException();
            }

            // FIXME: check for valid ice secret, because getLogLen() is working only if "icesecretwrite" is valid
            //  (there a secure hole - get methods allowed if "icesecretread" is not set)
            //  if secret is invalid it throws UnmarshalOutOfBoundsException();
            try
            {
                // set allowhtml property to the server to check ice writesecret
                _meta.getAllServers()[0].setConf("allowhtml", "true");
            }
            catch
            {
                throw new MurmurPlugin.InvalidSecretException();
            }

            return _meta;
        }



    }


}