using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad {

    public static bool unlockAllLevels = true;
	public static int levelNo;
	
	// Ensure that we can save whether the player has purchased premium pack or not
	public static void Save() {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/levelData.dat");
            bf.Serialize(file, SaveLoad.levelNo);
            file.Close();
            Debug.Log("Saved on level " + levelNo);
        }
		catch
        {
            Debug.LogError("Saving game data failed.");
        }
	}

	// This should be run on start
	public static void Load() {
        if (unlockAllLevels)
        {
            SaveLoad.levelNo = 25;
            Debug.Log("All levels are unlocked (unlockAllLevels=True in SaveLoad)");
        }
        else
        {
            if (File.Exists(Application.persistentDataPath + "/levelData.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/levelData.dat", FileMode.Open);
                SaveLoad.levelNo = (int)bf.Deserialize(file);
                file.Close();
                if (levelNo > 30)
                {
                    SaveLoad.levelNo = 1;
                    Debug.Log("Corrupt data found. Reset to level 1");
                }
                else
                {
                    Debug.Log("Loaded level " + levelNo);
                }
            }
            else
            {
                // Handle load failure.
                SaveLoad.levelNo = 1;
                Debug.Log("No save data found. Starting on level 1");
            }
        }
	}
}
