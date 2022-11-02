using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

//[ExecuteAlways]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody body;
    public GameObject explosionPrefab;
    public Transform track;
    private List<Transform> pathElementsShow = new List<Transform>();
    private List<Vector3>[] pathElements = new List<Vector3>[4];

    public AudioClip soundMove;
    private AudioSource audioMove;

    public Slider progressBar;

    public RoadArchitect.Road architectRoad;

    private List<TimerCollision> listTimerCollisions = new List<TimerCollision>();
    private ControlerTrack trackController = new ControlerTrack();
    private Turner turner = new Turner();
    private SimpleTimer timer = new SimpleTimer();

    static public float speed = 10f;
    private float speedMax = 50f;
    private float speedStart = 10f;
    public float speedIncrease = 2.0f;
    public float speedDecrease = 4.0f;
    private float speedTurnMax = 2f;
    private float speedTurn = 2f;
    public float leftPos = -2f;
    public float rightPos = 2f;
    public float force = 10f;
    public float radForce = 3f;
    public float timerCollision;

    static public bool IsClickTurnLeft { get; set; }
    static public bool IsClickTurnRight { get; set; }
    static public bool IsClickAccelerator { get; set; }
    static public bool IsClickBrekes { get; set; }

    bool isTurnLeft;
    bool isTurnRight;

    private void OnCollisionEnter(Collision collision)
    {
        //Vector3 sizeObj = collision.transform.GetComponent<Collider>().bounds.size;
        return;
        if (collision.gameObject.tag == "Track") {
            return;
        }

        GameObject collisonObj = collision.gameObject;
        foreach (var timer in listTimerCollisions) {
            if (timer.collisionObj == collisonObj) {
                return;
            }
        }
        listTimerCollisions.Add(new TimerCollision(collisonObj));

        if (collision.gameObject.tag == "Target") {
            ContactPoint contact = collision.contacts[0];
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 position = contact.point;
            Instantiate(explosionPrefab, position, rotation);
            progressBar.value += 0.1f;

            //float d = 2f;
            //Transform car = collision.gameObject.transform;
            //float xDist = body.position.x - car.position.x;
            //float xShift = xDist > 0 ? -(d - xDist) : (d - Math.Abs(xDist));

            //if (xDist < d) {
            //    transform.position = new Vector3(transform.position.x - xShift, transform.position.y, transform.position.z);
            //    //car.position = new Vector3(car.position.x + xShift / 2, car.position.y, car.position.z);

            //    Vector3 vec = body.transform.rotation.eulerAngles;
            //    body.transform.rotation = Quaternion.Euler(vec.x, vec.y - xShift * 10, vec.z);
            //}

            if (CanvasSlots.IsSound) {
                GetComponent<AudioSource>().time = 3.4f;
                GetComponent<AudioSource>().Play();
            }
        }
        else {
            ContactPoint contact = collision.contacts[0];
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 position = contact.point;
            Instantiate(explosionPrefab, position, rotation);

            if (CanvasSlots.IsSound) {
                GetComponent<AudioSource>().time = 3.4f;
                GetComponent<AudioSource>().Play();
            }

            //GameController.IsLose = true;
            //speed = speedStart;
            //body.velocity = Vector3.zero;
            //audioMove.Stop();

            float d = 2f;
            Transform car = collision.gameObject.transform;
            float xDist = body.position.x - car.position.x;
            float xShift = xDist > 0 ? -(d - xDist) : (d - Math.Abs(xDist));

            if (xDist < d) {
                transform.position = new Vector3(transform.position.x - xShift / 2, transform.position.y, transform.position.z);
                car.position = new Vector3(car.position.x + xShift / 2, car.position.y, car.position.z);

                Vector3 vec = body.transform.rotation.eulerAngles;
                body.transform.rotation = Quaternion.Euler(vec.x, vec.y - xShift * 10, vec.z);
            }

            body.AddExplosionForce(force, position, radForce);

            //speed *= 0.8f;
            //Debug.Log("Crash: " + sizeObj.x + " " + sizeObj.y + " " + sizeObj.z + ". Tag: " + collision.gameObject.tag);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var element in pathElementsShow) {
            Gizmos.DrawSphere(element.position, 1);
        }
    }

    void Start()
    {
        body = GetComponent<Rigidbody>();

        //audioMove = gameObject.AddComponent<AudioSource>();
        //audioMove.clip = soundMove;
        //audioMove.Play();

        speed = speedStart;

        List<Vector3> markersTrack = new List<Vector3>();
        for(int i=0; i<pathElements.Length; i++) {
            pathElements[i] = new List<Vector3>(); 
        }
        for (int i = 0; i<track.childCount; i++) {
            Vector3 newVec1 = RotationCounter.getShiftMarker(track.GetChild(i).position, track.GetChild(i).rotation.eulerAngles.y, 7.5f, -1);
            Transform newTransform1 = Instantiate(track.GetChild(i), newVec1, Quaternion.identity);
            pathElements[0].Add(newVec1);

            Vector3 newVec2 = RotationCounter.getShiftMarker(track.GetChild(i).position, track.GetChild(i).rotation.eulerAngles.y, 2.5f, -1);
            Transform newTransform2 = Instantiate(track.GetChild(i), newVec2, Quaternion.identity);
            pathElements[1].Add(newVec2);

            Vector3 newVec3 = RotationCounter.getShiftMarker(track.GetChild(i).position, track.GetChild(i).rotation.eulerAngles.y, 2.5f, 1);
            Transform newTransform3 = Instantiate(track.GetChild(i), newVec3, Quaternion.identity);
            pathElements[2].Add(newVec3);

            Vector3 newVec4 = RotationCounter.getShiftMarker(track.GetChild(i).position, track.GetChild(i).rotation.eulerAngles.y, 7.5f, 1);
            Transform newTransform4 = Instantiate(track.GetChild(i), newVec4, Quaternion.identity);
            pathElements[3].Add(newVec4);

            pathElementsShow.Add(newTransform3);
            markersTrack.Add(newVec3);
        }

        //pathElementsShow = createBezier(pathElementsShow);

        for(int i=0; i<pathElements.Length; i++) {
            trackController.setMarkersTrack(pathElements[i], i);
        }

        turner.setStartAngle(transform.rotation.eulerAngles.y);
        float ang = RotationCounter.getNewDirection(transform.position, trackController.getCurrentMarker(), transform.rotation.eulerAngles.y);
        turner.setTarget(ang, transform.rotation.eulerAngles.y);

        timeMove = numberPoints * 3f;
    }

    void debugTest()
    {
        string str = "Track markers: ";
        //for (int i = 0; i<markersTrack.Count; i++) {
        //    str += markersTrack[i].z + " ";
        //}
        Debug.Log(str);
    }

    bool isTest = true;
    bool isTestPath = true;
    int numPath = 0;
    void Update()
    {
        //if (Input.GetKey("left") || IsClickTurnLeft)
        //    isTurnLeft = true;
        //else isTurnLeft = false;


        //if (Input.GetKey("right") || IsClickTurnRight)
        //    isTurnRight = true;
        //else isTurnRight = false;

        //if (IsClickAccelerator || Input.GetKey("f")) {
        //    increaseSpeed();
        //} else if (speed > 5) speed -= 1 * Time.deltaTime;

        //if (IsClickBrekes || Input.GetKey("d")) {
        //    decreaseSpeed();
        //}
        testMoving();


        //Test
        if (Input.GetKey("t") && isTest) {
            debugTest();
            isTest = false;
        } else if (!Input.GetKey("t")) isTest = true;

        
        if (Input.GetKey("e") && isTestPath) {
            trackController.setCurrentPath(numPath++);
            if (numPath >= pathElements.Length) numPath = 0;
            isTestPath = false;
        } else if (!Input.GetKey("e")) isTestPath = true;
        //

        for (int i = 0; i < listTimerCollisions.Count; i++) {
            if(listTimerCollisions[i].isProcess()) {
                listTimerCollisions.RemoveAt(i);
                i--;
            }
        }


        if (GetComponent<AudioSource>().isPlaying && GetComponent<AudioSource>().time >= 5.0f)
            GetComponent<AudioSource>().Stop();

        //if (audioMove.isPlaying && audioMove.time >= 9.0f)
        //    audioMove.time = 1.5f;
    }

    private void FixedUpdate()
    {
        //float t = Time.fixedDeltaTime;

        t += Time.deltaTime / timeMove;
        int n = numberPoints;
        Vector3[] points = new Vector3[n];
        for (int i = 0; i<n; i++) points[i] = pathElements[2][i];
        transform.position = Bezier.GetPoint4(t, points);
        //if (isTurnLeft) {
        //    //Quaternion turnTo = Quaternion.Euler(0, tarAng, 0);
        //    //body.transform.rotation = Quaternion.Lerp(transform.rotation, turnTo, speedTurn * t);
        //}
        //else if (isTurnRight) {
        //    //Quaternion turnTo = Quaternion.Euler(0, tarAng, 0);
        //    //body.transform.rotation = Quaternion.Lerp(transform.rotation, turnTo, speedTurn * t);
        //}

        //New
        //if (turner.turn == Turner.Turn.Left) {
        //    Quaternion turnTo = Quaternion.Euler(0, turner.getTarget(), 0);
        //    body.transform.rotation = Quaternion.Lerp(transform.rotation, turnTo, turner.turnSpeed * t);
        //} else if (turner.turn == Turner.Turn.Right) {
        //    Quaternion turnTo = Quaternion.Euler(0, turner.getTarget(), 0);
        //    body.transform.rotation = Quaternion.Lerp(transform.rotation, turnTo, turner.turnSpeed * t);
        //}

        //turner.checkReady(transform.rotation.eulerAngles.y);

        //body.MovePosition(transform.position + transform.TransformDirection(Vector3.forward * speed * t));
        ////

        ////body.MovePosition(transform.position + moveForward);
        ////body.AddForce(moveForward, ForceMode.VelocityChange);
    }

    //[Range(0, 1)]
    //public float t = 0;
    float t = 0;
    int nElement = 0;
    public int numberPoints = 4;
    float timeMove;
    void testMoving()
    {
        //if(speed < 30) {
        //    increaseSpeed();
        //}

        //if(trackController.checkDistance(transform.position)) {
        //    float ang = RotationCounter.getNewDirection(transform.position, trackController.getCurrentMarker(), transform.rotation.eulerAngles.y);
        //    turner.setTarget(ang, transform.rotation.eulerAngles.y);
        //}


        //Vector3 point1 = pathElements[2][0];
        //Vector3 point2 = pathElements[2][1];
        //Vector3 point3 = pathElements[2][2];
        //Vector3 point4 = pathElements[2][3];
        //Vector3 point5 = pathElements[2][4];
        //Vector3 point6 = pathElements[2][5];
        //t += Time.deltaTime / timeMove;
        //int n = numberPoints;
        //Vector3[] points = new Vector3[n];
        //for(int i=0; i<n; i++) points[i] = pathElements[2][i];
        //transform.position = Bezier.GetPoint4(t, points);
        //transform.rotation = Quaternion.LookRotation(Bezier.GetRotation(point1, point2, point3, point4, t));
    }

    private void increaseSpeed()
    {
        if (speed >= speedMax) return;

        float speed1 = speedMax * 0.3f, speed2 = speedMax * 0.6f, speed3 = speedMax * 0.8f, speed4 = speedMax * 0.9f;
        float spInc = 0;

        if (speed < speed1) spInc = speedIncrease * 2;
        else if (speed < speed2) spInc = speedIncrease;
        else if (speed < speed3) spInc = speedIncrease * 0.5f;
        else spInc = speedIncrease * 0.2f;

        speed += spInc * Time.deltaTime;
    }
    private void decreaseSpeed()
    {
        if (speed <= 0) return;

        speed -= speedDecrease * Time.deltaTime;
    }

    private List<Transform> createBezier(List<Transform> pathElements)
    {
        List<Transform> resElements = new List<Transform>();
        List<Vector3> markersTrack = new List<Vector3>();

        for (int i = 0; i<pathElements.Count; i++) {
            markersTrack.Add(pathElements[i].position);
        }

        int n = markersTrack.Count * 10;
        Vector3[] points = markersTrack.ToArray();
        float t;
        for (int i = 0; i<n; i++) {
            t = i / n;
            Vector3 resElement = Bezier.GetPoint4(t, points);
            Transform newTransform = Instantiate(track.GetChild(i), resElement, Quaternion.identity);
            resElements.Add(newTransform);
        }

        return resElements;
    }
}

