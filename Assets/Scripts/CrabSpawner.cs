using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabSpawner : MonoBehaviour
{
    [SerializeField] Transform crab;
    [SerializeField] Transform terrain;
    [SerializeField] float min;
    [SerializeField] float max;
    [SerializeField] float range;

    Renderer ren;
    Bounds bound;

    float terrainAngle;
    float currTime = 0;
    float crabOffsetY = 22f;

    float randomTime
    {
        get { return Random.Range(min, max); }
    }

    Vector3 RandomPosition
    {
        get
        {
            float RandomX = Random.Range(bound.min.x, bound.max.x);
            float RandomZ = Mathf.Clamp(Random.Range(bound.min.z, bound.max.z), SandGameManager.Instance.Player.transform.position.z, SandGameManager.Instance.Player.transform.position.z + range);

            RandomZ += 5;

            float y = -Mathf.Tan(terrainAngle) * (RandomZ - bound.min.z) + terrain.position.y + crabOffsetY;

            return new Vector3(RandomX, y, RandomZ);
        }
    }

    Vector3 FrontPosition
    {
        get
        {
            Transform player = SandGameManager.Instance.Player;

            float y = -Mathf.Tan(terrainAngle) * (player.transform.position.z - bound.min.z + 5) + terrain.position.y + crabOffsetY;

            return new Vector3(player.transform.position.x , y, player.transform.position.z + 3f);
        }
    }

    bool isStart;

    private void Start()
    {
        terrainAngle = terrain.rotation.eulerAngles.x * Mathf.Deg2Rad;
        ren = terrain.GetComponent<Renderer>();
        bound = ren.bounds;
    }

    public void OnCrabStart()
    {
        if (isStart) return;

        isStart = true;
        StartCoroutine(SpawnStart());
        StartCoroutine(SpawnFront());
    }

    public void OnCrabStop()
    {
        if (!isStart) return;
        isStart = false;
        StopCoroutine(SpawnStart());
        StopCoroutine(SpawnFront());
    }

    public void CrabSpawn(Vector3 pos)
    {
        //float randomSpeed = Random.Range(10f, 20f);
        GameObject _crab = Instantiate(crab.gameObject);
        _crab.transform.position = pos;
        //_crab.transform.position = randomPosition;
        _crab.GetComponent<Crab>().UpperStart(10);
    }

    IEnumerator SpawnStart()
    {
        while (isStart)
        {
            currTime += Time.deltaTime;
            if(currTime > randomTime)
            {
                currTime = 0;
                CrabSpawn(RandomPosition);
            }
            yield return null;
        }
    }

    IEnumerator SpawnFront()
    {
        while (isStart)
        {
            yield return new WaitForSeconds(2);

            if (!isStart) break;

            SandGameManager.Instance.MarkOn();

            yield return new WaitForSeconds(2f);

            if (!isStart) break;
            CrabSpawn(FrontPosition);
            //curTime += Time.deltaTime;
            //if (curTime > spawnTime)
            //{
            //    Debug.Log("front claw");
            //    yield return new WaitForSeconds(3);
            //    curTime = 0;
            //    CrabSpawn(FrontPosition);
            //}
            //yield return null;
        }
    }

}
