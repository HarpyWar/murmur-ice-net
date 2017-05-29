// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Text;
using MurmurPlugin;
using System.Linq;
using Ice;

namespace Murmur
{
    public class VirtualServer : IVirtualServer, IDisposable
    {
        public IVirtualServerKeeper Keeper
        {
            get
            {
                return _keeper;
            }
        }
        private IVirtualServerKeeper _keeper;

        /// <summary>
        /// Server object with cached data
        /// </summary>
        private VirtualServerEntity _entity;


        private ServerPrx _server;
        private MetaPrx _meta;
        private Instance _instance;

        internal MetaPrx Meta { get { return _meta; } }
        internal ServerPrx Server { get { return _server; } }

        internal VirtualServer(ServerPrx server, Instance instance)
        {
            _instance = instance;
            _meta = _instance.Meta;
            _server = server;

            _entity = new VirtualServerEntity()
            {
                Address = _instance.Address,
                MurmurVersion = _instance.GetVersionString()
            };

            _keeper = new VirtualServerKeeper(this);
        }



        public VirtualServerEntity GetEntity()
        {
            return _entity;
        }


        #region SERVER

        /// <summary>
        /// Flag notation the virtual server is online or not
        /// </summary>
        public bool IsRunning(bool cache = false)
        {
            if (!cache || _isRunning == null)
            {
                _isRunning = _server.isRunning();
            }
            return (bool)_isRunning;
        }
        private bool? _isRunning;

