using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// All the possible Tile variants. The enum is setup in the inspector for the normal levels.
/// For the generated levels they get a random int given in <see cref="LevelGeneratorManager.levelGenerator"/>
/// </summary>
public enum Obstacles
{
   NORMAL,
   UNBREAKABLE,
   COVERED,
   DEATH,
   RANDOM,
   FLIPED,
}

/// <summary>
/// The TileManager has everythning that has to do with tiles. It does not hold induvidual components like health,
/// but stores the overal data. (the amount of tiles in scene etc.)
/// </summary>
public class TileManager : MonoBehaviour
{
    public static TileManager instance;

    public List<Tile> activeTiles = new List<Tile>();   //gets all breakeble tiles 
    public Color[] colors;  //colors for the tile health
    Tile[] allTiles; //takes all tiles that are in the game
    public GameObject[] differentTiles; //different type of tiles

    public Sprite[] RandomSprites;  //sprites of the random powerup

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        MakeList();
    }

   
    /// <summary>
    /// This checks if all tiles are destroyed and if so, goes to the next level
    /// </summary>
    public void CheckTiles()
    {
        if (activeTiles.Count <= 0)
        {
            GetComponent<GameManager>().NextLevel(false);

        }
    }

    /// <summary>
    /// This makes the list with all the tiles in a level. 
    /// </summary>
    public void MakeList()
    {
        allTiles = FindObjectsOfType<Tile>();

        activeTiles.Clear();

        for (int i = 0; i < allTiles.Length; i++)
        {
            if (allTiles[i].gameObject.layer == 7 && allTiles[i].obstacles != Obstacles.DEATH && allTiles[i].health >= 1 && allTiles[i].obstacles != Obstacles.RANDOM)
            {
                activeTiles.Add(allTiles[i]);
            }
        }

        
    }
}
