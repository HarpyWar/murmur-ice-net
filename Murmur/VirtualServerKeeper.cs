// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using MurmurPlugin;

namespace Murmur
{
    /// <summary>
    /// Backup and restщre virtual server to XML
    /// </summary>
    public class VirtualServerKeeper : IVirtualServerKeeper
    {
        private IVirtualServer server;

        public VirtualServerKeeper(IVirtualServer server)
        {
            this.server = server;
        }

        /// <summary>
        /// Save all server settings to object
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>false if something wrong</returns>
        public bool Backup(string filename)
        {
            var entity = _backupEntity();
            if (entity != null)
                return SaveToFile(filename, entity);

            return false;
        }
        public string BackupToXml()
        {
            var entity = _backupEntity();
            if (entity != null)
                return SaveToXml(entity);

            return string.Empty;
        }
        private VirtualServerEntity _backupEntity()
        {
            // server must be running (getChannels() doesn's work)
            if (!server.IsRunning())
                return null;

            try
            {
                // fetch id
                var id = server.Id;

                // -- CONFIG
                // do not include default conf values
                server.GetAllConf(false, false);

                // -- USERS
                // registered
                server.GetUsers(true, true, null, false);

                // -- CHANNELS
                // not temporary and not inherited acls
                server.GetAllChannels(false, true, false, false);

                // -- BANS
                server.GetBans(false);
            }
            catch
            {
                // TODO: backup log?
                return null;
            }
            return server.GetEntity();
        }


        /// <summary>
        /// Delete all server settings and replace it to the new
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>false if something wrong</returns>
        public bool Restore(string filename)
        {
            var entity = LoadFromFile(filename);
            if (entity != null)
                return _restoreEntity(entity);
           
            return false;
        }
        public bool RestoreFromXml(string xml)
        {
            var entity = LoadFromXml(xml);
            if (entity != null)
                return _restoreEntity(entity);

            return false;
        }
        public bool Restore(VirtualServerEntity entity)
        {
            if (entity != null)
                return _restoreEntity(entity);

            return false;
        }