        /// <summary>
        /// Return current users online
        ///  (return null if it can't retrieve online)
        /// </summary>
        public int? GetOnline()
        {
            try
            {
                return _server.getUsers().Count;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Return server uptime
        /// </summary>
        /// <returns></returns>
        public int GetUptime(bool cache = false)
        {
            if (!cache)
            {
                _upTime = _server.getUptime();
            }
            return _upTime;
        }
        private int _upTime;



        public bool Start()
        {
            try
            {
                if (!_server.isRunning())
                {
                    _server.start();
                    SetConf("boot", "true");
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        public bool Stop()
        {
            try
            {
                if (_server.isRunning())
                {
                    _server.stop();
                    SetConf("boot", "false");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Restart()
        {
            if (!Stop() || !Start())
                return false;

            return true;
        }



        /// <summary>
        /// Send text message to user
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="text"></param>
        public void SendMessage(int sessionId, string text)
        {
            _server.sendMessage(sessionId, text);
        }

        /// <summary>
        /// Send text message to channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="text"></param>
        /// <param name="tree">If true, the message will be sent to the channel and all its subchannels.</param>
        public void SendMessageChannel(int channelId, string text, bool tree = true)
        {
            _server.sendMessageChannel(channelId, tree, text);
        }

        /// <summary>
        /// Set password for superuser account
        /// </summary>
        /// <param name="password">if null or empty then random password will be generated</param>
        public void SetSuperuserPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                password = Helper.GetRandomString(8);

            _server.setSuperuserPassword(password);
        }


        /// <summary>
        /// Return log file as string
        /// </summary>
        /// <param name="lines">lines cound from the end of log</param>
        /// <param name="format">format unixtime to readable date?</param>
        /// <returns></returns>
        public string GetLog(int lines, bool format = true)
        {
            var log = _server.getLog(0, lines);

            var logText = new StringBuilder();
            // iterate from the end
            for (int i = log.Length - 1; i >= 0; i--)
            {
                var date = (format)
                    ? _unixTimeStampToDateTime(log[i].timestamp).ToString()
                    : log[i].timestamp.ToString();
                logText.AppendLine(string.Format("[{0}] {1}", date, log[i].txt));
            }

            return logText.ToString();
        }
        private static DateTime _unixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            // convert to local timezone
            dtDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dtDateTime, TimeZoneInfo.Local.Id);
            return dtDateTime.ToLocalTime();
        }

        #endregion


        #region PROPERTIES

        /// <summary>
        /// Server unique identifier
        /// </summary>
        public int Id
        {
            get
            {
                if (_entity.Id == 0)
                    _entity.Id = _server.id();

                return _entity.Id;
            }
        }


        /// <summary>
        /// Server port
        /// </summary>
        public int Port
        {
            get
            {
                var port = GetConf("port", false);
                // if port is empty set to (base port + sever id - 1)
                if (string.IsNullOrEmpty(port))
                    port = (int.Parse(_meta.getDefaultConf()["port"]) + Id - 1).ToString();

                _entity.Port = int.Parse(port);
                return _entity.Port;
            }
            set
            {
                _entity.Port = value;
                SetConf("port", value.ToString());
            }
        }

        /// <summary>
        /// Max number of users
        /// </summary>
        public int Slots
        {
            get { return int.Parse(GetConf("users")); }
            set { SetConf("users", value.ToString()); }
        }

        /// <summary>
        /// The blurb sent by the server on connect
        /// </summary>
        public string WelcomeMessage
        {
            get { return GetConf("welcometext"); }
            set { SetConf("welcometext", value); }
        }

        /// <summary>
        /// Server name (channel root name)
        /// (The data for registration in the public server list)
        /// </summary>
        public string Name
        {
            get { return GetConf("registername"); }
            set { SetConf("registername", value); }
        }

        /// <summary>
        /// Server PW
        /// (The data for registration in the public server list)
        /// </summary>
       	public string Password
        {
            get { return GetConf("password"); }
            set { SetConf("password", value); }
        }

        /// <summary>
        /// Allow html in chat messages/user comments/channel descriptions
        /// </summary>
        public bool AllowHtml
        {
            get { return GetConf("allowhtml") == "true"; }
            set { SetConf("allowhtml", value ? "true" : "false"); }
        }

        /// <summary>
        /// Default channel id where users joined
        ///  (to apply this iption on registered users set RememberChannel = false )
        /// </summary>
        public int DefaultChannel
        {
            get { return int.Parse(GetConf("defaultchannel")); }
            set { SetConf("defaultchannel", value.ToString()); }
        }

        /// <summary>
        /// Bandwidth in kbit/s (max ~130-140)
        /// http://mumble.sourceforge.net/Murmur.ini#bandwidth
        /// </summary>
        public int Bandwidth
        {
            get
            {
                var value = int.Parse(GetConf("bandwidth"))/1000;
                if (value > _maxBandwidth)
                    value = _maxBandwidth;

                return value;
            }
            set
            {
                if (value > _maxBandwidth)
                    value = _maxBandwidth;

                SetConf("bandwidth", (value * 1000).ToString());
            }
        }
        private const int _maxBandwidth = 320; // kbit/sec

        /// <summary>
        /// Client inactivity timeout in ms
        /// </summary>
        public int Timeout
        {
            get { return int.Parse(GetConf("timeout")); }
            set { SetConf("timeout", value.ToString()); }
        }

        /// <summary>
        /// Max users per channel
        /// FIXME: doesn't work 
        /// </summary>
        public int MaxUsersPerChannel
        {
            get
            {
                // FIXME: it always null (why?)
                var value = GetConf("usersperchannel");
                if (value == null)
                    return 0;

                return int.Parse(value);
            }
            set { SetConf("usersperchannel", value.ToString()); }
        }

        /// <summary>
        /// Max message text length in chat
        /// </summary>
        public int MaxTextMessageLength
        {
            get { return int.Parse(GetConf("textmessagelength")); }
            set { SetConf("textmessagelength", value.ToString()); }
        }

        /// <summary>
        /// Regexp on channel name
        /// </summary>
        public string ChannelNameRegex
        {
            get { return GetConf("channelname"); }
            set { SetConf("channelname", value); }
        }

        /// <summary>
        /// Regexp on username
        /// </summary>
        public string UserNameRegex
        {
            get { return GetConf("username"); }
            set { SetConf("username", value); }
        }

        /// <summary>
        /// Should the server remember last channel for registered users?
        /// </summary>
        public bool RememberChannel
        {
            get { return GetConf("rememberchannel") == "true"; }
            set { SetConf("rememberchannel", value ? "true" : "false"); }
        }

        /// <summary>
        /// Mumble supports strong authentication via client certificates, and providing you have an RPC mechanism in place to set them, weak authentication via passwords. 
        /// By default, users without certificates are still allowed on a server if they know the serverpassword, 
        /// or if there isn't one set - however they cannot self-register, even if the server allows it (as there's no certificate to register).
        /// 
        /// Setting CertificateRequired to true ensures that only clients who possess a certificate of some kind are allowed on the server. 
        /// This is mostly safe, as recent Mumble versions will automatically generate a certificate if the certificate wizard is closed prematurely.
        /// (if true then setup Certificate/CertificateKey)
        /// </summary>
        public bool CertificateRequired
        {
            get { return GetConf("certrequired") == "true"; }
            set { SetConf("certrequired", value ? "true" : "false"); }
        }

        /// <summary>
        /// x509 certificate in PEM form
        /// </summary>
        public string Certificate
        {
            get { return GetConf("certificate"); }
            set { SetConf("certificate", value); }
        }
        /// <summary>
        /// x509 private key in PEM form
        /// </summary>
        public string CertificateKey
        {
            get { return GetConf("key"); }
            set { SetConf("key", value); }
        }
		
        /// <summary>
        /// This setting is the DNS hostname where your server can be reached. 
        /// It only needs to be set if you want your server to be addressed in the server list by it's hostname instead of by IP, 
        /// but if it's set it must resolve on the internet or registration will fail.
        /// (The data for registration in the public server list)
        /// 
        /// Example: mumble.some.host
        /// </summary>
        public string RegisterHostname
        {
            get { return GetConf("registerhostname"); }
            set { SetConf("registerhostname", value); }
        }
		/// <summary>
        /// Server site/URL/blog with it will be registered
        /// (The data for registration in the public server list)
        /// 
        /// Example: http://mumble.sourceforge.net
        /// </summary>
        public string RegisterUrl
        {
            get { return GetConf("registerurl"); }
            set { SetConf("registerurl", value); }
        }
		/// <summary>
        /// Plain-text secret between your server and the registration server. 
        /// It's sole purpose is to prevent other servers from impersonating your server in the public server list.
        /// Set this setting empty to disable registration with the public server list.
        ///  (The data for registration in the public server list)
        /// </summary>
        public string RegisterPassword
        {
            get { return GetConf("registerpassword"); }
            set { SetConf("registerpassword", value); }
        }

        #endregion




        #region CONFIG

        /// <summary>
        /// Flag to know if config was already loaded from remote or not
        /// </summary>
        private bool _isNewConfig = true;

        /// <summary>
        /// Return all config options
        /// </summary>
        /// <param name="getDefault">Include default config values if they not exists in the server config</param>
        /// <param name="cache"></param>
        /// <returns>Dictionary enumeration of config key-value</returns>
        public SerializableDictionary<string, string> GetAllConf(bool getDefault, bool cache = false)
        {
            // update cache
            if (!cache || _isNewConfig)
            {
                _isNewConfig = false;

                // clear
                _entity.Config.Clear();

                // add
                foreach (var c in _server.getAllConf())
                {
                    if (c.Key == "port")
                        _entity.Port = int.Parse(c.Value);

                    _entity.Config.Add(c.Key, c.Value);
                }

                if (getDefault)
                {
                    // iterate default config
                    foreach (var c in _instance.GetDefaultConf())
                    {
                        // if option not exists in server config then add it
                        if (!_entity.Config.ContainsKey(c.Key))
                            _entity.Config.Add(c.Key, c.Value);
                    }
                }

            }
            return _entity.Config;
        }

        /// <summary>
        /// Get value from config
        /// </summary>
        /// <param name="key">option identifier (dictionary key)</param>
        /// <param name="getDefault">Return default config values if they not exists in the server config</param>
        /// <param name="cache"></param>
        /// <returns>null - if value not exists</returns>
        public string GetConf(string key, bool getDefault = true, bool cache = false)
        {
            if (!cache || !_entity.Config.ContainsKey(key))
            {
                // get from remote
                var value = _server.getConf(key);
                if (string.IsNullOrEmpty(value))
                {
                    // return default config value if exist
                    if (getDefault)
                        if (_instance.GetDefaultConf().ContainsKey(key))
                            value = _instance.GetDefaultConf()[key];

                    if (string.IsNullOrEmpty(value))
                        return null;
                }


                // update cache
                if (_entity.Config.ContainsKey(key))
                    _entity.Config[key] = value;
                else
                    _entity.Config.Add(key, value);
            }

            if (_entity.Config.ContainsKey(key))
                return _entity.Config[key];

            return null;
        }

        /// <summary>
        /// Add/update option in config
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetConf(string key, string value)
        {
            // update remote
            _server.setConf(key, value);

            // update cache
            if (_entity.Config.ContainsKey(key))
                _entity.Config[key] = value;
            else
                _entity.Config.Add(key, value);
        }

        #endregion



        #region CHANNEL


        /// <summary>
        /// Flag to know if channels was already loaded from remote or not
        /// </summary>
        private bool _isNewChannels = true;

        /// <summary>
        /// Return all server channels
        /// </summary>
        /// <param name="getTemporary">Include temporary channels</param>
        /// <param name="getAcl">Return acls/groups for channels</param>
        /// <param name="getInherited">Include inherited acls/groups</param>
        /// <param name="cache"></param>
        /// <returns>null - if server is not running</returns>
        public SerializableDictionary<int, VirtualServerEntity.Channel> GetAllChannels(bool getTemporary = false, bool getAcl = true, bool getInherited = true, bool cache = false)
        {
            // server must be running (getChannels() doesn's work)
            if (!IsRunning())
                return null;

            // update cache
            if (!cache || _isNewChannels)
            {
                _isNewChannels = false;

                // clear
                _entity.Channels.Clear();

                // add
                foreach (var c in _server.getChannels())
                {
                    // ignore temporary channels
                    if (c.Value.temporary && !getTemporary)
                        continue;


                    var channel = getChannel(c.Value);

                    Group[] groups;
                    ACL[] acls;
                    bool inherit;
                    if (getAcl)
                    {
                        _server.getACL(c.Value.id, out acls, out groups, out inherit);

                        channel.InheritAcl = inherit;

                        // -- GROUPS
                        int cg = 0;
                        foreach (var @g in groups)
                        {
                            // ignore inherited groups
                            if (!getInherited && @g.inherited)
                                continue;

                            channel.Groups.Add(cg, new VirtualServerEntity.Channel.Group()
                            {
                                Name = @g.name,
                                Inherit = @g.inherit,
                                Inheritable = @g.inheritable,
                                Inherited = @g.inherited,

                                Members = @g.members,
                                Add = @g.add,
                                Remove = @g.remove,
                            });
                            cg++;
                        }

                        // -- ACLS
                        int ca = 0;
                        foreach (var @a in acls)
                        {
                            // ignore inherited acls
                            if (!getInherited && @a.inherited)
                                continue;

                            channel.Acls.Add(ca, new VirtualServerEntity.Channel.Acl()
                            {
                                Allow = @a.allow,
                                ApplyHere = @a.applyHere,
                                ApplySubs = @a.applySubs,
                                Deny = @a.deny,
                                Inherited = @a.inherited,
                                Group = @a.group,
                                UserId = @a.userid,
                            });
                            ca++;
                        }
                    }


                    _entity.Channels.Add(c.Value.id, channel);
                }
            }
            return _entity.Channels;
        }

        /// <summary>
        /// Add new channel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <returns>user id</returns>
        public int AddChannel(string name, int parentId)
        {
            int cid = 0;

            // only one root channel is available (it has parent id = -1 and can't be removed or added)
            if (parentId > -1)
            {
                // add channel
                cid = _server.addChannel(name, parentId);
            }

            // add in cache if not exist
            if (!_entity.Channels.ContainsKey(cid))
                _entity.Channels.Add(cid, new VirtualServerEntity.Channel()
                {
                    Id = cid,
                    Name = name,
                    ParentId = parentId
                });

            return cid;
        }

        /// <summary>
        /// Update exist channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="setAcl">add acls and groups?</param>
        public void UpdateChannel(VirtualServerEntity.Channel channel, bool setAcl = false)
        {
            // set channel info
            var state = new Channel(channel.Id, channel.Name, channel.ParentId, channel.Links, channel.Description, false, channel.Position);
            // WARNING! LINKS MUST BE VALID OR InvalidChannelException will be thrown
            _server.setChannelState(state);

            if (setAcl)
            {
                // set groups
                Group[] groups = new Group[channel.Groups.Count];
                for (int i = 0; i < channel.Groups.Count; i++)
                {
                    var g = channel.Groups[i];
                    groups[i] = new Group(g.Name, g.Inherited, g.Inherit, g.Inheritable, g.Add, g.Remove, g.Members);
                }
                // set acls
                ACL[] acls = new ACL[channel.Acls.Count];
                for (int i = 0; i < channel.Acls.Count; i++)
                {
                    var a = channel.Acls[i];
                    acls[i] = new ACL(a.ApplyHere, a.ApplySubs, a.Inherited, a.UserId, a.Group, a.Allow, a.Deny);
                }
                _server.setACL(channel.Id, acls, groups, channel.InheritAcl);
            }

            // update in cache
            if (_entity.Channels.ContainsKey(channel.Id))
                _entity.Channels[channel.Id] = channel;
            else
                _entity.Channels.Add(channel.Id, channel);
        }

        /// <summary>
        /// Remove channel
        /// </summary>
        /// <param name="channelId"></param>
        public void RemoveChannel(int channelId)
        {
            // remove remote
            _server.removeChannel(channelId);

            // remove from cache
            if (_entity.Channels.ContainsKey(channelId))
                _entity.Channels.Remove(channelId);
        }


        #endregion


        #region USERS

        /// <summary>
        /// Flag to know if users was already loaded from remote or not
        /// </summary>
        private bool _isNewUsers = true;
        private bool _isNewOnlineUsers = true;

        /// <summary>
        /// Return users who connected to the server now
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public SerializableDictionary<int, VirtualServerEntity.OnlineUser> GetOnlineUsers(bool cache = false)
        {
            // update cache
            if (!cache || _isNewOnlineUsers)
            {
                _isNewOnlineUsers = false;

                // clear
                _entity.OnlineUsers.Clear();

                // non-registered users have id = -1
                // so we use simple increment as id
                int i = 0;

                // add
                foreach (var u in _server.getUsers())
                {
                    var user = getOnlineUser(u.Value);
                    _entity.OnlineUsers.Add(i, user);
                    i++;
                }
            }
            return _entity.OnlineUsers;
        }

        /// <summary>
        /// Create online user object from Ice user object
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        internal static VirtualServerEntity.OnlineUser getOnlineUser(User u)
        {
            var user = new VirtualServerEntity.OnlineUser()
            {
                Id = u.userid,
                ChannelId = u.channel,
                Name = u.name,
                Address = u.address,
                BytesPerSec = u.bytespersec,
                Comment = u.comment,
                Context = u.context,
                Deaf = u.deaf,
                Identity = u.identity,
                Idlesecs = u.idlesecs,
                Mute = u.mute,
                OnlineSecs = u.onlinesecs,
                Os = u.os,
                OsVersion = u.osversion,
#if MURMUR_123
                PrioritySpeaker = u.prioritySpeaker,
                Recording = u.recording,
#endif
                Release = u.release,
                SelfDeaf = u.selfDeaf,
                SelfMute = u.selfMute,
                Session = u.session,
                Supress = u.suppress,
                TcpOnly = u.tcponly,
#if MURMUR_124
                TcpPing = u.tcpPing,
                UdpPing = u.udpPing,
#endif
                Version = u.version,
            };

            return user;
        }

        /// <summary>
        /// Create channel object from Ice channel object
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        internal static VirtualServerEntity.Channel getChannel(Channel c)
        {
            var channel = new VirtualServerEntity.Channel()
            {
                Id = c.id,
                Name = c.name,
                Description = c.description,
                ParentId = c.parent,
                Links = c.links,
                Position = c.position,
                Temporary = c.temporary,
            };
            return channel;
        }



        /// <summary>
        /// Return only registered users
        /// </summary>
        /// <param name="getInfo">Include user info</param>
        /// <param name="getTexture">Return user texture or not</param>
        /// <param name="filter">TODO: what is filter?</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public SerializableDictionary<int, VirtualServerEntity.User> GetUsers(bool getInfo = true, bool getTexture = false, string filter = null, bool cache = false)
        {
            // update cache
            if (!cache || _isNewUsers)
            {
                _isNewUsers = false;

                // clear
                _entity.Users.Clear();

                // add
                foreach (var u in _server.getRegisteredUsers(filter))
                {
                    // insert to _entity.Users is in GetUser method
                    GetUser(u.Key, getInfo, getTexture, cache); // cache = always false
                }
            }
            return _entity.Users;
        }

        /// <summary>
        /// Return registered user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="getInfo">Include user info</param>
        /// <param name="getTexture">Return user texture or not</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public VirtualServerEntity.User GetUser(int userId, bool getInfo = true, bool getTexture = false, bool cache = false)
        {
            if (!cache || !_entity.Users.ContainsKey(userId))
            {
                // create object
                var user = new VirtualServerEntity.User()
                {
                    Id = userId,
                };
                if (getTexture)
                    user.Texture = _server.getTexture(userId);


                if (getInfo)
                {
                    // get from remote
                    var userInfo = _server.getRegistration(userId);
                    if (userInfo == null)
                        return null;

                    foreach (var @i in userInfo)
                    {
                        user.Info.Add((VirtualServerEntity.User.UserInfo)@i.Key, @i.Value);
                    }
                }

                // update in cache
                if (_entity.Users.ContainsKey(userId))
                    _entity.Users[userId] = user;
                else
                    _entity.Users.Add(userId, user);
            }

            return _entity.Users[userId];
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>user id</returns>
        public int RegisterUser(VirtualServerEntity.User user)
        {
            // create object
            var newUser = new Dictionary<UserInfo, string>();
            foreach (var ui in user.Info)
            {
                newUser.Add((UserInfo)ui.Key, ui.Value);
            }

            // register user
            var id = _server.registerUser(newUser);
            // update id
            user.Id = id;

            // set user texture
            if (user.Texture.Length > 0)
                _server.setTexture(id, user.Texture);

            // add in cache
            _entity.Users.Add(id, user);

            return id;
        }

        /// <summary>
        /// Remove user registration
        /// </summary>
        /// <param name="userId"></param>
        public void UnregisterUser(int userId)
        {
            _server.unregisterUser(userId);

            // remove from cache
            if (_entity.Users.ContainsKey(userId))
                _entity.Users.Remove(userId);
        }


        /// <summary>
        /// Update exist user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="user">with updated info</param>
        public void UpdateUserInfo(VirtualServerEntity.User user)
        {
            var newInfo = new Dictionary<UserInfo, string>();
            foreach(var it in user.Info)
            {
                newInfo.Add((Murmur.UserInfo)it.Key, it.Value);
            }

            // set user info
            _server.updateRegistration(user.Id, newInfo);

            // update in cache
            if (_entity.Users.ContainsKey(user.Id))
                _entity.Users[user.Id] = user;
            else
                _entity.Users.Add(user.Id, user);
        }



        /// <summary>
        /// Kick user from the server
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public void KickUser(int sessionId, string reason)
        {
            _server.kickUser(sessionId, reason);

            // remove from cache
            foreach (var u in _entity.OnlineUsers)
            {
                if (u.Value.Session == sessionId)
                {
                    _entity.OnlineUsers.Remove(u.Key);
                    break;
                }
            }
        }

        #endregion


        #region BANS

        /// <summary>
        /// Flag to know if users was already loaded from remote or not
        /// </summary>
        private bool _isNewBans = true;

        public SerializableDictionary<int, VirtualServerEntity.Ban> GetBans(bool cache = false)
        {
            if (!cache || _isNewBans)
            {
                _isNewBans = false;

                // clear
                _entity.Bans.Clear();

                // add
                int cb = 0;
                foreach (var b in _server.getBans())
                {
                    _entity.Bans.Add(cb, new VirtualServerEntity.Ban()
                    {
                        Address = b.address,
                        Bits = b.bits,
                        Duration = b.duration,
                        Hash = b.hash,
                        Name = b.name,
                        Reason = b.reason,
                        Start = (int)b.start // cast long->int needed to compatible with v1.2.2 
                    });
                    cb++;
                }
            }
            return _entity.Bans;
        }

        /// <summary>
        /// Replace ban list
        /// </summary>
        /// <param name="bans">bans = null will clear ban list</param>
        public void SetBans(SerializableDictionary<int, VirtualServerEntity.Ban> bans)
        {
            // clear banlist
            if (bans == null)
            {
                // clear remote
                _server.setBans(null);

                // clear cache
                _entity.Bans.Clear();

                return;
            }

            // update remote
            Ban[] _bans = new Ban[bans.Count];
            for (int i = 0; i < bans.Count; i++)
            {
                var b = bans[i];
                _bans[i] = new Ban(b.Address, b.Bits, b.Name, b.Hash, b.Reason, b.Start, b.Duration);
            }
            _server.setBans(_bans);

            // clear cache
            _entity.Bans.Clear();

            // update cache
            _entity.Bans = bans;
        }

        #endregion


        /// <summary>
        /// Return tree of channels and users
        ///  (for ChannelViewer)
        /// </summary>
        /// <returns></returns>
        public VirtualServerEntity.Tree GetTree()
        {
            return _getTreeItem(_server.getTree());
        }

        private VirtualServerEntity.Tree[] _getTree(Tree[] tree)
        {
            if (tree == null)
                return null;

            var obj = new VirtualServerEntity.Tree[tree.Length];
            for (int i = 0; i < tree.Length; i++)
            {
                obj[i] = _getTreeItem(tree[i]);
            }
            return obj;
        }
        private VirtualServerEntity.Tree _getTreeItem(Tree tree)
        {
            if (tree == null)
                return null;

            var obj = new VirtualServerEntity.Tree()
            {
                Channel = getChannel(tree.c),
                Children = _getTree(tree.children)
            };

            // fill users
            if (tree.users.Length > 0)
            {
                obj.Users = new VirtualServerEntity.OnlineUser[tree.users.Length];
                for (int i = 0; i < tree.users.Length; i++)
                {
                    obj.Users[i] = getOnlineUser(tree.users[i]);
                }
            }

            return obj;
        }


        #region CALLBACKS

        private Dictionary<string, object> callbacks = new Dictionary<string, object>();

        public string AddCallback(IVirtualServerCallbackHandler callback)
        {
            var cb = _instance.AddVirtualServerCallback(callback, this);
            callbacks.Add(cb.Key, cb.Value);
            return cb.Key;
        }
        public string AddContextCallback(int session, string action, string title, IVirtualServerContextCallbackHandler callback, Context ctx)
        {
            var cb = _instance.AddVirtualServerContextCallback(session, action, title, callback, (int)ctx, this);
            callbacks.Add(cb.Key, cb.Value);
            return cb.Key;
        }

        /// <summary>
        /// Remove callback by id
        /// </summary>
        /// <param name="id">If id == null then remove all callbacks</param>
        public void RemoveCallback(string id = null)
        {
            foreach(var c in callbacks)
            {
                if (id != null && id != c.Key)
                {
                    continue;
                }
                if (c.Value is ServerContextCallbackPrx)
                {
                    Server.removeContextCallback((ServerContextCallbackPrx)c.Value);
                }
                if (c.Value is ServerCallbackPrx)
                {
                    Server.removeCallback((ServerCallbackPrx)c.Value);
                }
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