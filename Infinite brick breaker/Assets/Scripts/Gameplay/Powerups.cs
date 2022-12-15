using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This enum stores all the possible powerups. It works the same as in <see cref="TileManager"/> with the tiles.
/// </summary>
public enum Powerup
{
    EXTRABALL,
    TELEPORT,
    TEMPONEHEALTH,
    TEMPSPEEDUP
}
public class Powerups : MonoBehaviour
{ 
    public Powerup powerup;
    [HideInInspector] public bool cooldown;
    bool checkCooldown;
    

   
    // Start is called before the first frame update
    void Start()
    {
        checkCooldown = cooldown;
        setup();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector2(0, 50 * Time.deltaTime));      //constant rotation for all powerups (the hitbox also turns)

        if (checkCooldown != cooldown)      //this makes it trigger once. the checking of 2 bools is really light. this makes it perfect for this.
        {
            if (cooldown) //if the powerup is on cooldown the alpha is a bit lower so it visualy looks deactivated
            {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
            }
            checkCooldown = cooldown;
        }
    }
    /// <summary>
    ///This setup is the same for the <see cref="Tile.ColorChange(true)"/>, but now for the powerups
    /// </summary>
    void setup()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;

        switch (powerup)
        {
            case Powerup.EXTRABALL:
               GetComponent<SpriteRenderer>().sprite = GameManager.instance.powerUpTextures[0];
                GetComponent<BoxCollider2D>().size = new Vector2(1.3f, 1);
                break;

                case Powerup.TELEPORT:
               transform.GetChild(0).gameObject.SetActive(true);
                GetComponent<SpriteRenderer>().sprite = GameManager.instance.powerUpTextures[1];
                GetComponent<BoxCollider2D>().size = new Vector2(2.6f, 2);

                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                break;

            case Powerup.TEMPONEHEALTH:
                GetComponent<SpriteRenderer>().sprite = GameManager.instance.powerUpTextures[2];
                break;


            case Powerup.TEMPSPEEDUP:
                GetComponent<SpriteRenderer>().sprite = GameManager.instance.powerUpTextures[3];
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                GetComponent<BoxCollider2D>().size = new Vector2(2.5f, 2);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Ball>() != null && !cooldown)
        {
            cooldown = true;

            switch (powerup)    //activation of the powerups
            {
                case Powerup.EXTRABALL:
                   GameObject b = Instantiate(GameManager.instance.ball, transform.position, Quaternion.identity);
                    Destroy(this.gameObject);
                    break;

                case Powerup.TELEPORT:
                    StartCoroutine(Teleporter(collision.GetComponent<Ball>()));
                    break;

                case Powerup.TEMPONEHEALTH:
                    StartCoroutine(TempOneHealthTimer());
                    cooldown = true;
                    break;

                case Powerup.TEMPSPEEDUP:
                    cooldown = true;
                    StartCoroutine(SpeedUpAndDown(collision.gameObject));
                    break;
            }
        }
    }

    /// <summary>
    /// Every powerup has its own IEnumerator. These put the power in action.
    /// </summary>
   

    IEnumerator Teleporter(Ball ball)
    { 
        GameObject locationHolder = transform.GetChild(0).gameObject;
        Vector2 location = transform.GetChild(0).position;

        yield return new WaitForEndOfFrame();
        locationHolder.transform.parent = null;

        ball.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        ball.gameObject.GetComponent<Rigidbody2D>().simulated = false;

        yield return new WaitForSeconds(0.3f);  //0.3 is also the time for the line renderer to destroy itself
        ball.gameObject.GetComponent<TrailRenderer>().enabled = false;

        
        //teleport to location
        ball.gameObject.transform.position = location;

        yield return new WaitForSeconds(1);

        locationHolder.transform.parent = this.transform;
        ball.gameObject.GetComponent<Rigidbody2D>().simulated = true;
        ball.gameObject.GetComponent<TrailRenderer>().enabled = true;
        ball.gameObject.GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(10f);
        cooldown = false;
    }


    /// <summary>
    /// if you hit this powerup, all the tiles will get to one health for a second. This does not work in combination with all tiles -1 health for obvious reasons
    /// </summary>
    IEnumerator TempOneHealthTimer()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
       

        for (int i = 0; i < allTiles.Length; i++)
        {
            allTiles[i].healthHolder = allTiles[i].health;
            allTiles[i].health = 1;
            allTiles[i].ColorChange(false);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(3);

        Tile[] allTiles2 = FindObjectsOfType<Tile>();

        for (int i = 0; i < allTiles2.Length; i++)
        {
            if(allTiles2[i] != null)
            {
                allTiles2[i].health = allTiles2[i].healthHolder;
                allTiles2[i].ColorChange(false);
            }
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForEndOfFrame();
        Destroy(this.gameObject);

    }


    /// <summary>
    /// Temporary extra speed for the ball
    /// </summary>
    IEnumerator SpeedUpAndDown(GameObject speedingObject)
    {        
        Rigidbody2D rb = speedingObject.GetComponent<Rigidbody2D>();


        speedingObject.GetComponent<Ball>().differentSpeed = true;
        rb.AddForce(rb.velocity.normalized * 10 );

        yield return new WaitForSeconds(3);


        if (speedingObject != null)
        {
            speedingObject.GetComponent<Ball>().differentSpeed = false; 
        }

        speedingObject = null;
        yield return new WaitForSeconds(5);

        cooldown = false;

    }
}
