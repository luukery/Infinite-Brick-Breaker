using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    float speed;

    Rigidbody2D rb;

    Ball ball;

    float paddleHeight;
    TrailRenderer trail;

    Vector2 direction;
    Vector2 currentDirection;

    [SerializeField] float offset;
    [HideInInspector]public bool differentSpeed;

    public bool explosiveBall;
    int explosiveHealth = 1;



    // Start is called before the first frame update
    void Start()
    {
        #region GetComponents
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        paddleHeight = GameObject.Find("Paddle").gameObject.transform.position.y;
        ball = this.GetComponent<Ball>();
        trail.material.EnableKeyword("_EMISSION");


        #endregion
        differentSpeed = false;

        speed = GameManager.instance.launchSpeed;
        StartCoroutine(SetAlpha());
    }


    /// <summary>
    /// sets alpha at the start so that it fades into the scene instead of popping in out of nowhere.
    /// </summary>
    /// <returns></returns>
    IEnumerator SetAlpha()
    {
        yield return null;

        GameManager.instance.balls.Add(this.GetComponent<Ball>());

        Color temp = new Color(1, 1, 1, 0); //color for the normal ball

        if (explosiveBall)
        {
            temp = new Color(0,0,0,0);  //different color for the explosive ball
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        float elapsedTime = 0;
        float duration = 0.2f;

        while (elapsedTime < duration)  //the idea of fading in started here
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);

            yield return null;
        }
        GetComponent<TrailRenderer>().enabled = true;

        yield return new WaitForSeconds(1);
        SetRandomTrajectory();
    }
   
    /// <summary>
    /// This launches the ball into a good direction.
    /// </summary>
    public void SetRandomTrajectory()
    {
        direction = Vector2.zero;
        direction.x = Random.Range(-0.5f, 0.5f);

        if (!explosiveBall)
        {
            direction.y = -1;
            direction.x = Random.Range(-0.5f, 0.5f);
        }
        else
        {
            direction.y = Random.Range(-180, 180);
            direction.x = Random.Range(-180, 180f);
        }

        if (direction.x < 0.15f && direction.x > -0.15f)
        {
            SetRandomTrajectory();
            return;
        }
        rb.AddForce(direction.normalized * speed * Time.deltaTime);
    }

    /// <summary>
    /// this makes sure the ball can speed up, but also that the ball goes slowly after a weird collision
    /// </summary>
    private void FixedUpdate()
    {
        if (!differentSpeed)
        {
            rb.velocity = rb.velocity.normalized * speed * Time.deltaTime;

            if (rb.velocity.magnitude < 6)
            {
                rb.AddForce(direction.normalized * speed * Time.deltaTime); ;
            }
        }
    }


    /// <summary>
    /// Changes the line of the trailrenderer
    /// </summary>
    public void ChangeColorOfLine(int health)
    {
        trail.startColor = TileManager.instance.colors[health];
        trail.material.color = trail.startColor;
        trail.material.SetColor("_EmissionColor", trail.startColor);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {


        #region offset bounce

        /// <summary>
        /// With a normal bounce, the ball will get stuck or go straight after some time. Because of this I added a offset everytime the ball hits somthing.
        /// </summary>

        if (collision.transform.position.x < transform.position.x)
        {
            rb.velocity += new Vector2(offset, 0);

        }
        else if (collision.transform.position.x > transform.position.x)
        {
            rb.velocity += new Vector2(-offset, 0);
        }

        if (collision.transform.position.y < transform.position.y)
        {
            rb.velocity += new Vector2(0, -offset);
        }
        else if (collision.transform.position.y > transform.position.y && collision.gameObject.layer != 6)
        {
            rb.velocity += new Vector2(0, offset);
        }
        else if (collision.gameObject.layer == 6)
        {
            rb.velocity += new Vector2(0, -offset);
        }

        #endregion

        #region explosive ball

        /// <summary>
        /// The explosive ball is a powerup from the random block, but because it is a ball it uses the same movement, but different aspects on the collision.
        /// </summary>

        if (!explosiveBall)
        {
            if (collision.transform.CompareTag("Paddle"))
            {
                rb.velocity += new Vector2(0, offset);
            }

            if (collision.transform.CompareTag("Tile") && collision.transform.GetComponent<Tile>().obstacles == Obstacles.NORMAL)    //Every tile that is breakeble
            {
                int tempHealth = collision.transform.GetComponent<Tile>().health;
                ChangeColorOfLine(tempHealth);
            }
        }
        else
        {
            if (collision.transform.CompareTag("Tile"))
            {
                if (collision.transform.GetComponent<Tile>().obstacles != Obstacles.DEATH)
                {
                    Tile v = collision.gameObject.GetComponent<Tile>();
                    v.health = 1;
                    v.HealthUpdate();

                    explosiveHealth--;

                    if (explosiveHealth <= 0)
                    {
                        Destroy(this.gameObject);
                    }
                }

            }
        }

        #endregion

        if (collision.transform.CompareTag("DeathHit"))      //killbox below the paddle
        {
            DestroyBall();
        }
    }

    //direct destruction
    public void DestroyBall()
    {
        GameManager.instance.balls.Remove(this.gameObject.GetComponent<Ball>());


        for (int i = 0; i < GameManager.instance.balls.Count; i++)
        {
            if(GameManager.instance.balls[i] == null)
            {
                GameManager.instance.balls.Remove(GameManager.instance.balls[i]);
            }
        }

        if (GameManager.instance.balls.Count <= 0)
        {
            GameManager.instance.RemoveLife();
        }

        Destroy(this.gameObject);
    }



}
