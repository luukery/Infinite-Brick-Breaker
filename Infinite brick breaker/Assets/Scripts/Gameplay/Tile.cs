using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is the script that is on the Tiles itself. here the health gets lowered when needed, the setup will be done from here and the colors will change accordingly.
/// </summary>
public class Tile : MonoBehaviour
{
    SpriteRenderer rend;

    public int health;
    [HideInInspector] public int healthHolder;
    public Obstacles obstacles;

    float countdownSpeed;
    int randomPowerupIndex;
    int endPowerup = -1;

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    

    /// <summary>
    /// In this void the health gets updated and everything that comes with that depending on the type of tile.
    /// </summary>
    public void HealthUpdate()
    {
        this.health--;

        if (health <= 0)
        {
            if (obstacles == Obstacles.RANDOM)       //activate spin
            {

                Powerups[] powerups = FindObjectsOfType<Powerups>();

                for (int i = 0; i < powerups.Length; i++)
                {
                    if (powerups[i].powerup == Powerup.TEMPONEHEALTH)
                    {
                        powerups[i].cooldown = true;
                    }
                }

                GameObject go = new GameObject("ItemShovel");
                go.transform.parent = transform;
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.transform.position = transform.position;

                transform.GetComponent<BoxCollider2D>().enabled = false;
                transform.GetComponent<SpriteRenderer>().enabled = false;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);

                StartCoroutine(RandomItem(sr));
            }
            else
            {
                DestroyTile();
            }
        }
        else
        {
            ColorChange(false);
        }
    }

    /// <summary>
    /// This void sets up everything that the tile will need in game. The 'firstime' bool is only true the first time going trough this. When it is true
    /// the tile gets setup with everyting it needs. 
    /// If it is not the first time the color just gets updated to the right amount of health.
    /// </summary>
    public void ColorChange(bool firstTime)
    {
        if (firstTime)
        {
            switch (obstacles)      //if a obstacle gets layer 7 it means the block can be broken
            {
                case Obstacles.NORMAL:
                    this.gameObject.layer = 7;
                    break;

                case Obstacles.UNBREAKABLE:
                    GameObject u = Instantiate(TileManager.instance.differentTiles[2], new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + -0.1f), Quaternion.identity);
                    u.transform.parent = this.transform;
                    break;

                case Obstacles.COVERED:
                    this.gameObject.layer = 7;
                    GameObject p = Instantiate(TileManager.instance.differentTiles[1], transform.position, Quaternion.identity);
                    p.transform.parent = this.transform;
                    p.transform.position = new Vector3(transform.position.x, transform.position.y + -0.2f, -0.01f);
                    break;

                case Obstacles.DEATH:
                    this.gameObject.layer = 7;
                    GameObject d = Instantiate(TileManager.instance.differentTiles[0], transform.position, Quaternion.identity);
                    d.transform.parent = this.transform;
                    break;

                case Obstacles.RANDOM:
                    this.gameObject.layer = 7;
                    GameObject s = Instantiate(TileManager.instance.differentTiles[3], transform.position, Quaternion.identity);
                    s.transform.position = new Vector3(transform.position.x, transform.position.y, -0.01f);
                    s.transform.parent = this.transform;
                    break;

                case Obstacles.FLIPED:
                    this.gameObject.layer = 7;
                    transform.rotation = Quaternion.Euler(0, 0, 90);
                    break;
            }
        }
        else
        {
            rend.color = TileManager.instance.colors[health];
        }
    }


    /// <summary>
    /// When a tile has 0 or less health this void gets activated. This makes sure the destruction of a tile goes as planned.
    /// </summary>
    public void DestroyTile()
    {
        if (obstacles == Obstacles.DEATH)
        {

            for (int i = GameManager.instance.balls.Count - 1; i > -1; i--)
            {
                GameObject l = GameManager.instance.balls[i].gameObject;
                l.GetComponent<Ball>().DestroyBall();
            }
        }

        if (TileManager.instance.activeTiles.Contains(this.GetComponent<Tile>()))
        {
            TileManager.instance.activeTiles.Remove(this.GetComponent<Tile>());
        }

        TileManager.instance.CheckTiles();

        ParticleManager.instance.ExplodingTile(transform.position);

        if (this.gameObject != null)
        {
              Destroy(this.gameObject);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(TileShake(0.1f, 1f));


        if (obstacles == Obstacles.COVERED)
        {
            if (collision.transform.position.y > this.transform.position.y)
            {
                collision.gameObject.GetComponent<Ball>().ChangeColorOfLine(health);
                HealthUpdate();     //I use this, because sometimes you break the brick and it doesn't bounce
            }
        }
        else if (gameObject.layer == 7 && !collision.transform.GetComponent<Ball>().explosiveBall)     // layer 7 = LoseHealth layer
        {
            HealthUpdate();
        }
    }


    /// <summary>
    /// This void is the backbone of the random tile. 
    /// </summary>
    IEnumerator RandomItem(SpriteRenderer sr)
    {
        if (endPowerup != randomPowerupIndex)
        {
            if (randomPowerupIndex == TileManager.instance.RandomSprites.Length - 1)
            {
                randomPowerupIndex = 0;
            }
            else
            {
                randomPowerupIndex++;
            }
            sr.sprite = TileManager.instance.RandomSprites[randomPowerupIndex];
        }

        yield return new WaitForSeconds((countdownSpeed));

        #region End of waiting period

        if (countdownSpeed >= 0.3f)
        {
            if (endPowerup == -1)
            {
                endPowerup = Random.Range(0, 3);
            }

            if (endPowerup == randomPowerupIndex)   //checks if the rotation is at the powerup random.range picked
            {

                switch (endPowerup)     //activate the current powerup
                {
                    case 0:
                        for (int i = 0; i < 5; i++)
                        {
                            GameObject g = Instantiate(GameManager.instance.explodingBall, transform.position, Quaternion.identity);

                            g.transform.parent = GameManager.instance.activeLevel.transform;
                        }
                        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);

                        break;

                    case 1:
                        Tile[] allTiles = FindObjectsOfType<Tile>();

                        yield return new WaitForSeconds((2));

                        for (int i = 0; i < allTiles.Length; i++)
                        {
                            if (i == 5)
                            {
                                break;
                            }

                            if (allTiles[i] != null && allTiles[i].obstacles == Obstacles.UNBREAKABLE)
                            {
                                Destroy(allTiles[i].gameObject);
                                yield return new WaitForSeconds(0.5f);
                            }
                        }

                        break;

                    case 2:

                        Tile[] alltiles = FindObjectsOfType<Tile>();




                        yield return new WaitForSeconds((2));


                        for (int i = 0; i < alltiles.Length; i++)
                        {
                            if (alltiles[i] != null)
                            {
                                if (alltiles[i].gameObject.layer == 7 && alltiles[i].obstacles != Obstacles.DEATH)
                                {
                                    alltiles[i].HealthUpdate();
                                    yield return new WaitForSeconds(0.1f);
                                }
                            }
                        }

                        break;
                }

                Powerups[] powerups = FindObjectsOfType<Powerups>();


                for (int i = 0; i < powerups.Length; i++)
                {
                    if (powerups[i].powerup == Powerup.TEMPONEHEALTH)
                    {
                        powerups[i].cooldown = false;
                    }
                }

                yield return new WaitForSeconds(2);
                Destroy(this.gameObject);
            }
            else
            {
                StartCoroutine(RandomItem(sr));     //keeps the items rotating until he reached the right one
            }
        }
        #endregion
        else
        {
            countdownSpeed += 0.01f;
            StartCoroutine(RandomItem(sr));
        }
    }

    /// <summary>
    /// When a tile gets hit (damaged or not) it will make a little shake. This is done here.
    /// </summary>
    IEnumerator TileShake(float duration, float magnitude)
    {
        Vector2 orgininalPos = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(transform.position.x + -0.03f, transform.position.x + 0.03f) * magnitude;

            transform.localPosition = new Vector2(x, transform.localPosition.y);

            elapsed += Time.deltaTime;

            yield return null;
        }
        transform.localPosition = orgininalPos;

    }
}
