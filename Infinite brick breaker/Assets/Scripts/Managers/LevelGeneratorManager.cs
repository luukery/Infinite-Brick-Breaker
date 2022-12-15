using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is taking effect after level 7. That is because the levels get generated after that.
/// This script is also on the managers GameObject. You can set the variables in the editor.
/// It will be generate on base of a template. From there it will fill it in for itself. 
/// </summary>
public class LevelGeneratorManager : MonoBehaviour
{
    public static LevelGeneratorManager instance;

     [SerializeField] GameObject[] spawnableObjects; 
     GameObject[] spawnPoints;

     List<GameObject> activeSpawnlocations = new List<GameObject>();

     int amountOfTiles;
     int amountOfNormalTiles;
    

    [HideInInspector] public int levelsGenerated;

    [Header("Max Amount Of Tiles for Generation")]      //public is the number player fills in at [INSPECTOR] and the private int will check the number that has been filled in
    
    [Tooltip("The minimal amount of normal tiles that will spawn when generating")]
    public int minTiles;

    [Tooltip("The maximum amount of unbreakeble tiles that can spawn when generating ")]
    public int maxUnbreakeble;
    int maxUnbreakebleCount;

    [Tooltip("The maximum amount of covered tiles that can spawn when generating ")]

    public int maxCovered;
    int maxCoveredCount;

    [Tooltip("The maximum amount of death tiles that can spawn when generating ")]
    public int maxDeath;
    int maxDeathCount;

    [Tooltip("The maximum amount of random tiles that can spawn when generating ")]
    public int maxRandom;
    int maxRandomCount;

    [Tooltip("Maximum amount of powerups that can spawn when generating")]
    public int maxPowerup;

    GameObject activeLayout;


   

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// setup for the generation of a level
    /// </summary>
    public void levelGeneratorSetup()
    {
        if(activeLayout != null)
        {
           Destroy(activeLayout);
        }

        int levelGeneratorLayout = Random.Range(0, GameManager.instance.generatingLevelSetups.Length);

        activeLayout = Instantiate(GameManager.instance.generatingLevelSetups[levelGeneratorLayout]);

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        amountOfTiles = Random.Range(minTiles, spawnPoints.Length);

        while(!(activeSpawnlocations.Count == amountOfTiles))
        {
            int s = Random.Range(0, spawnPoints.Length);

            if (!activeSpawnlocations.Contains(spawnPoints[s]))
            {
                activeSpawnlocations.Add(spawnPoints[s]);
            }
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!activeSpawnlocations.Contains(spawnPoints[i]))
            {
                spawnPoints[i].SetActive(false); 
            }
        }

        amountOfNormalTiles = amountOfTiles;