class ControlerTrack
{
    int nextMarker = 0;
    int currentPath = 2;
    float critDist = 3f;
    float pastDistance = -1;
    Vector3 currentMarker = new Vector3();

    List<Vector3>[] markersTrack = new List<Vector3>[4];

    public void setMarkersTrack(List<Vector3> markers, int n)
    {
        if (n < 0 || n >= markersTrack.Length || markers == null) return;
        markersTrack[n] = markers;
        if(n == markersTrack.Length - 1) currentMarker = markersTrack[currentPath][nextMarker];
    }

    public void setCurrentPath(int n)
    {
        if(n >= 0 && n < markersTrack.Length) {
            currentPath = n;
        }
    }

    public bool checkDistance(Vector3 carPos)
    {
        bool isNext = false;
        float dist = Vector3.Distance(carPos, currentMarker);

        if(pastDistance == -1f) pastDistance = dist;

        if ((dist < critDist || dist > (pastDistance + critDist)) && nextMarker < (markersTrack[currentPath].Count - 1)) {
            currentMarker = markersTrack[currentPath][++nextMarker];
            pastDistance = Vector3.Distance(carPos, currentMarker);;
            isNext = true;
        }

        if (dist < pastDistance && !isNext) pastDistance = dist;

        return isNext;
    }

