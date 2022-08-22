using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.NPCScriptableObj;
using AISenses;
using Unity.Entities;
using System.Threading.Tasks;
using System;

public class TestSpawn : MonoBehaviour
{
    public List<NPCSpawn> test;
    public WorldNPCSpwan NPCToSpawn;
    public void Start()
    {
         //InvokeRepeating(nameof(Spawn), 0, 15);
  
        NPCToSpawn.SpawnWorld(this.transform.position);
    }
    async void Spawn()
    {
        for (int i = 0; i < test.Count; i++)
        {
            if (test[i].SpawnSomething)
            {
                for (int j = 0; j < test[i].SpawnPerCall; j++)
                {
                    Utilities.GlobalFunctions.RandomPoint(transform.position, 5.0f, out Vector3 Spos);
                    test[i].SOToSpawn.Spawn(Spos);
                    await Task.Delay(TimeSpan.FromSeconds(1f));

                    test[i].spawned++;
                }
            }
        }

    }



    [System.Serializable]
   public class NPCSpawn {
        public NPCSO SOToSpawn;
        [Range(1, 50)]
        public int SpawnPerCall;
        [Range(1, 2000)]
        public int spawnCNT;
        public int spawned;
        public bool SpawnSomething => spawned < spawnCNT;
    }

    void OnceAllSpawnedCreateExplosion() {
        GameObject GO = new GameObject();
       GO.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;

        GO.AddComponent<SoundAuthoring>().Emitter = new SoundEmitter()
        {
            Sound = AISenses.SoundTypes.Explosion,
            SoundLevel = 100
        };
     
    }
}
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject optionsMenuPanel;

   
    public bool isGamePaused { get; private set; }


    public void Pause()
    {
        isGamePaused = true;
        pauseMenuPanel.SetActive(isGamePaused);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isGamePaused = false;
        optionsMenuPanel.SetActive(isGamePaused);
        pauseMenuPanel.SetActive(isGamePaused);

        Time.timeScale = 1f;
    }

    public void OnPauseMenu()
    {
        if (isGamePaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
}