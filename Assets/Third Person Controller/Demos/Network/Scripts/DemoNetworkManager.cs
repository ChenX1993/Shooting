using UnityEngine;
using UnityEngine.Networking;

namespace Opsive.ThirdPersonController.Demos.Networking
{
    /// <summary>
    /// Demo component which spawns the player in a unique location.
    /// </summary>
    public class DemoNetworkManager : NetworkEventManager
    {
        /// <summary>
        /// Spawn the player in a unique location to prevent it overlapping with another character.
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            var spawnPosition = Vector3.right * conn.connectionId;
            var player = GameObject.Instantiate(playerPrefab, spawnPosition, Quaternion.identity) as GameObject;
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

#if UNITY_5_1 || UNITY_5_2
            // The new connection isn't active yet so check against 0 connections to determine if the server just started and objects can spawn.
            if (NetworkServer.connections.Count == 0) {
                EventHandler.ExecuteEvent("OnNetworkAddFirstPlayer");
            }
#else
            if (NetworkServer.connections.Count == 1) {
                EventHandler.ExecuteEvent("OnNetworkAddFirstPlayer");
            }
#endif
        }
    }
}