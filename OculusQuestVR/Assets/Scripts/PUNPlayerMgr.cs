using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace QuestVR_MP
{
    public class PUNPlayerMgr : MonoBehaviourPun, IPunObservable
    {
        [Tooltip("The local player instance. Use this to know if local player is represented in the scene")]
        public static GameObject LocalPlayerInstance;

        public Text playerTypeTxt;

        // VR Elements
        private Transform localVRHeadset;
        private Transform localVRControllerLeft;
        private Transform localVRControllerRight;

        [Tooltip("The avatar representing the player's head attached to the VR headset")]
        public GameObject Head;

        [Tooltip("The avatar representing the player's left hand controller")]
        public GameObject LeftHand;

        [Tooltip("The avatar representing the player's right hand controller")]
        public GameObject RightHand;

        //[Header("VR Laser Pointer")]
        //[Tooltip("The avatar representing the laser pointer, activated with the right hand controller")]
        //public LaserPointer rightHandLaser;

        // Smoothing Variables For Remote Player's Motion
        private Vector3 correctPlayerHeadPosition = Vector3.zero;
        private Quaternion correctPlayerHeadRotation = Quaternion.identity;
        private Vector3 correctPlayerLeftHandPosition = Vector3.zero;
        private Quaternion correctPlayerLeftHandRotation = Quaternion.identity;
        private Vector3 correctPlayerRightHandPosition = Vector3.zero;
        private Quaternion correctPlayerRightHandRotation = Quaternion.identity;

        private bool triggerPulled;
        private bool touchPadActivated;

        //private GameObject _Turntable;

        private void Awake()
        {
            // Important:
            // used in RoomManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronised
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                localVRHeadset = GameObject.Find("CenterEyeAnchor").transform;                 // Get transform data from local VR Headset
                localVRControllerLeft = GameObject.Find("CustomHandLeft").transform;
                localVRControllerRight = GameObject.Find("CustomHandRight").transform;

                //rightHandLaser.transform.SetParent(localVRControllerRight);
                //rightHandLaser.transform.localPosition = Vector3.zero;

                // Don't display our own avatar to ourselves    
                Head.SetActive(false);
                LeftHand.SetActive(false);
                RightHand.SetActive(false);
            }

            // Critical
            // Don't Destroy on load to prevent player from being destroyed when another player joins / leaves the room
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //Debug.Log("I am the master client");
                playerTypeTxt.text = "Master Client";
            }
            else
            {
                //Debug.Log("I am the remote client");
                playerTypeTxt.text = "Remote Client";
            }

            if (photonView.IsMine)
            {
                Debug.Log("I'm instantiated...!!!");
            }
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                //// Controller 'Trigger' is activated
                //if (triggerAction.GetState(handType))
                //{
                //    //Debug.Log("Trigger pressed...");
                //    if (triggerPulled) // only want to tell the laser to activate once
                //        return;

                //    triggerPulled = true;

                //    rightHandLaser.TriggerPulled();
                //}
                //// Controller de-activated
                //else
                //{
                //    triggerPulled = false;
                //}

                // TO DO: Attempt to transfer ownership between players (WIP)
                //if (touchPadAction.GetStateDown(handType))
                //{
                //    
                //    // Check if we are the current owner of the Turntable object 
                //    Debug.Log(_Turntable.GetComponent<PhotonView>().Owner + " : " + PhotonNetwork.LocalPlayer);

                //    if (_Turntable.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                //    {
                //        //RotateTable.isActivated = touchPadAction.GetState(handType);
                //        RotateTable.isActivated = !RotateTable.isActivated;
                //    }
                //    else
                //    {
                //        // Transfer ownership to ourselves
                //        _Turntable.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                //        //RotateTable.isActivated = touchPadAction.GetState(handType);
                //        RotateTable.isActivated = !RotateTable.isActivated;
                //    }
                //}
            }
            else if (!photonView.IsMine)
            {
                // Smooth Remote player's motion on local machine
                SmoothPlayerMotion(ref Head, ref correctPlayerHeadPosition, ref correctPlayerHeadRotation);
                SmoothPlayerMotion(ref LeftHand, ref correctPlayerLeftHandPosition, ref correctPlayerLeftHandRotation);
                SmoothPlayerMotion(ref RightHand, ref correctPlayerRightHandPosition, ref correctPlayerRightHandRotation);
            }
        }

        /// <summary>
        /// Applies LERP interpolation to smooth the remote player's game object motion over the network. 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="gameObjectCorrectTransformPosition"></param>
        /// <param name="gameObjectCorrectTransformRotation"></param>
        private void SmoothPlayerMotion(ref GameObject gameObject, ref Vector3 gameObjectCorrectTransformPosition, ref Quaternion gameObjectCorrectTransformRotation)
        {
            // Smoothing variables
            float distance = Vector3.Distance(gameObject.transform.position, gameObjectCorrectTransformPosition);
            const int SMOOTHING_FACTOR = 5;
            const float SMOOTH_DISTANCE = 2f;

            // Apply smoothing
            if (distance < SMOOTH_DISTANCE)
            {
                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, gameObjectCorrectTransformPosition, Time.deltaTime * SMOOTHING_FACTOR);
                gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, gameObjectCorrectTransformRotation, Time.deltaTime * SMOOTHING_FACTOR);
            }
            else
            {
                gameObject.transform.position = gameObjectCorrectTransformPosition;
                gameObject.transform.rotation = gameObjectCorrectTransformRotation;
            }
        }

        /// <summary>
        /// Controls the exchange of data between local and remote player's VR data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Send local VR Headset position and rotation data to networked player
                stream.SendNext(localVRHeadset.position);
                stream.SendNext(localVRHeadset.rotation);
                stream.SendNext(localVRControllerLeft.position);
                stream.SendNext(localVRControllerLeft.rotation);
                stream.SendNext(localVRControllerRight.position);
                stream.SendNext(localVRControllerRight.rotation);
            }
            else if (stream.IsReading)
            {
                // Receive networked player's VR Headset position and rotation data
                correctPlayerHeadPosition = (Vector3)stream.ReceiveNext();
                correctPlayerHeadRotation = (Quaternion)stream.ReceiveNext();
                correctPlayerLeftHandPosition = (Vector3)stream.ReceiveNext();
                correctPlayerLeftHandRotation = (Quaternion)stream.ReceiveNext();
                correctPlayerRightHandPosition = (Vector3)stream.ReceiveNext();
                correctPlayerRightHandRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
