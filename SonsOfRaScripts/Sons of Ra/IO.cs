using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SonsOfRa.IO {
	public static class FileIO {
		private static string remoteFilePrefix = "data/";

		public static bool Save(byte[] data, string path, bool saveLocally) {
			if (SteamWorksManager.Instance.GetIsOnline()) {
				if (saveLocally) {
					string localpath = GetLocalSettingsFilePath(path);

					try {
						File.WriteAllBytes(localpath, data);
					}
					catch (System.Exception e) {
						Debug.LogError("Failed to save file locally\n" + e.Message);
						return false;
					}
				}

				if (SteamRemoteStorage.IsCloudEnabled) {
					try {
						bool writeSuccess = SteamRemoteStorage.FileWrite(remoteFilePrefix + path, data);
						Debug.Log(writeSuccess ? "successful cloud save" : "cloud save failed");
					}
					catch (System.Exception e) {
						Debug.LogError("Failed to save file remote\n" + e.Message);
						return false;
					}
				}

				return true;
			}
			Debug.LogWarning("SAVE FAILED, STEAM WAS NOT ONLINE");
			return false;
		}

		public static byte[] Load(string path) {
			//Debug.Log("There are " + SteamRemoteStorage.FileCount + " files saved to cloud");
			//foreach (var file in SteamRemoteStorage.Files) {
			//	Debug.Log($"{file} ({SteamRemoteStorage.FileSize(file)} {SteamRemoteStorage.FileTime(file)})");
			//}

			// get local path
			string localpath = GetLocalSettingsFilePath(path);

			// check if local file exists
			bool localFileExists = File.Exists(localpath);

			System.DateTime localWriteTime = new System.DateTime(), remoteWriteTime = new System.DateTime();
			// get write time of local path
			if (localFileExists) {
				localWriteTime = File.GetLastWriteTime(localpath);
			}
			//Debug.Log(localFileExists ? "local file exists" : "local file does not exist");

			// check if remote file exists
			bool remoteFileExists = SteamRemoteStorage.FileExists(remoteFilePrefix + path);

			// get write time of remote path
			if (remoteFileExists) {
				remoteWriteTime = SteamRemoteStorage.FileTime(remoteFilePrefix + path);
			}
			//Debug.Log(remoteFileExists ? "remote file exists" : "remote file does not exist");

			// if only one of them exists, load it
			// if both exist, load local
			// if neither exist, return empty array
			if (localFileExists) {
				//Debug.Log("loading local file");
				BinaryFormatter binRead = new BinaryFormatter();
				return File.ReadAllBytes(localpath);
			}
			else if (remoteFileExists) {
				//Debug.Log("loading remote file");
				return SteamRemoteStorage.FileRead(remoteFilePrefix + path);
			}
			else {
				//Debug.Log("no files found");
				return new byte[0];
			}
		}

		private static string GetLocalSettingsFilePath(string _filename) {
			// Create file path to stat saving location if it does not exist
			string directoryPath = Application.persistentDataPath.ToString() + Constants.settingsFilePath;
			if (!Directory.Exists(directoryPath)) {
				Directory.CreateDirectory(directoryPath);
			}

			// create file name, then file path
			return Path.Combine(directoryPath, _filename);
		}
	}
}
