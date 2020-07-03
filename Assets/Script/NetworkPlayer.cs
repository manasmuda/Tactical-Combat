using UnityEngine;

public class NetworkPlayer
{
    //private GameObject character;
    private bool localPlayer;
    int playerId;

    public NetworkPlayer(int playerId)
    {
        this.playerId = playerId;
    }

    public void DeleteGameObject()
    {
       
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    // This is called for the local player only
    public void Initialize(GameObject characterPrefab, Vector3 pos)
    {
        // Create character
        //Quaternion rotation = Quaternion.identity;
        //this.character = GameObject.Instantiate(characterPrefab, pos, rotation);
        this.localPlayer = true;
    }

    // *** FOR SENDING MESSAGES (LOCAL PLAYER) *** //

    /*public SimpleMessage GetSpawnMessage()
    {
        SimpleMessage message = new SimpleMessage(MessageType.Spawn,playerId.ToString());
        //Vector3 pos = this.character.transform.position;
        //Quaternion rotation = this.character.transform.rotation;
        return message;
    }*/

}