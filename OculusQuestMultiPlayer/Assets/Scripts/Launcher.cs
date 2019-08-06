using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace OculusMP
{
    /// <summary>
    /// Launcher class handles initial Photon networking method calls, allowing the user to automatically connect to the Photon network
    /// and join a room.
    /// </summary>
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private and Public Attributes
        private string gameVersion = "1";       // Leave set to 1 by default, unless we need to make breaking changes on a project that is Live.

        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        public byte maxPlayersPerRoom;

        private bool isConnecting;

        #endregion

        #region General Methods
        private void Awake()
        {
            // Critical
            // This makes sure we can use PhotonNetwork.LoadLevel() on the master client 
            // and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            // Auto connect to lobby
            Connect();
        }

        /// <summary>
        /// This method is called automatically when the user runs the application
        /// </summary>
        public void Connect()
        {
            // Set connecting flag - we are wanting to connect
            isConnecting = true;

            // Check if we are connected to Photon Network (server) and join a random room
            if (PhotonNetwork.IsConnected)
            {
                // Critical
                // First attempt to join a random room
                // If this fails, we'll get notified in OnJoinRandomFailed() callback and we'll create a room.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // Critical
                // Connect to the Photon Network (server) 
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();       // Set on PhotonServerSettings in unity editor
            }
        }
        #endregion

        #region Photon Callbacks
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster() was called by PUN");

            // Check if we are wanting to connect (prevent looping when we disconnect from a room)
            if (isConnecting)
            {
                // Critical
                // Attempt to join a potential existing room.
                // If unsuccessful, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause.ToString());
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we'll create one.\n calling PhotonNetwork.CreateRoom()");

            // Critical
            // We failed to join a random room (room may not exist or room may be already full)
            // So, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            //const string VR_ROOM = "HYDAC_Scene";

            Debug.Log("OnJoinedRoom() was called by PUN. Now this client is in a room.");
            Debug.Log(PhotonNetwork.ServerAddress);

            // Critical
            // We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` 
            // to sync our instance scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("Loading VR Room");

                // Critical
                // Load the Room Level.
                //PhotonNetwork.LoadLevel(RoomName.SceneName);
                PhotonNetwork.LoadLevel("VR_Room");
            }
        }
        #endregion
    }
}