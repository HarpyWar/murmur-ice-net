using System.ComponentModel;

namespace MurmurPlugin
{
    public class MurmurVersion
    {
        /// <summary>
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
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
        }
    }
}