        private bool _restoreEntity(VirtualServerEntity entity)
        {
            // server must be running
            if (!server.IsRunning())
                return false;

            try
            {
                // -- CONFIG
                // 1) clear remote
                foreach (var c in server.GetAllConf(false, false).Clone())
                {
                    // ignore port and slots
                    if (c.Key == "port" || c.Key == "users")
                        continue;

                    server.SetConf(c.Key, null);
                }
                // 2) setup
                foreach (var c in entity.Config)
                {
                    // ignore port and slots
                    if (c.Key == "port" || c.Key == "users")
                        continue;

                    server.SetConf(c.Key, c.Value);
                }


                // -- USERS (before channels, because acls of channels has old user IDs)
                // 1) clear remote
                foreach (var u in server.GetUsers(false, false, null, false).Clone())
                {
                    // ignore SuperUser (it can't be removed)
                    if (u.Key == 0)
                        continue;

                    server.UnregisterUser(u.Key);

#if DEBUG
                    Console.WriteLine("[{0}][clear] user #{1}", server.Id, u.Key);
#endif
                }
                var newUserIds = new Dictionary<int, int>(); // newly created user IDs list (they have another IDs than backuped)
                // 2) setup
                foreach (var u in entity.Users)
                {
                    // do not create SuperUser
                    if (u.Value.Id == 0)
                        continue;

                    // remember old id, because after add user it will be updated to the new
                    var old_uid = u.Value.Id;

                    try
                    {
                        // register new user and update object Id
                        u.Value.Id = server.RegisterUser(u.Value);

                        // map oldid <-> newid
                        newUserIds.Add(old_uid, u.Value.Id);
#if DEBUG
                        Console.WriteLine("[{0}][add] user #{1}", server.Id, u.Key);
#endif
                    }
                    catch
                    {
                        // UNDONE: user names with special characters (like ★) throw exception
#if DEBUG
                        Console.WriteLine("[ERROR] bad username " + u.Value.Info[VirtualServerEntity.User.UserInfo.UserName].ToString());
#endif
                    }
                }


                // -- CHANNELS
                // 1) clear remote
                foreach (var c in server.GetAllChannels(true, false, false).Clone())
                {
                    // ignore root channel (it can't be removed)
                    if (c.Value.Id == 0)
                        continue;

                    // if parentid < id then channel already removed (cascade remove)
                    if (c.Value.ParentId > 0 && c.Value.ParentId < c.Value.Id)
                        continue;

                    server.RemoveChannel(c.Value.Id);
#if DEBUG
                    Console.WriteLine("[{0}][clear] channel #{1}", server.Id, c.Key);
#endif
                }
                var newChannelIds = new Dictionary<int, int>(); // newly created channel IDs list (they have another IDs than backuped)
                // 2) create channels
                foreach (var c in entity.Channels)
                {
                    // do not create root channel
                    if (c.Value.Id != 0)
                    {
                        // remember old id, because after add channel it will be updated to the new
                        var old_cid = c.Value.Id;
                        // create channel (name and parentid=root) and update object Id
                        c.Value.Id = server.AddChannel(c.Value.Name, 0);
 
                        // map oldid <-> newid
                        newChannelIds.Add(old_cid, c.Value.Id); 
                    }

                    // update new user IDs for acls
                    foreach (var acl in c.Value.Acls)
                    {
                        // if user found in newly created users then replace user id to the new
                        // (ignore superuser with id = 0)
                        if (acl.Value.UserId > 0 && newUserIds.ContainsKey(acl.Value.UserId))
                            c.Value.Acls[acl.Key].UserId = newUserIds[acl.Value.UserId];
                    }

                    // update new user IDs for group members, add and remove list of users
                    foreach (var g in c.Value.Groups)
                    {
                        for (int i = 0; i < g.Value.Members.Length; i++)
                        {
                            var userid = g.Value.Members[i];
                            // if user found in newly created users then replace user id to the new
                            // (ignore superuser with id = 0)
                            if (userid > 0 && newUserIds.ContainsKey(userid))
                                c.Value.Groups[g.Key].Members[i] = newUserIds[userid];
                        }
                        for (int i = 0; i < g.Value.Remove.Length; i++)
                        {
                            var userid = g.Value.Remove[i];
                            if (userid > 0 && newUserIds.ContainsKey(userid))
                                c.Value.Groups[g.Key].Remove[i] = newUserIds[userid];
                        }
                        for (int i = 0; i < g.Value.Add.Length; i++)
                        {
                            var userid = g.Value.Add[i];
                            if (userid > 0 && newUserIds.ContainsKey(userid))
                                c.Value.Groups[g.Key].Add[i] = newUserIds[userid];
                        }
                    }
#if DEBUG
                    Console.WriteLine("[{0}][add] channel #{1}", server.Id, c.Key);
#endif
                }
                // 3) update channels (parentId and links are reference to exist channels now, because they are created in prev step)
                foreach (var c in entity.Channels)
                {
                    // if parent channel is found in new channel IDs (was created earlier) then replace it to the new id
                    // (ignore root channel with id = 0)
                    if (c.Value.ParentId > 0 && newChannelIds.ContainsKey(c.Value.ParentId))
                        c.Value.ParentId = newChannelIds[c.Value.ParentId];

                    // relink with new channel ids
                    for (int i = 0; i < c.Value.Links.Length; i++)
                    {
                        if (newChannelIds.ContainsKey(c.Value.Links[i]))
                            c.Value.Links[i] = newChannelIds[c.Value.Links[i]];
                    }
#if DEBUG
                    Console.WriteLine("[{0}][updating] channel #{1} {2}", server.Id, c.Key, c.Value.Name);
#endif
                    server.UpdateChannel(c.Value, true);
                }

                // -- BANS
                // 1) clear
                server.SetBans(null);
                // 2) setup
                server.SetBans(entity.Bans);

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }



        public VirtualServerEntity LoadFromFile(string filename)
        {
            try
            {
                var sxml = File.ReadAllText(filename);
                return LoadFromXml(sxml);
            }
            catch
            {
                return null;
            }
        }

        public VirtualServerEntity LoadFromXml(string sxml)
        {
            if (string.IsNullOrEmpty(sxml))
                return null;

            var entity = new VirtualServerEntity();

            try
            {
                var reader = new StringReader(sxml);
                var serializer = new XmlSerializer(entity.GetType());
                entity = (VirtualServerEntity)serializer.Deserialize(reader);

                return entity;
            }
            catch
            {
                return null;
            }
        }

        public bool SaveToFile(string filename, VirtualServerEntity entity)
        {
            try
            {
                File.WriteAllText(filename, SaveToXml(entity));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string SaveToXml(VirtualServerEntity entity)
        {
            var serializer = new XmlSerializer(entity.GetType());
            var strb = new StringBuilder();
            var strw = new StringWriter(strb, System.Globalization.CultureInfo.InvariantCulture);
            serializer.Serialize(strw, entity);
            string sxml = strb.ToString();

            return sxml;
        }
    }
}