using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestVR_MP
{
    public class BallSpawn : MonoBehaviour
    {
        public GameObject ball;

        public void CreateBall()
        {
            Instantiate(ball, new Vector3(0f, 1f, 0f), Quaternion.identity);
        }
    }
}
