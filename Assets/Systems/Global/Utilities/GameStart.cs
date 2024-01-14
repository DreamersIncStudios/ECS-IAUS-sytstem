using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DreamersInc.SaveSystem
{


    public class GameStart : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GetOrCreateDirectories();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void GetOrCreateDirectories()
        {
            if (!Directory.Exists(Application.persistentDataPath + "GameData"))
            {
              DirectoryInfo stream = Directory.CreateDirectory(Application.persistentDataPath + "GameData");
            }
            if (!Directory.Exists(Application.persistentDataPath + "GameSettingData"))
            {
                DirectoryInfo stream = Directory.CreateDirectory(Application.persistentDataPath + "GameSettingData");
            }
        }
    }
}
