using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The game manager is the script that holds all the overal information and voids of the game.
/// The voids that are in here are making sure the gameplay structure works. 
/// Things like level changes, removing health, checking health are found here. 
///
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    [Header("Player stats")]
    [HideInInspector] public List<PhysicalHealth> lives = new List<PhysicalHealth>();
     public List<Ball> balls = new List<Ball>();

    [Header("Spawneble objects")]       //these are public, because they're prefabs from 'project'
    public GameObject ball;
    public GameObject healthHolder;
    public GameObject explodingBall;
   
    [Header("In game stats")]
    public float launchSpeed;
    public float speedGainByLevel;
    [HideInInspector] public float launchSpeedSave;

    [Header("Arrays")]
    public Sprite[] powerUpTextures;
    public Sprite[] healthTextures;


    public GameObject[] levels;
    public GameObject[] generatingLevelSetups;

    [HideInInspector] public GameObject activeLevel;
    [HideInInspector] public int index;             //this tracks what round the player is. After 7 levels and it starts generating it keeps counting up.


    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        launchSpeedSave = launchSpeed;
       
    }

    /// <summary>
    /// This void is used so that you can call the Ienumerator from other scripts when needed
    /// </summary>
    public void RemoveLife()
    {
        StartCoroutine(removeLife());
    }

    /// <summary>
    /// This removes a life from the player when there are no more balls in the game or a death tile has broken.
    /// the 'while' loop is the fading effect when a life goes away. This 'while' loop will be seen more for other gameobjects.
    /// </summary>
    IEnumerator removeLife()
    {
        GameObject ph = lives[lives.Count - 1].gameObject;

        SpriteRenderer sr = ph.GetComponent<SpriteRenderer>();
        Color temp = new Color(1, 1, 1, 1);

        float elapsedTime = 0;
        float duration = 0.2f;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(1,0, elapsedTime / duration);
            sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);

            yield return null;  
        }

        lives.Remove(ph.GetComponent<PhysicalHealth>());
        Destroy(ph);
        yield return new WaitForEndOfFrame();

        if(lives.Count > 0)
        {
            SpawnBall();
        }
        else 
        {
            ReturnToMenu();
        }
    }

   
    /// <summary>
    /// When a level is copmleted, or when the game starts, this void will make sure the level is switching to the right one.
    /// after the 7 main levels are completed, the levels that follow will be generated on a basis of 3 templates.
    /// </summary>
    public void NextLevel(bool start)
    {
        StartCoroutine(RemoveBall());

        if (activeLevel != null)    //remove all unbreakeble tiles from last level
        {
            Destroy(activeLevel);
        }

        if (start)      //if you boot up the game for the first time the index for the levels reset
        {
            index = 0;
            StartCoroutine(SpawnLevel());
        }
        else
        {
            index++;

            if (index >= levels.Length)   //generate new level
            {
                LevelGeneratorManager.instance.levelGeneratorSetup();       //if you're past lv 7
            }
            else
            {
                StartCoroutine(SpawnLevel());                               //if you're not
            }
        }

        
    }


    IEnumerator SpawnLevel()    //spawn the active level
    {
        StartCoroutine(RemoveBall());

        Destroy(activeLevel);
        activeLevel = null;

        yield return new WaitForEndOfFrame();

        activeLevel = Instantiate(levels[index], new Vector2(0, 0), Quaternion.identity);

       


        Tile[] amountOfTiles = Object.FindObjectsOfType<Tile>();

        for (int i = 0; i < amountOfTiles.Length; i++)
        {
           
               SpriteRenderer sr = amountOfTiles[i].GetComponent<SpriteRenderer>();
               Color temp = TileManager.instance.colors[amountOfTiles[i].health];



                float elapsedTime = 0;
                float duration = 0.2f;


                while(elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;

                    float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                    sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);
                   

                    yield return null;
                }

                amountOfTiles[i].ColorChange(true);

                
            if(amountOfTiles[i].obstacles != Obstacles.DEATH)       //right now the death tile is part placeholder so for now we do it this way, until we have the art and we're sure we get it different
            {
                int children = amountOfTiles[i].transform.childCount;

                if (children != 0)
                {
                    for (int y = 0; y < children; y++)
                    {
                        sr = amountOfTiles[i].transform.GetChild(y).GetComponent<SpriteRenderer>();
                        sr.enabled = true;

                        elapsedTime = 0;
                        duration = 0.3f;

                        while (elapsedTime < duration)
                        {
                            elapsedTime += Time.deltaTime;

                            float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                            sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);

                            yield return null;
                        }
                    }
                }
               
            }
            else //so this is for the death tile in particular
            {
                for (int q = 0; q < 3; q++)
                {
                    GameObject qsr = amountOfTiles[i].transform.GetChild(0).transform.GetChild(q).gameObject;
                    sr = qsr.gameObject.GetComponent<SpriteRenderer>();
                    temp = sr.color;



                    elapsedTime = 0;
                    duration = 0.2f;

                    while(elapsedTime < duration)
                    {
                        elapsedTime += Time.deltaTime;
                        float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);

                        sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);

                        yield return null;
                    }

                    if(q == 2)
                    {
                       ParticleSystem p = amountOfTiles[i].transform.GetChild(0).GetComponent<ParticleSystem>();
                        p.Play();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }


            yield return new WaitForSeconds(0.1f);  //this wait makes sure all the tiles appear one by one
        }


        Powerups[] powerups = FindObjectsOfType<Powerups>();

        for (int i = 0; i < powerups.Length; i++)
        {
            SpriteRenderer sr = powerups[i].GetComponent<SpriteRenderer>();
            powerups[i].transform.GetComponent<SpriteRenderer>().enabled = true;

            float elapsedTime = 0;
            float duration = 0.3f;

            while(elapsedTime < duration)       //only does not work on [COVERED], because the way the prefab is.
            {
                elapsedTime += Time.deltaTime;

                float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, newAlpha);
            
                yield return null;
            }

            if(powerups[i].powerup == Powerup.TELEPORT)     //bit of hard code, but it does it's job pretty much complete so it doesn't really mater does it?
            {
                ParticleSystem ps = powerups[i].GetComponentInChildren<ParticleSystem>();
                ps.Play();
            }
           
            yield return new WaitForSeconds(0.1f);
        }



        GetComponent<TileManager>().MakeList();
        launchSpeed += speedGainByLevel;

        yield return new WaitForSeconds(0.1f);

       

        yield return new WaitForSeconds(0.1f);

        SpawnBall();
    }


  
    /// <summary>
    /// You can take a guess to what this does
    /// </summary>
    void ReturnToMenu()
    {
        if(activeLevel != null)
        {
            Destroy(activeLevel);
        }

        GetComponent<ButtonScript>().startMenu.SetActive(true);
    }


    /// <summary>
    /// This only gets triggered when a level is completed. 
    /// </summary>
    IEnumerator RemoveBall()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            if(balls[0] != null)
            {
                balls[0].GetComponent<CircleCollider2D>().enabled = false;
                SpriteRenderer sr = balls[0].GetComponent<SpriteRenderer>();
                Color temp = new Color(1, 1, 1, 1);
                balls[0].GetComponent<TrailRenderer>().enabled = false;
                float elapsedTime = 0;
                float duration = 0.2f;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;

                    float newAlpha = Mathf.Lerp(1, 0, elapsedTime / duration);
                    sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);

                    yield return null;
                }

                if(balls.Count > 0)
                {
                    balls.Remove(balls[0]);
                }

            }
        }

        if (balls.Count < 0)
        {
            for (int y = balls.Count; y > 0; y--)
            {
                GameObject b = balls[0].gameObject;

                balls.Remove(balls[0]);
                Destroy(b);
            }
        }
    }

    /// <summary>
    /// the general call so spawn a ball. For the overview I made a void so I can track the amount of balls that are spawned to this one void.
    /// <summary>
    public void SpawnBall()
    {
        Instantiate(ball, new Vector2(0, 0), Quaternion.identity);
    }

   
}