    public Vector3 getCurrentMarker()
    {
        return currentMarker;
    }
}

class Turner
{
    public enum Turn { None, Left, Right };

    public Turn turn = Turn.None;
    float maxSpeed = 12f;
    public float turnSpeed = 1f;
    float targetAngle = 0;
    public float currentAngle;
    bool isTransitionLeft = false;
    bool isTransitionRight = false;
    float lastAng;

    public void setTarget(float tarAng, float curAng)
    {
        if (tarAng < 0) {
            turn = Turn.Left;
        } else {
            turn = Turn.Right;
        }

        targetAngle = curAng + tarAng;

        if(targetAngle >= 360 || targetAngle < 0) {
            if (targetAngle >= 360) {
                targetAngle -= 360;
                isTransitionRight = true;
            }
            if (targetAngle < 0) {
                targetAngle += 360;
                isTransitionLeft = true;
            }
        }

        float dif = Mathf.Abs(tarAng);
        turnSpeed = (dif / 90) * maxSpeed;
        //Debug.Log("Set: " + turn + " : " + tarAng + " : " + curAng + ". Target: " + targetAngle + " : " + turnSpeed);
    }

    public void setStartAngle(float ang)
    {
        targetAngle = ang;
        lastAng = ang;
    }

    public float getTarget()
    {
        return currentAngle;
    }

