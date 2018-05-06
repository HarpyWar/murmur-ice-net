// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;

namespace MurmurPlugin
{
    public interface IVirtualServer : IDisposable
    {
        IVirtualServerKeeper Keeper { get; }

        VirtualServerEntity GetEntity();

        /// <summary>
        /// Flag notation the virtual server is online or not
        /// </summary>
        bool IsRunning(bool cache = false);

        /// <summary>
        /// Return current users online
        ///  (return null if it can't retrieve online)
        /// </summary>
        int? GetOnline();

        /// <summary>
        /// Return server uptime
        /// </summary>
        /// <returns></returns>
        int GetUptime(bool cache = false);

        bool Start();
        bool Stop();
        bool Restart();

        /// <summary>
        /// Send text message to user
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="text"></param>
        void SendMessage(int sessionId, string text);

        /// <summary>
        /// Send text message to channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="text"></param>
        /// <param name="tree"></param>
        void SendMessageChannel(int channelId, string text, bool tree = true);

        /// <summary>
        /// Set password for superuser account
        /// </summary>
        /// <param name="password">if null or empty then random password will be generated</param>
        void SetSuperuserPassword(string password);

        /// <summary>
        /// Return log file as string
        /// </summary>
        /// <param name="lines">lines cound from the end of log</param>
        /// <param name="format">format unixtime to readable date?</param>
        /// <returns></returns>
        string GetLog(int lines, bool format = true);




        /// <summary>
        /// Server unique identifier
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Server port
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Max number of users
        /// </summary>
        int Slots { get; set; }

        /// <summary>
        /// The blurb sent by the server on connect
        /// </summary>
        string WelcomeMessage { get; set; }

        /// <summary>
        /// Server name (channel root name)
        /// (The data for registration in the public server list)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Server PW
        /// (The data for registration in the public server list)
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Allow html in chat messages/user comments/channel descriptions
        /// </summary>
        bool AllowHtml { get; set; }

        /// <summary>
        /// Default channel id where users joined
        ///  (to apply this iption on registered users set RememberChannel = false )
        /// </summary>
        int DefaultChannel { get; set; }

        /// <summary>
        /// Bandwidth restriction in kbit/sec (max 320 kbit/s)
        /// </summary>
        int Bandwidth { get; set; }

        /// <summary>
        /// Client inactivity timeout in ms
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Max users per channel
        /// FIXME: doesn't work 
        /// </summary>
        int MaxUsersPerChannel { get; set; }

        /// <summary>
        /// Max message text length in chat
        /// </summary>
        int MaxTextMessageLength { get; set; }

        /// <summary>
        /// Regexp on channel name
        /// </summary>
        string ChannelNameRegex { get; set; }

        /// <summary>
        /// Regexp on username
        /// </summary>
        string UserNameRegex { get; set; }

        /// <summary>
        /// Should the server remember last channel for registered users?
        /// </summary>
        bool RememberChannel { get; set; }

        /// <summary>
        /// Mumble supports strong authentication via client certificates, and providing you have an RPC mechanism in place to set them, weak authentication via passwords. 
        /// By default, users without certificates are still allowed on a server if they know the serverpassword, 
        /// or if there isn't one set - however they cannot self-register, even if the server allows it (as there's no certificate to register).
        /// 
        /// Setting CertificateRequired to true ensures that only clients who possess a certificate of some kind are allowed on the server. 
        /// This is mostly safe, as recent Mumble versions will automatically generate a certificate if the certificate wizard is closed prematurely.
        /// (if true then setup Certificate/CertificateKey)
        /// </summary>
        bool CertificateRequired { get; set; }

        /// <summary>
        /// x509 certificate in PEM form
        /// </summary>
        string Certificate { get; set; }

        /// <summary>
        /// x509 private key in PEM form
        /// </summary>
        string CertificateKey { get; set; }

        /// <summary>
        /// This setting is the DNS hostname where your server can be reached. 
        /// It only needs to be set if you want your server to be addressed in the server list by it's hostname instead of by IP, 
        /// but if it's set it must resolve on the internet or registration will fail.
        /// (The data for registration in the public server list)
        /// 
        /// Example: mumble.some.host
        /// </summary>
        string RegisterHostname { get; set; }

        /// Server site/URL/blog with it will be registered
        /// (The data for registration in the public server list)
        /// 
        /// Example: http://mumble.sourceforge.net
        /// </summary>
        string RegisterUrl { get; set; }

        /// <summary>
        /// Plain-text secret between your server and the registration server. 
        /// It's sole purpose is to prevent other servers from impersonating your server in the public server list.
        /// Set this setting empty to disable registration with the public server list.
        ///  (The data for registration in the public server list)
        /// </summary>
        string RegisterPassword { get; set; }


