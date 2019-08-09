using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace QuestVR_MP
{
    public class BallSpawn : MonoBehaviourPun
    {
        public GameObject ball;

        public void CreateBall()
        {
            PhotonNetwork.InstantiateSceneObject(ball.name, new Vector3(0f, 1f, 0f), Quaternion.identity);
        }
    }
}
