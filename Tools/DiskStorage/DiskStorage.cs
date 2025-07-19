using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Eitrix
{
    public delegate void DiskSerializer(FileStream stream);

    /// -----------------------------------------------------------------------
    /// <summary>
    /// Generic class for reading and writing from disk.  This works on both
    /// PC and XBox.  
    /// 
    /// Call Initialize() before trying to use this.
    /// </summary>
    /// -----------------------------------------------------------------------
    public static class DiskStorage
    {
        public static string StorageName = "GenericStorage";
        static DateTime lastStorageRequest = DateTime.MinValue;

        private static bool available = false;
        public static bool Available { get { return available; } }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Initialize storage for the game.  Storage name is a unique identifier
        /// for this program.  
        /// </summary>
        /// -----------------------------------------------------------------------
        public static void Initialize(string storageName)
        {
            StorageName = storageName;
            if (!Available) StartStorageRequest();
        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Load Something from disk.  Write your own deserializer to read the bits.
        /// </summary>
        /// -----------------------------------------------------------------------
        public static void ReadFromFile(string fileName, DiskSerializer serializer)
        {
            if (!available) throw new ApplicationException("Storage is not available.  Did you call initialize?");
            StorageContainer container = null;
            FileStream stream = null;
            try
            {
                string fullpath = GetLocalPath(fileName, ref container);
                if (!File.Exists(fullpath)) throw new FileNotFoundException("Can't find '" + fileName + "'");

                stream = File.Open(fullpath, FileMode.Open, FileAccess.Read);
                if (stream.Length > 0)
                {
                    serializer(stream);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                if (stream != null) stream.Close();
                if (container != null) container.Dispose();
            }
        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Save Something to a file.  Write your own serializer to store the bits
        /// </summary>
        /// -----------------------------------------------------------------------
        public static void SaveToFile(string fileName, DiskSerializer serializer)
        {
            if (!available) throw new ApplicationException("Storage is not available.  Did you call initialize?");
            StorageContainer container = null;
            FileStream stream = null;
            try
            {
                string fullpath = GetLocalPath(fileName, ref container);
                if (File.Exists(fullpath)) File.Delete(fullpath);
                stream = File.Open(fullpath, FileMode.OpenOrCreate);
                serializer(stream);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            finally
            {               
                if (stream != null) stream.Close();
                if (container != null) container.Dispose();
            }
        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Get the local storage path for this file
        /// </summary>
        /// -----------------------------------------------------------------------
        private static string GetLocalPath(string fileName, ref StorageContainer container)
        {
            string localStoragePath;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Xbox)
            {
                localStoragePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"SavedGames\" + StorageName + @"\AllPlayers\" + fileName
                    );
            }            
            else  // Unix
            {
                string[] paths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    @".config", StorageName, @"SavedGames", @"AllPlayers", fileName
                };

                localStoragePath = Path.Combine(paths);
            }

            if (!Directory.Exists(Path.GetDirectoryName(localStoragePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(localStoragePath));
            }
            
            return localStoragePath;
        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Get the local storage path for this file
        /// </summary>
        /// -----------------------------------------------------------------------
        private static void StartStorageRequest()
        {
            available = true;
        }
    }
}