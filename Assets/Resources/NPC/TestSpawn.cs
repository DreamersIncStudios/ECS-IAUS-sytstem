using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.NPCSO;
using AISenses;
using Unity.Entities;
public class TestSpawn : MonoBehaviour
{
    public List<NPCSpawn> test;

    public void Start()
    {
        InvokeRepeating(nameof(Spawn), 0, 5);
        Invoke(nameof(OnceAllSpawnedCreateExplosion), 60);
    }


    public void Test() {
        Debug.Log("Button Pressed");
    }
    void Spawn()
    {
        for (int i = 0; i < test.Count; i++)
        {
            if (test[i].SpawnSomething)
            {
                for (int j = 0; j < test[i].SpawnPerCall; j++)
                {
                    test[i].SOToSpawn.Spawn(this.transform.position);
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