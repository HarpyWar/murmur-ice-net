// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Xml.Serialization;

namespace MurmurPlugin
{
    /// <summary>
    /// Murmur Virtual Server inner structure
    /// </summary>
    [Serializable]
    public class VirtualServerEntity
    {
        public int Id;
        public string Address;
        public int Port;
        public string MurmurVersion;

        public SerializableDictionary<string, string> Config = new SerializableDictionary<string, string>();
        public SerializableDictionary<int, User> Users = new SerializableDictionary<int, User>();
        public SerializableDictionary<int, Channel> Channels = new SerializableDictionary<int, Channel>();
        public SerializableDictionary<int, Ban> Bans = new SerializableDictionary<int, Ban>();

        [XmlIgnore]
        public SerializableDictionary<int, OnlineUser> OnlineUsers = new SerializableDictionary<int, OnlineUser>();


        /// <summary>
        /// Do not use parameters - serialization needs constructor without parameters
        /// </summary>
        public VirtualServerEntity()
        {
        }

        public class User
        {
            public int Id;
            public byte[] Texture;

            public SerializableDictionary<UserInfo, string> Info = new SerializableDictionary<UserInfo, string>();
            public enum UserInfo
            {
                UserName,
                UserEmail,
                UserComment,
                UserHash,
                UserPassword,
                UserLastActive
            }
        }

        public class OnlineUser
        {
            public int Id;
            public int ChannelId;
            public string Name;
            public byte[] Address;
            public int BytesPerSec;
            public string Comment;
            public string Context;
            public bool Deaf;
            public string Identity;
            public int Idlesecs;
            public bool Mute;
            public int OnlineSecs;
            public string Os;
            public string OsVersion;
            public bool PrioritySpeaker;
            public bool Recording;
            public string Release;
            public bool SelfDeaf;
            public bool SelfMute;
            public int Session;
            public bool Supress;
            public float TcpPing;
            public bool TcpOnly;
            public float UdpPing;
            public int Version;

            public void Move(IVirtualServer server, int newChannelId)
            {
                this.ChannelId = newChannelId;
                server.UpdateUserState(this);
            }
            public void SetMute(IVirtualServer server, bool mute)
            {
                this.Mute = mute;
                server.UpdateUserState(this);
            }
            public void SetDeafen(IVirtualServer server, bool deaf)
            {
                this.Deaf = deaf;
                server.UpdateUserState(this);
            }

        }


        public class Channel
        {
            public int Id;
            public string Name;
            public int ParentId;
            public bool InheritAcl = true;

            public string Description;
            public int Position;
            public bool Temporary = false;

            public int[] Links;

            public SerializableDictionary<int, Group> Groups = new SerializableDictionary<int, Group>();
            public SerializableDictionary<int, Acl> Acls = new SerializableDictionary<int, Acl>();

            public class Group
            {
                public string Name;
                public bool Inherit;
                public bool Inheritable;
                public bool Inherited;

                public int[] Members;
                public int[] Add;
                public int[] Remove;
            }

            public class Acl
            {
                public int Allow;
                public bool ApplySubs;
                public bool ApplyHere;
                public int Deny;
                public string Group;
                public bool Inherited;
                public int UserId;
            }

            public void Move(IVirtualServer server, int newParentId)
            {
                this.ParentId = newParentId;
                server.UpdateChannelState(this);
            }
            public void SetName(IVirtualServer server, string newName)
            {
                this.Name = newName;
                server.UpdateChannelState(this);
            }
        }

        public class Tree
        {
            public Channel Channel;
            public Tree[] Children;
            public OnlineUser[] Users;
        }

        public class Ban
        {
            public byte[] Address;
            public int Bits;
            public int Duration;
            public string Hash;
            public string Name;
            public string Reason;
            public int Start;
        }

        public struct LogEntry
        {
            public int Timestamp;
            public string Text;
        }
    }
}
