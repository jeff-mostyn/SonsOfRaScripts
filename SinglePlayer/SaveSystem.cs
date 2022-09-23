using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveConquestPlayer(ConquestPlayer player)
    {
		BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + Constants.conquestFilePath + player.gameObject.name + ".data";
        //Debug.Log(path);
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData pData = new PlayerData(player);

        formatter.Serialize(stream, pData);
        stream.Close();
    }

    public static PlayerData LoadConquestPlayer(ConquestPlayer player)
    {
        string path = Application.persistentDataPath + Constants.conquestFilePath + player.gameObject.name + ".data";
		//Debug.Log(path + "   loading");

		if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData pData = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return pData;
        }
        else
        {
            //Debug.LogError("Save file not found");
            return null;
        }
    }

    public static void SaveNodeData(Node node, string nodeName) {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + Constants.conquestFilePath + nodeName + ".data";
		FileStream stream = new FileStream(path, FileMode.Create);

        NodeData nData = new NodeData(node);

        formatter.Serialize(stream, nData);
        stream.Close();
    }

    public static NodeData LoadNodeData(string nodeName) {
		string path = Application.persistentDataPath + Constants.conquestFilePath + nodeName + ".data";

        if (File.Exists(path)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            NodeData nData = formatter.Deserialize(stream) as NodeData;
            stream.Close();

            return nData;
        }
        else {
            return null;
        }
    }

    public static void ClearData() {
		string directoryPath = Application.persistentDataPath.ToString() + Constants.conquestFilePath;
		if (!Directory.Exists(directoryPath)) {
			Directory.CreateDirectory(directoryPath);
		}

		foreach (var file in Directory.GetFiles(Application.persistentDataPath + Constants.conquestFilePath)) {
            FileInfo file_info = new FileInfo(file);
            file_info.Delete();
        }
    }
}
