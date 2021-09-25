using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using Microsoft.Xna.Framework.GamerServices;
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
        static StorageDevice storageDevice;
        static DateTime lastStorageRequest = DateTime.MinValue;
#if XBOX360
        static IAsyncResult storageResult;
#endif

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
#if XBOX360
                if (storageDevice == null || !storageDevice.IsConnected)
                {
                    return;
                }
#endif
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
#if XBOX360
                if (storageDevice == null || !storageDevice.IsConnected)
                {
                    return;
                }
#endif
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
#if XBOX360
            container = storageDevice.OpenContainer(StorageName);
            string fullpath = Path.Combine(container.Path, fileName);
#else
            string fullpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),@"SavedGames\" + StorageName + @"\AllPlayers\" + fileName);
            if (!Directory.Exists(Path.GetDirectoryName(fullpath))) Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
#endif
            return fullpath;
        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Get the local storage path for this file
        /// </summary>
        /// -----------------------------------------------------------------------
        private static void StartStorageRequest()
        {
#if XBOX360
            if ((DateTime.Now - lastStorageRequest).TotalSeconds > 3)
            {
                try
                {
                    // Set the request flag
                    if ((!Guide.IsVisible))
                    {
                        lastStorageRequest = DateTime.Now;
                        storageResult = Guide.BeginShowStorageDeviceSelector(StorageCompletedCallback, null);
                    }
                }
                catch (Exception) { }
            }
#else
            available = true;     
#endif

        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Callback for storage request
        /// </summary>
        /// -----------------------------------------------------------------------
        static void StorageCompletedCallback(IAsyncResult result)
        {
            storageDevice = Guide.EndShowStorageDeviceSelector(result);
            available = true;
        }

    }
}