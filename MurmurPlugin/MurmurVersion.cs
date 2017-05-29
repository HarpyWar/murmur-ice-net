using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace MurmurPlugin
{
    public class MurmurVersion
    {
        /// <summary>
        /// Murmur Version
        /// </summary>
        /// <param name="version">1.2.3.380</param>
        public MurmurVersion(string version)
        {
            var parts = version.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        int.TryParse(parts[i], out _major);
                        break;
                    case 1:
                        int.TryParse(parts[i], out _minor);
                        break;
                    case 2:
                        int.TryParse(parts[i], out _build);
                        break;
                    case 3:
                        int.TryParse(parts[i], out _revision);
                        break;
                }
            }
        }

        public MurmurVersion()
        {
        }

        public int Major
        {
            get
            {
                return _major;
            }
        }
        private int _major;

        public int Minor
        {
            get
            {
                return _minor;
            }
        }
        private int _minor;

        public int Build
        {
            get
            {
                return _build;
            }
        }
        private int _build;

        public int Revision
        {
            get
            {
                return _revision;
            }
        }
        private int _revision;


        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}{3}", 
                Major, Minor, Build, 
                Revision > 0 ? "." + Revision : ""); // add revision only if >0
        }





        /// <summary>
        /// Return version by string in avaiable versions
        /// </summary>
        /// <param name="versionString">1.2.3.4</param>
        /// <returns></returns>
        public static MurmurVersion FindMurmurVersion(string versionString)
        {
            return GetMurmurVersionList().Where(_v => _v.ToString() == versionString).FirstOrDefault();
        }

        /// <summary>
        /// Return list of available Murmur versions inside a directory with the program
        /// It looks for Murmur_*.dll in a working directory
        /// (value cached after first get)
        /// </summary>
        /// <returns></returns>
        public static MurmurVersion[] GetMurmurVersionList()
        {
            // lazy initialization
            if (_versions != null)
            {
                return _versions;
            }

            var versions = new List<MurmurVersion>();
            var searchPattern = _fileStarts + "*" + _fileEnds;
            foreach (var f in Directory.GetFiles(Directory.GetCurrentDirectory(), searchPattern))
            {
                var version = MurmurVersion.GetVersionFromFileName(f);
                if (version != null)
                {
                    versions.Add(new MurmurVersion(version));
                }
            }
            _versions = versions.ToArray();
            return _versions;
        }
        private static MurmurVersion[] _versions;



        static string _fileStarts = "Murmur_";
        static string _fileEnds = ".dll";

        /// <summary>
        /// Murmur_1.2.3.dll -> 1.2.3
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>null if filename is not valid</returns>
        public static string GetVersionFromFileName(string fileName)
        {
            var f = Path.GetFileNameWithoutExtension(fileName).ToLower();
            if (f.StartsWith(_fileStarts.ToLower()))
            {
                var _version = f.Substring(_fileStarts.ToLower().Length, f.Length - _fileStarts.Length);
                //string fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(fileName).FileVersion;
                return _version;
            }
            return null;
        }
        /// <summary>
        /// 1.2.3 -> Murmur_1.2.3.dll
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        public static string GetFileNameFromVersion(string versionString)
        {
            return string.Format("{0}{1}{2}", _fileStarts, versionString, _fileEnds);
        }
    }
}