    public bool checkReady(float ang)
    {
        bool isReady = false;

        if(isTransitionLeft && ang > lastAng) {
            isTransitionLeft = false;
        }
        else if (isTransitionRight && ang < lastAng) {
            isTransitionRight = false;
        }
        lastAng = ang;

        if (turn == Turn.Left) {
            if (ang <= targetAngle && !isTransitionLeft) {
                isReady = true;
                turn = Turn.None;
                //Debug.Log("Ready: " + ang + " : " + targetAngle);
            } 
            else currentAngle = ang - 20;
        }
        else if (turn == Turn.Right) {
            if (ang >= targetAngle && !isTransitionRight) {
                isReady = true;
                turn = Turn.None;
                //Debug.Log("Ready: " + targetAngle);
            } 
            else currentAngle = ang + 20;
        }

        if (currentAngle >= 360 || currentAngle < 0) {
            if (currentAngle >= 360) currentAngle -= 360;
            if (currentAngle < 0) currentAngle += 360;
        }

        return isReady;
    }
}

class SimpleTimer
{
    float timer = 1.0f;
    bool isStart = false;

    public void start(float time)
    {
        timer = time;
        isStart = true;
    }

    public bool isStarting()
    {
        return isStart;
    }

    public bool isProcess()
    {
        if (!isStart) return false;

        if (timer > 0) {
            timer -= Time.deltaTime;
            return false;
        } else {
            isStart = false;
            return true;
        }
    }
}

class TimerCollision
{
    float timer = 1.0f;
    public GameObject collisionObj = null;

    public TimerCollision(GameObject collObj)
    {
        collisionObj = collObj;
    }

    public bool isProcess()
    {
        if (timer > 0) {
            timer -= Time.deltaTime;
            return false;
        } else {
            return true;
        }
    }
}
