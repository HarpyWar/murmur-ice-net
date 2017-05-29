// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using MurmurPlugin;
using System.IO;

namespace MurmurPlugin
{
    public static class Helper
    {


        public static string GetRandomString(int size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);//thanks to McAden

            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString().ToLower();
        }



        /// <summary>
        /// Check server port for open
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="timeout"> </param>
        /// <returns></returns>
        public static bool IsPortOpened(string address, int port, int timeout = 3)
        {

            var ip = System.Net.Dns.GetHostAddresses(address)[0];

            AutoResetEvent connectDone = new AutoResetEvent(false);
            TcpClient client = new TcpClient();


            // connect with timeout 
            //  http://stackoverflow.com/questions/795574/c-sharp-how-do-i-stop-a-tcpclient-connect-process-when-im-ready-for-the-progr
            try
            {
                using (TcpClient tcp = new TcpClient())
                {
                    IAsyncResult ar = tcp.BeginConnect(ip, port, null, null);
                    System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
                    try
                    {
                        if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeout), false))
                        {
                            tcp.Close();
                            throw new TimeoutException();
                        }
                        tcp.EndConnect(ar);
                    }
                    finally
                    {
                        wh.Close();
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;

        }




        /// <summary>
        /// Return default attribute value
        ///  (used to get enum default value)
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TEnum GetDefaultValue<TEnum>() where TEnum : struct
        {
            Type t = typeof(TEnum);
            DefaultValueAttribute[] attributes = (DefaultValueAttribute[])t.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attributes != null &&
                attributes.Length > 0)
            {
                return (TEnum)attributes[0].Value;
            }
            else
            {
                return (TEnum)Enum.GetValues(typeof(TEnum)).GetValue(0);
            }
        }
    }
}
