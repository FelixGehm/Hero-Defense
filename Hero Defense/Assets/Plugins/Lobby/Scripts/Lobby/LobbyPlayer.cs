using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Prototype.NetworkLobby
{
    public enum CharacterEnum { Gunslinger, Mage, Test1, Test2 };


    public class LobbyPlayer : NetworkLobbyPlayer
    {
        public InputField nameInput;
        public Button readyButton;
        public Button waitingPlayerButton;
        public Button removePlayerButton;

        public Dropdown characterDropdown;

        public GameObject localIcone;
        public GameObject remoteIcone;

        //OnMyName function will be invoked on clients when server change the value of playerName
        [SyncVar(hook = "OnMyName")]
        public string playerName = "WerDasLiestIstDoof";

        [SyncVar(hook = "OnMyChar")]
        public CharacterEnum playerChar = CharacterEnum.Gunslinger;



        public Color OddRowColor = new Color(250.0f / 255.0f, 250.0f / 255.0f, 250.0f / 255.0f, 1.0f);
        public Color EvenRowColor = new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f, 1.0f);

        static Color JoinColor = new Color(255.0f/255.0f, 0.0f, 101.0f/255.0f,1.0f);
        static Color NotReadyColor = new Color(34.0f / 255.0f, 44 / 255.0f, 55.0f / 255.0f, 1.0f);
        static Color ReadyColor = new Color(0.0f, 204.0f / 255.0f, 204.0f / 255.0f, 1.0f);
        static Color TransparentColor = new Color(0, 0, 0, 0);


        public override void OnClientEnterLobby()
        {
            base.OnClientEnterLobby();

            if (LobbyManager.s_Singleton != null) LobbyManager.s_Singleton.OnPlayersNumberModified(1);

            LobbyPlayerList._instance.AddPlayer(this);
            LobbyPlayerList._instance.DisplayDirectServerWarning(isServer && LobbyManager.s_Singleton.matchMaker == null);

            if (isLocalPlayer)
            {
                SetupLocalPlayer();
            }
            else
            {
                SetupOtherPlayer();
            }

            //setup the player data on UI. The value are SyncVar so the player
            //will be created with the right value currently on server
            OnMyName(playerName);
            OnMyChar(playerChar);

            Debug.Log("I am the OnCLientENterLobby Function");
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            //if we return from a game, color of text can still be the one for "Ready"
            readyButton.transform.GetChild(0).GetComponent<Text>().color = Color.white;

           SetupLocalPlayer();
        }

        void ChangeReadyButtonColor(Color c)
        {
            ColorBlock b = readyButton.colors;
            b.normalColor = c;
            b.pressedColor = c;
            b.highlightedColor = c;
            b.disabledColor = c;
            readyButton.colors = b;
        }

        void SetupOtherPlayer()
        {
            nameInput.interactable = false;
            characterDropdown.interactable = false;

            removePlayerButton.interactable = NetworkServer.active;

            ChangeReadyButtonColor(NotReadyColor);
            
            readyButton.transform.GetChild(0).GetComponent<Text>().text = "...";
            readyButton.interactable = false;

            Debug.Log("PlayerChar = " + playerChar);

            OnClientReady(false);
        }

        void SetupLocalPlayer()
        {
            nameInput.interactable = true;
            characterDropdown.interactable = true;

            remoteIcone.gameObject.SetActive(false);
            localIcone.gameObject.SetActive(true);

            CheckRemoveButton();
                        

            ChangeReadyButtonColor(JoinColor);

            readyButton.transform.GetChild(0).GetComponent<Text>().text = "JOIN";
            readyButton.interactable = true;

            //have to use child count of player prefab already setup as "this.slot" is not set yet
            if (playerName == "")
                CmdNameChanged("Player" + (LobbyPlayerList._instance.playerListContentTransform.childCount-1));
            

            //we switch from simple name display to name input            
            nameInput.interactable = true;

            nameInput.onEndEdit.RemoveAllListeners();
            nameInput.onEndEdit.AddListener(OnNameChanged);
                        
                        
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(OnReadyClicked);

            //when OnClientEnterLobby is called, the loval PlayerController is not yet created, so we need to redo that here to disable
            //the add button if we reach maxLocalPlayer. We pass 0, as it was already counted on OnClientEnterLobby
            if (LobbyManager.s_Singleton != null) LobbyManager.s_Singleton.OnPlayersNumberModified(0);
        }

        //This enable/disable the remove button depending on if that is the only local player or not
        public void CheckRemoveButton()
        {
            if (!isLocalPlayer)
                return;

            int localPlayerCount = 0;
            foreach (UnityEngine.Networking.PlayerController p in ClientScene.localPlayers)
                localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

            removePlayerButton.interactable = localPlayerCount > 1;
        }

        public override void OnClientReady(bool readyState)
        {
            if (readyState)
            {
                ChangeReadyButtonColor(TransparentColor);

                Text textComponent = readyButton.transform.GetChild(0).GetComponent<Text>();
                textComponent.text = "READY";
                textComponent.color = ReadyColor;
                readyButton.interactable = false;
                nameInput.interactable = false;

                characterDropdown.interactable = false;
            }
            else
            {
                ChangeReadyButtonColor(isLocalPlayer ? JoinColor : NotReadyColor);

                Text textComponent = readyButton.transform.GetChild(0).GetComponent<Text>();
                textComponent.text = isLocalPlayer ? "JOIN" : "...";
                textComponent.color = Color.white;
                readyButton.interactable = isLocalPlayer;
                nameInput.interactable = isLocalPlayer;

                characterDropdown.interactable = isLocalPlayer;
            }
        }

        public void OnPlayerListChanged(int idx)
        { 
            GetComponent<Image>().color = (idx % 2 == 0) ? EvenRowColor : OddRowColor;            
        }

        ///===== callback from sync var

        public void OnMyName(string newName)
        {
            playerName = newName;
            nameInput.text = playerName;
        }


        public void OnMyChar(CharacterEnum newChar)
        {
            Debug.Log("OnMyChar() (Hook)");

            playerChar = newChar;
            characterDropdown.value = (int)newChar;
        }

        //===== UI Handler

        //Note that those handler use Command function, as we need to change the value on the server not locally
        //so that all client get the new value throught syncvar
        public void OnReadyClicked()
        {
            SendReadyToBeginMessage();
        }

        public void OnNameChanged(string str)
        {
            CmdNameChanged(str);
        }

        public void OnCharChanged(int newCharacter)
        {
            CmdCharChanged((CharacterEnum) newCharacter);
        }

        public void OnRemovePlayerClick()
        {
            if (isLocalPlayer)
            {
                RemovePlayer();
            }
            else if (isServer)
            {
                LobbyManager.s_Singleton.KickPlayer(connectionToClient);
            }   
        }

        public void ToggleJoinButton(bool enabled)
        {
            readyButton.gameObject.SetActive(enabled);
            waitingPlayerButton.gameObject.SetActive(!enabled);
        }

        [ClientRpc]
        public void RpcUpdateCountdown(int countdown)
        {
            LobbyManager.s_Singleton.countdownPanel.UIText.text = "Match Starting in " + countdown;
            LobbyManager.s_Singleton.countdownPanel.gameObject.SetActive(countdown != 0);
        }

        [ClientRpc]
        public void RpcUpdateRemoveButton()
        {
            CheckRemoveButton();
        }

        //====== Server Command
        

        [Command]
        public void CmdNameChanged(string name)
        {
            playerName = name;
        }

        [Command]
        public void CmdCharChanged(CharacterEnum character)
        {
            Debug.Log("CmdCharChanged()");
            playerChar = character;
        }

        //Cleanup thing when get destroy (which happen when client kick or disconnect)
        public void OnDestroy()
        {
            LobbyPlayerList._instance.RemovePlayer(this);
            if (LobbyManager.s_Singleton != null) LobbyManager.s_Singleton.OnPlayersNumberModified(-1);
        }
    }
}