        /// <summary>
        /// Return all config options
        /// </summary>
        /// <param name="getDefault">Include default config values if they not exists in the server config</param>
        /// <param name="cache"></param>
        /// <returns>Dictionary enumeration of config key-value</returns>
        SerializableDictionary<string, string> GetAllConf(bool getDefault, bool cache = false);

        /// <summary>
        /// Get value from config
        /// </summary>
        /// <param name="key">option identifier (dictionary key)</param>
        /// <param name="getDefault">Return default config values if they not exists in the server config</param>
        /// <param name="cache"></param>
        /// <returns>null - if value not exists</returns>
        string GetConf(string key, bool getDefault = true, bool cache = false);

        /// <summary>
        /// Add/update option in config
        /// (port and slot are ignored - use Port and Slots properties instead)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetConf(string key, string value);

        /// <summary>
        /// Return all server channels
        /// </summary>
        /// <param name="getTemporary">Include temporary channels</param>
        /// <param name="getAcl">Return acls/groups for channels</param>
        /// <param name="getInherited">Include inherited acls/groups</param>
        /// <param name="cache"></param>
        /// <returns>null - if server is not running</returns>
        SerializableDictionary<int, VirtualServerEntity.Channel> GetAllChannels(bool getTemporary = false, bool getAcl = true, bool getInherited = true, bool cache = false);

        /// <summary>
        /// Add new channel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <returns>user id</returns>
        int AddChannel(string name, int parentId);

        /// <summary>
        /// Update exist channel (can be used to move channels)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="setAcl">add acls and groups?</param>
        void UpdateChannelState(VirtualServerEntity.Channel channel, bool setAcl = false);

        /// <summary>
        /// Remove channel
        /// </summary>
        /// <param name="channelId"></param>
        void RemoveChannel(int channelId);

        /// <summary>
        /// Return users who connected to the server now
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        SerializableDictionary<int, VirtualServerEntity.OnlineUser> GetOnlineUsers(bool cache = false);

        /// <summary>
        /// Return only registered users
        /// </summary>
        /// <param name="getInfo">Include user info</param>
        /// <param name="getTexture">Return user texture or not</param>
        /// <param name="filter">TODO: what is filter?</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        SerializableDictionary<int, VirtualServerEntity.User> GetUsers(bool getInfo = true, bool getTexture = false, string filter = null, bool cache = false);

        /// <summary>
        /// Return registered user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="getInfo">Include user info</param>
        /// <param name="getTexture">Return user texture or not</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        VirtualServerEntity.User GetUser(int userId, bool getInfo = true, bool getTexture = false, bool cache = false);

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>user id</returns>
        int RegisterUser(VirtualServerEntity.User user);

        /// <summary>
        /// Remove user registration
        /// </summary>
        /// <param name="userId"></param>
        void UnregisterUser(int userId);


        /// <summary>
        /// Update exist user
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="setAcl">add acls and groups?</param>
        void UpdateUserInfo(VirtualServerEntity.User user);

        /// <summary>
        /// Update online user state (can be used to move/deaf/mute users)
        /// </summary>
        /// <param name="user"></param>
        void UpdateUserState(VirtualServerEntity.OnlineUser user);

        /// <summary>
        /// Kick user from the server
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        void KickUser(int sessionId, string reason);

        SerializableDictionary<int, VirtualServerEntity.Ban> GetBans(bool cache = false);

        /// <summary>
        /// Replace ban list
        /// </summary>
        /// <param name="bans">bans = null will clear ban list</param>
        void SetBans(SerializableDictionary<int, VirtualServerEntity.Ban> bans);



        /// <summary>
        /// Return tree of channels and users
        ///  (for ChannelViewer)
        /// </summary>
        /// <returns></returns>
        VirtualServerEntity.Tree GetTree();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>callback id</returns>
        string AddCallback(IVirtualServerCallbackHandler callback);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session">User session to bind the context menu item</param>
        /// <param name="action">Any action identifier that will be passed to callback event</param>
        /// <param name="title">Menu item title</param>
        /// <param name="callback"></param>
        /// <param name="ctx">context can be bind to many sources, for example: MurmurPlugin.Context.User | MurmurPlugin.Context.Channel</param>
        /// <returns>callback id</returns>
        string AddContextCallback(int session, string action, string title, IVirtualServerContextCallbackHandler callback, Context ctx);

        /// <summary>
        /// Remove server callback by id
        /// </summary>
        /// <param name="id">If id == null then remove all callbacks</param>
        void RemoveCallback(string id = null);
    }
}