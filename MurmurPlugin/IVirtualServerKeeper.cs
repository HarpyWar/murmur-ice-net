namespace MurmurPlugin
{
    public interface IVirtualServerKeeper
    {
        /// <summary>
        /// Save all server settings to object
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>false if something wrong</returns>
        bool Backup(string filename);
        string BackupToXml();

        /// <summary>
        /// Delete all server settings and replace it to the new
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>false if something wrong</returns>
        bool Restore(string filename);
        bool RestoreFromXml(string xml);
        bool Restore(VirtualServerEntity entity);


        string SaveToXml(VirtualServerEntity entity);
        bool SaveToFile(string filename, VirtualServerEntity entity);
        VirtualServerEntity LoadFromXml(string sxml);
        VirtualServerEntity LoadFromFile(string filename);
    }
}