        StartCoroutine(levelGenerator());

        
    }

    /// <summary>
    /// The making of the level
    /// </summary>
    IEnumerator levelGenerator()
    {
        #region resetGenerationNumbers
        maxUnbreakebleCount = 0;
        maxRandomCount = 0;
        maxDeathCount = 0;
        maxRandomCount = 0;
        #endregion


        levelsGenerated++;
        GameManager.instance.index++;

        GameObject b = new GameObject("Generated level " + levelsGenerated);
        GameManager.instance.activeLevel = b;

        List<int> permanumbers = new List<int>();       //temp list that checks the spawnlocations of the powerup(s)

        int l = Random.Range(0, maxPowerup);

        while (permanumbers.Count != l)      //fill in where the powerups will spawn
        {
            int randomSpawnLocation = Random.Range(0, activeSpawnlocations.Count);

            if (!permanumbers.Contains(randomSpawnLocation))
            {
                permanumbers.Add(randomSpawnLocation);
            }

            yield return null;

        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < activeSpawnlocations.Count; i++)
        {
            int powerupSpawn = Random.Range(0, activeSpawnlocations.Count);

            GameObject tile = null ;

            if(permanumbers.Contains(i))        //checks if a powerup has to spawn here
            {
               tile = Instantiate(spawnableObjects[1], activeSpawnlocations[i].transform.position, Quaternion.identity, b.transform);

                if(tile.transform.position.y <= -4)
                {
                    Destroy(tile);
                }

                int randomPowerup = Random.Range(0, 4);

                if(randomPowerup == 1)
                {
                    randomPowerup = 3;
                }

                tile.GetComponent<Powerups>().powerup = (Powerup)randomPowerup;

            }
            else                  //add a normal tile (extra tiles are included here
            {
                tile = Instantiate(spawnableObjects[0], activeSpawnlocations[i].transform.position, Quaternion.identity, b.transform);

                int randomObstacle = Random.Range(0, 5);

                tile.GetComponent<Tile>().health = Random.Range(1, 6);
                tile.GetComponent<Tile>().obstacles = (Obstacles)randomObstacle;

                
                #region requirments and finetuning
                switch (randomObstacle)     //here the details for the tiles with details so that you dont get impossible levels
                {
                        case 1:     //unbreakable
                       

                        if(maxUnbreakebleCount >= maxUnbreakeble)
                        {
                            tile.GetComponent<Tile>().obstacles = Obstacles.NORMAL;
                        }
                        else
                        {
                            maxUnbreakebleCount++;
                        }
                        break;

                    case 2:         //covered
                        
                        if(tile.transform.position.y > 2 || maxCoveredCount >= maxCovered)
                        {
                            tile.GetComponent<Tile>().obstacles = Obstacles.NORMAL;
                        }
                        else
                        {
                            maxCoveredCount++;
                        }
                        break;



                    case 3:         //death
                        if(maxDeathCount >= maxDeath)
                        {
                            tile.GetComponent<Tile>().obstacles = Obstacles.NORMAL;
                        }
                        else
                        {
                            maxDeathCount++;
                            tile.GetComponent<Tile>().health = 5;
                        }

                        break;

                    case 4:         //random

                        if(maxRandomCount >= maxRandom)
                        {
                            tile.GetComponent<Tile>().obstacles = Obstacles.NORMAL;

                        }
                        else
                        {
                            maxRandomCount++;
                        }
                        break;

                   //fliped cannot be generated, because it gives problems with the level layout(s)
                }

                if(tile.transform.position.y <= -4)
                {
                    tile.GetComponent<Tile>().health = Random.Range(1, 3);

                    if(tile.GetComponent<Tile>().obstacles != Obstacles.NORMAL)
                    {
                        tile.GetComponent<Tile>().obstacles = Obstacles.NORMAL;
                    }

                }
                #endregion

            }

            float elapsedTime = 0;
            float duration = 0.2f;
            Color temp = new Color(0,0,0,0);
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();

            if(tile.gameObject.CompareTag("Tile") )
            {
                temp = TileManager.instance.colors[tile.GetComponent<Tile>().health];
            }
            else if(tile.gameObject.CompareTag("Powerup"))
            {
                temp = new Color(1, 1, 1, 0);
            }


            while (elapsedTime < duration)      //looking back at it, I really shoulve made a fade function instead of using it multiple times
            {
                elapsedTime += Time.deltaTime;

                float newAlpha = Mathf.Lerp(0,1, elapsedTime/ duration);

                sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);


                yield return null; 
            }

            if (tile.transform.CompareTag("Tile"))
            {
                tile.GetComponent<Tile>().ColorChange(true);

                if (tile.GetComponent<Tile>().obstacles != Obstacles.DEATH)
                {
                    int children = tile.transform.childCount;

                    for (int y = 0; y < children; y++)
                    {
                        sr = tile.transform.GetChild(y).GetComponent<SpriteRenderer>();


                        elapsedTime = 0;
                        duration = 0.2f;
                        temp = new Color(1, 1, 1, 0);

                        while(elapsedTime < duration)
                        {
                            elapsedTime += Time.deltaTime;
                            float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                            sr.color = new Color(temp.r, temp .g, temp .b, newAlpha);

                            yield return null;
                        }
                        yield return new WaitForSeconds(0.2f);

                    }  
                }
                else
                {
                   int children = tile.transform.GetChild(0).childCount;

                    for (int s = 0; s < children; s++)
                    {
                        sr = tile.transform.GetChild(0).GetChild(s).GetComponent<SpriteRenderer>();


                        elapsedTime = 0;
                        duration = 0.2f;
                        temp = new Color(1, 1, 1, 0);

                        while (elapsedTime < duration)  //yet again
                        {
                            elapsedTime += Time.deltaTime;
                            float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                            sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);

                            yield return null;
                        }
                        yield return new WaitForSeconds(0.2f);

                    }

                    tile.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        Destroy(activeLayout);
        activeLayout = null;
        activeSpawnlocations.Clear();

        GameManager.instance.SpawnBall();
        TileManager.instance.MakeList();

    }
}
