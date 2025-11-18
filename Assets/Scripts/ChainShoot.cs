using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ChainShoot : MonoBehaviour
{
    [SerializeField] float refreshRate = 0.1f;
    [SerializeField][Range(1, 10)] int maximunEnemiesInChain = 3;
    [SerializeField] float delayBetweenEacChain = 0.5f;
    [SerializeField] Transform playerFirePoint;
    [SerializeField] EmenyDetector playerEnemyDetector;
    [SerializeField] GameObject linRendererPrefab;

    bool shooting;
    bool shot;
    float counter = 1;
    GameObject currentClosetEnemy;

    List<GameObject> spawnedLinRenderers = new List<GameObject>();
    List<GameObject> enemiesInChain = new List<GameObject>();
    List<GameObject> activeEffects = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            if(playerEnemyDetector.GetEnemiesInRange().Count > 0)
            {
                if(!shooting)
                {
                    StartShooting();
                }
            }
            else
            {
                StopShooting();
            }
        }

        if(Input.GetButton("Fire1"))
        {
            StopShooting();
        }
    }

    IEnumerator ChainReaction(GameObject closestEnemy)
    {
        yield return new WaitForSeconds(delayBetweenEacChain);

        if (counter == maximunEnemiesInChain)
        {
            yield return null;
        }
        else
        {
            if (shooting)
            {
                counter++;
                enemiesInChain.Add(closestEnemy);
                if(!enemiesInChain.Contains(closestEnemy.GetComponent<EmenyDetector>().GetClosestEnemy()))
                {
                    NewLineRenderer(closestEnemy.transform, closestEnemy.GetComponent<EmenyDetector>().GetClosestEnemy().transform);
                    StartCoroutine(ChainReaction(closestEnemy.GetComponent<EmenyDetector>().GetClosestEnemy()));
                }
               
            }
        }
    }

    void NewLineRenderer(Transform startPos, Transform endPos, bool getClosestEnmeyToPlayer = false)
    {
        GameObject lineR = Instantiate(linRendererPrefab);
        spawnedLinRenderers.Add(lineR);
        StartCoroutine(UpdateLineRenderer(lineR , startPos, endPos, getClosestEnmeyToPlayer));
    }

    IEnumerator UpdateLineRenderer(GameObject lineR, Transform startPos, Transform endPos, bool getClosesEnemyToPlayer = false)
    {
        if(shooting && shot && lineR != null)
        {
            lineR.GetComponent<LineRendererController>().SetPosition(startPos, endPos);
            yield return new WaitForSeconds(refreshRate);

            if (getClosesEnemyToPlayer )
            {
                StartCoroutine(UpdateLineRenderer(lineR, startPos, playerEnemyDetector.GetClosestEnemy().transform, true));
                if(currentClosetEnemy != playerEnemyDetector.GetClosestEnemy())
                {
                    StopShooting();
                    StartShooting();
                }
            }
            else
            {
                StartCoroutine(UpdateLineRenderer(lineR, startPos, endPos));
            }
        }
    }

    void StartShooting()
    {
        shooting = true;

        if (playerEnemyDetector != null && playerFirePoint != null && linRendererPrefab != null)
        {
            if (!shot)
            {
                shot = true;
                currentClosetEnemy = playerEnemyDetector.GetClosestEnemy();
                NewLineRenderer(playerFirePoint, playerEnemyDetector.GetClosestEnemy().transform, true);

                if (maximunEnemiesInChain > 1)
                {
                    StartCoroutine(ChainReaction(playerEnemyDetector.GetClosestEnemy()));
                }
            }
        }
    }

    void StopShooting()
    {
        shooting = false;
        shot = false;   

        for (int i = 0; i < spawnedLinRenderers.Count; i++)
        {
            Destroy(spawnedLinRenderers[i]);
        }

        spawnedLinRenderers.Clear();
        enemiesInChain.Clear();

        for (int i = 0; i < activeEffects.Count; i++)
        {
            Destroy(activeEffects[i]);
        }

        activeEffects.Clear();
    }
}
