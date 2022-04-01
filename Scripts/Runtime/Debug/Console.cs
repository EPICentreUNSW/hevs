using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HEVS
{
    /// <summary>
    /// A console / terminal that can be accessed via the tilde ` key on a master node.
    /// </summary>
    [RequireComponent(typeof(ClusterObject))]
    public class Console : MonoBehaviour
    {
        class Line
        {
            public enum Type { output, input, message, warning, error };
            public Type type;
            public string text;

            public Line(Type type, string text)
            {
                this.type = type;
                this.text = text;
            }
        }

        /// <summary>
        /// Flag for where to place the console.
        /// </summary>
        public enum Place 
        { 
            /// <summary>
            /// Place at the lower part of the display.
            /// </summary>
            Low,
            /// <summary>
            /// Place in the middle of the display.
            /// </summary>
            Middle, 
            /// <summary>
            /// Place near the top of the display.
            /// </summary>
            High
        };

        /// <summary>
        /// Singleton access for the console.
        /// </summary>
        public static Console singleton { get; private set; }

        /// <summary>
        /// Delegate for a command entry.
        /// </summary>
        /// <param name="messageArray">The arguments for the command.</param>
        public delegate void Command(string[] messageArray);

        /// <summary>
        /// Delegate for a token entry.
        /// </summary>
        /// <returns>Returns the string representing the token.</returns>
        public delegate string Token(); 

        /// <summary>
        /// Key that opens the console.
        /// </summary>
        public KeyCode consoleKey = KeyCode.BackQuote;

        /// <summary>
        /// Key to submit a command to the console.
        /// </summary>
        public KeyCode registerKey = KeyCode.Return;

        /// <summary>
        /// key for deleting a character from the console.
        /// </summary>
        public KeyCode deleteKey = KeyCode.Backspace;

        /// <summary>
        /// Key for auto completing commands.
        /// </summary>
        public KeyCode completeKey = KeyCode.Tab;

        /// <summary>
        /// key for scrolling the console up.
        /// </summary>
        public KeyCode scrollUpKey = KeyCode.UpArrow;

        /// <summary>
        /// key for scrolling the console down.
        /// </summary>
        public KeyCode scrollDownKey = KeyCode.DownArrow;

        /// <summary>
        /// is the console open or not.
        /// </summary>
        public bool isOpen { get; private set; } = false;
        Canvas canvas;
        Text text;

        /// <summary>
        /// Dictionary of messages attached to the console.
        /// </summary>
        public static Dictionary<string, string> messages { get; private set; } = new Dictionary<string, string>();

        List<Line> lineList = new List<Line>();
        List<Line> logList = new List<Line>();

        /// <summary>
        /// Current number of warnings in the console.
        /// </summary>
        public int logWarningCount { get; private set; } = 0;
        /// <summary>
        /// Current number of errors in the console.
        /// </summary>
        public int logErrorCount { get; private set; } = 0;
        /// <summary>
        /// Current number of messages in the console.
        /// </summary>
        public int logMessageCount { get; private set; } = 0;

        /// <summary>
        /// List of line entries in the console.
        /// </summary>
        public List<string> inputList { get; private set; } = new List<string>();

        /// <summary>
        /// The current incomplete input string / command.
        /// </summary>
        public string input { get; private set; }

        /// <summary>
        /// Dictionary of commands registered with the console.
        /// </summary>
        public static Dictionary<string, Command> commands { get; private set; } = new Dictionary<string, Command>();

        /// <summary>
        /// Dictionary of token registered with the console.
        /// </summary>
        public static Dictionary<string, Token> tokens { get; private set; } = new Dictionary<string, Token>();

        float caretTime = 0;
        bool caretBlink = true;

        /// <summary>
        /// Caret blink rate in seconds.
        /// </summary>
        public float caretBlinkRate = 0.25f;

        /// <summary>
        /// Character representing the caret.
        /// </summary>
        public string caret = "_";

        /// <summary>
        /// How many lines to display in the console.
        /// </summary>
        public int visibleLines = 5;

        /// <summary>
        /// Maximum number of lines kept in the console before they are discarded.
        /// </summary>
        public int maximumStoredLines = 1000;

        /// <summary>
        /// Font the console uses.
        /// </summary>
        public string font = "Consolas";

        const string defaultFont = "Consolas";

        /// <summary>
        /// Font size for the console.
        /// </summary>
        public int fontSize = 20;

        /// <summary>
        /// Is the console on the master only.
        /// </summary>
        public bool masterConsole { get; private set; } = true;

        /// <summary>
        /// Is the console positioned in world space instead of screen space.
        /// </summary>
        public bool worldCanvas { get; private set; } = false;

        /// <summary>
        /// Where to position the console.
        /// </summary>
        public Place place = Place.Low;

        bool showFPS = false;
        bool showNetStats = false;

        int scrollIndex = 0;

        private void Awake()
        {
            if(singleton)
            {
                Debug.Log("HEVS: Only one console component nedeed - destroying Console on: " + gameObject.name);
                Destroy(this);
            }
            else
            {
                singleton = this;

                //Register Capture Log function Callback so Unity's Debug Logs and Errors can be recorded.
                UnityEngine.Application.logMessageReceived += CaptureLog;

                //Add default tokens
                AddToken("node", TokenNodeName);
                AddToken("realtime", TokenTimeSinceStartup);
                AddToken("datetime", TokenDateTime);
                AddToken("version", TokenHEVSVersion);

                //Add default commands
                AddCommand("listcommands", ShowCommands);
                AddCommand("listtokens", ShowTokens);
                AddCommand("showlog", ShowLog);
                AddCommand("log", AddLog);
                AddCommand("open", OpenNode);
                AddCommand("close", CloseNode);
                AddCommand("clearconsole", ClearConsole);
                AddCommand("clearlog", ClearLog);
                AddCommand("setconsole", SetConsole);
                AddCommand("setfont", SetConsoleFont);
                AddCommand("setfontsize", SetConsoleFontSize);
                AddCommand("setmasterconsole", SetMasterConsole);
                AddCommand("setnodeconsole", SetNodeConsole);
                AddCommand("spawn", Spawn);
                AddCommand("destroy", Destroy);
                AddCommand("listobjects", ListClusterObjects);
                AddCommand("fps", ToggleFPS);
                AddCommand("netstat", ToggleNetStats);
            }
        }

        private void Start()
        {
            string[] argumentArray = System.Environment.GetCommandLineArgs();
            foreach(string argument in argumentArray)
            {
                string[] splitArray = argument.Split('-');
                if( (splitArray.Length > 0) && (splitArray[0] == "console") )
                {
                    foreach(string command in splitArray.Skip(1).ToArray())
                    {
                        input = command;
                        RegisterInput();
                    }
                }
            }
        }

        private void Update()
        {
            if (isOpen)
            {
                if (masterConsole)
                {
                    if(HEVS.Cluster.isMaster)
                    {
                        UpdateInput();
                        UpdateText();
                        UpdateSync();
                    }
                }
                else
                {
                    UpdateInput();
                    UpdateText();
                }
            }
            else
            {
                if (UnityEngine.Input.GetKeyDown(consoleKey))
                    Open();
            }

            if (showFPS)
            {
                string timer = "";
                timer += "FPS (s): " + System.Math.Ceiling(Time.fps).ToString();// "0000.000");
                AddMessage("FPS", timer);
            }

            if (showNetStats)
            {
                string timer = "NetStats: ";
                timer += "Pre-Sync (ms|bytes) [" + Time.Profiler.preSync.ToString("000.0000") + "|" + Cluster.preSyncPacketSize +  "] ";
                timer += "Post-Sync (ms|bytes) [" + Time.Profiler.postSync.ToString("000.0000") + "|" + Cluster.postSyncPacketSize + "] ";
                timer += "Frame-Sync (ms) [" + Time.Profiler.frameSync.ToString("000.0000") + "] ";
                timer += "RPC Call Counts (all|master) [" + RPC.numberOfCallsLastFrame + "|" + RPC.numberOfMasterCallsLastFrame + "]";
                AddMessage("NETSTAT", timer);
            }
        }

        /// <summary>
        /// Listen for key input relating to the console and update as necessary. Should be called from Update().
        /// </summary>
        void UpdateInput()
        {
            if (UnityEngine.Input.GetKeyDown(consoleKey))
                Close();
            else if (UnityEngine.Input.GetKeyDown(registerKey))
                RegisterInput();
            else if (UnityEngine.Input.GetKeyDown(deleteKey))
            {
                if (input.Length > 0)
                {
                    input = input.Remove(input.Length - 1);
                }

                ResetScroll();
            }
            else if (UnityEngine.Input.GetKeyDown(scrollUpKey))
                Scroll(-1);
            else if (UnityEngine.Input.GetKeyDown(scrollDownKey))
                Scroll(1);
            else if (UnityEngine.Input.GetKeyDown(completeKey))
            {
                CompleteInput();
            }
            else if (UnityEngine.Input.inputString.Length > 0)
            {
                input += UnityEngine.Input.inputString;
                ResetScroll();
            }
        }

        /// <summary>
        /// Update an open console with the text needed for this frame. Should be called from Update().
        /// </summary>
        void UpdateText()
        {
            text.text = "";
            if (masterConsole)
                text.text = "MASTER CONSOLE\n";
            else
                text.text = "NODE CONSOLE --- " + HEVS.Core.activeNode.id +"\n"; 

            foreach(string key in messages.Keys)
            {
                string message = messages[key];
                foreach (string token in tokens.Keys)
                    message = message.Replace(token, tokens[token].Invoke());
                text.text += message + "\n";
            }

            if (text.text.Length > 0)
                text.text += "\n----------\n";

            if ((logErrorCount + logWarningCount + logMessageCount) > 0)
            {
                if (logErrorCount > 0)
                    text.text += " <color=red>Errors: " + logErrorCount + "</color>";
                if (logWarningCount > 0)
                    text.text += " <color=yellow>Warnings: " + logWarningCount + "</color>";
                if (logMessageCount > 0)
                    text.text += " Messages: " + logMessageCount;

                text.text += "\n----------\n";
            }   

            for (int count = Mathf.Max(0, lineList.Count - visibleLines); count < lineList.Count; count++)
            {
                Line line = lineList[count];
                if (line.type == Line.Type.input)
                    text.text += "> ";

                text.text += line.text + "\n";
            }

            text.text += "> " + input;

            caretTime -= HEVS.Time.deltaTime;
            if (caretTime <= 0)
            {
                caretBlink = !caretBlink;
                caretTime = caretBlinkRate;
            }

            if (caretBlink)
                text.text += caret;

            text.text += "\n";

        }

        /// <summary>
        /// Send Master console's text in an RPC to all clients, ensuring the Master console appears accross the cluster.
        /// Should be called from Update().
        /// </summary>
        void UpdateSync()
        {
            HEVS.RPC.Call(RPCSyncText, text.text);
        }

        /// <summary>
        /// RPC called from UpdateSync()
        /// </summary>
        /// <param name="consoleText">Text displayed on console, should be from Master console's text.</param>
        [HEVS.RPC]
        void RPCSyncText(string consoleText)
        {
            if(!HEVS.Cluster.isMaster && isOpen && text)
                text.text = consoleText; 
        }

        /// <summary>
        /// Auto-complete for the input currently in the console. Searches commands for best fit.
        /// If one result, sets input to it but does not register. If multiple, options are written to console.
        /// Called from User Input, default Tab key. 
        /// </summary>
        void CompleteInput()
        {
            List<string> choiceList = new List<string>();
            foreach(string key in commands.Keys)
            {
                if (key.Contains(input.ToLower()))
                    choiceList.Add(key);
            }

            if (choiceList.Count == 1)
                input = choiceList[0];
            else if (choiceList.Count > 1)
            {
                string output = "Suggestions: ";
                foreach (string choice in choiceList)
                    output += " " + choice;
                WriteLine(output);
            }
        }

        /// <summary>
        /// Processes the current input string. First word is used to identify a command, and the rest are passed as arguments.
        /// Called after User presses register key, default Enter. 
        /// </summary>
        void RegisterInput()
        {
            if (input.Length == 0)
                return;

            inputList.Add(input);
            while (inputList.Count > maximumStoredLines)
                inputList.RemoveAt(0);

            WriteLine(Line.Type.input, input);

            string[] commandArray = input.Split(' ');
            string command = commandArray[0].ToLower();
            string[] messageArray = commandArray.Skip(1).ToArray();
            if (commands.ContainsKey(command))
            {
                for(int index = 0; index < messageArray.Length; index++)
                {
                    foreach (string token in tokens.Keys)
                        messageArray[index] = messageArray[index].Replace(token, tokens[token].Invoke());
                }

                commands[command].Invoke(messageArray);
            }
            else if (command == "rpc")
            {
                SendRPC("all", messageArray);
            }
            else if (command == "client" || command == "clients")
            {
                SendRPC("client", messageArray);
            }
            else if(command[0] == '@')
            {
                SendRPC(command.Remove(0, 1), messageArray);
            }
            else
            {
                WriteLine(Line.Type.output, "Command not recognised");
            }


            input = "";
        }

        /// <summary>
        /// Open the console. It will appear on screen or in world, and listen for input.
        /// </summary>
        public void Open()
        {
            if (isOpen)
                return;

            isOpen = true;

            if (canvas)
                canvas.gameObject.SetActive(true);
            else
                CreateCanvas();

            input = "";
        }

        /// <summary>
        /// Close the console.
        /// </summary>
        public void Close()
        {
            if (!isOpen)
                return;

            isOpen = false;
            canvas.gameObject.SetActive(false);
        }

        /// <summary>
        /// Instantaites a new canvas for the console to appear on. 
        /// Canvas will be screen or world space, according to public bool worldCanvas.
        /// </summary>
        void CreateCanvas()
        {
            if(canvas)
                Destroy(canvas.gameObject);

            float width;
            float height;
            Vector2 anchor;

            GameObject canvasObject = new GameObject("Console Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvas.worldCamera = HEVS.Camera.main;

            if (worldCanvas)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.position = HEVS.Camera.main.transform.position + (HEVS.Camera.main.transform.forward * 1);
                canvas.transform.rotation = HEVS.Camera.main.transform.rotation;
                canvas.transform.SetParent(HEVS.Camera.main.transform);

                width = 1000;
                height = (((float)fontSize / 2) + 5) * visibleLines;
                anchor = new Vector2(0.5f, 0.5f);

                float scale = 0.001f; 
                canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(width, width);
                canvas.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                width = Screen.width;
                height = (((float)fontSize / 2) + 5) * visibleLines;
                if (place == Place.High)
                    anchor = new Vector2(0, 1);
                else if (place == Place.Middle)
                    anchor = new Vector2(0, 0.5f);
                else
                    anchor = new Vector2(0, 0);
            }

            GameObject panelObject = new GameObject("Console Panel");
            Image panel = panelObject.AddComponent<Image>();
            panel.color = Color.black;
            panel.transform.SetParent(canvas.transform, false);
            panel.rectTransform.anchorMin = anchor;
            panel.rectTransform.anchorMax = anchor;
            panel.rectTransform.pivot = anchor;
            panel.rectTransform.anchoredPosition = new Vector2(0, 0);
            panel.rectTransform.sizeDelta = new Vector2(width, height);
            VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(25, 25, 25, 25);
            layout.spacing = 5;
            ContentSizeFitter fitter = panelObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject textObject = new GameObject("Console Text");
            text = textObject.AddComponent<Text>();
            text.transform.SetParent(panel.transform, false);
            text.rectTransform.anchorMin = anchor;
            text.rectTransform.anchorMax = anchor;
            text.rectTransform.pivot = anchor;
            text.rectTransform.anchoredPosition = anchor;
            text.rectTransform.sizeDelta = new Vector2(width, height);
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.fontSize = fontSize; 

            string[] validFonts = Font.GetOSInstalledFontNames();
            if (!validFonts.Contains<string>(font))
                font = defaultFont; 

            text.font = Font.CreateDynamicFontFromOSFont(font, fontSize);
        }

        /// <summary>
        /// Write a line to the console. Entire string passed will appear as a single entry.
        /// Line will by displayed as output. 
        /// </summary>
        /// <param name="message">The message to add to the console.</param>
        public static void WriteLine(string message)
        {
            singleton.WriteLine(Line.Type.output, message);
        }

        /// <summary>
        /// Write a line to console. Type flags line as input or output. 
        /// </summary>
        /// <param name="type">The type of line to add to the console.</param>
        /// <param name="message">The message to add to the console.</param>
        void WriteLine(Line.Type type, string message)
        {
            lineList.Add(new Line(type, message));

            while (lineList.Count > maximumStoredLines)
                lineList.RemoveAt(0);

            ResetScroll();
        }

        /// <summary>
        /// Callback for capturing Unity's logs. Registered during Awake().
        /// Should be called whenever Unity sends a message to its own console.
        /// Messages are divided into different lists for errors, warnings and messages.
        /// </summary>
        /// <param name="logString"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        void CaptureLog(string logString, string stackTrace, LogType type)
        {
            logString = "[" + System.DateTime.Now.ToString() + "] " + logString ;
            Line.Type lineType;


            if( (type == LogType.Exception) || (type == LogType.Assert) || (type == LogType.Error) )
            {
                lineType = Line.Type.error; 
                logString = "<color=red>" + logString + "</color>";
                logErrorCount++;
            }
            else if (type == LogType.Warning)
            {
                lineType = Line.Type.warning; 
                logString = "<color=yellow>" + logString + "</color>";
                logWarningCount++;
            }
            else
            {
                lineType = Line.Type.message;
                logMessageCount++;
            }

            Line line = new Line(lineType, logString);
          
            logList.Add(line);

        }

        /// <summary>
        /// Scrolls through previously entered inputs. 
        /// Adjusts scrollIndex integer, then assigns that entry from inputList to the input string.
        /// scrollIndex will not go below zero, or above the mamimum index of inputList.
        /// </summary>
        /// <param name="scroll">Amount to adjust int scrollIndex by</param>
        void Scroll(int scroll)
        {
            if (inputList.Count == 0)
                return;

            scrollIndex += scroll;
            if (scrollIndex < 0)
                scrollIndex = 0;
            else if (scrollIndex >= inputList.Count)
                scrollIndex = inputList.Count - 1;

            input = inputList[scrollIndex];
        }

        /// <summary>
        /// Resets scroll to listening for user input. ScrollIndex is set to inputList.Count, as this is one higher than the last index.
        /// </summary>
        void ResetScroll()
        {
            scrollIndex = inputList.Count;
        }

        /// <summary>
        /// Register a command delegate to the command list. 
        /// Key provided is the word entered into the console to call the command.
        /// Command is the delegate that is invoked when key is provided.
        /// Key must be unique to all other commands registered, otherwise error will be logged and command not registered.
        /// Console registers default commands during Awake(), other scripts should only call AddCommand() in Start() or later. 
        /// </summary>
        /// <param name="key">The key to match to the command to execute.</param>
        /// <param name="command">The command to execute.</param>
        public static void AddCommand(string key, Command command)
        {
            key = key.ToLower();

            if (!commands.ContainsKey(key))
                commands.Add(key, command);
            else
                Debug.LogWarning("HEVS: Command Key: \"" + key + "\" already in use");
        }

        /// <summary>
        /// Register a token to the token list.
        /// Key provided must be unique to other token keys, and is the string entered to be replaced by the token delegate provided.
        /// </summary>
        /// <param name="key">The key to match to the token.</param>
        /// <param name="token">The token to replace the key with.</param>
        public static void AddToken(string key, Token token)
        {
            key = "{"+key.ToUpper()+"}";

            if (!tokens.ContainsKey(key))
                tokens.Add(key, token);
            else
                Debug.LogWarning("HEVS: Token Key: \"" + key + "\" already in use");
        }


        /// <summary>
        /// Add a message to the top of the console. If a message with the same key already exists, it is overwritten.
        /// Messages are printed at the top of the console each frame. Frequently calling AddMessage allows messages to update in realtime.
        /// Call RemoveMessage to remove that message from the console.
        /// </summary>
        /// <param name="key">The key to match to the message.</param>
        /// <param name="token">The message to replace the key with.</param>
        public static void AddMessage(string key, string message)
        {
            if (messages.ContainsKey(key))
                messages[key] = message;
            else
                messages.Add(key, message);
        }

        /// <summary>
        /// Remove a particular message key from the message list. This will mean it is no longer displayed.
        /// </summary>
        /// <param name="key">The key for the message to remove.</param>
        public static void RemoveMessage(string key)
        {
            if (messages.ContainsKey(key))
                messages.Remove(key);
        }

        /// <summary>
        /// Send console input from Master to Clients as an RPC. Target specifies which nodes execute the input.
        /// "all" executes on all, "client" and "clients" executes only on clients. Sending the node ID executes on that specific node.
        /// This function is triggered from RegisterInput() after splitting the input string for command processing.
        /// input is therefore a string array, which is recombined by the function for sending by rpc.
        /// Nodes will recieve a the same string as entered minus the rpc keyword. 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="messageArray"></param>
        void SendRPC(string target, string[] messageArray)
        {
            if (!HEVS.Cluster.isMaster)
            {
                WriteLine("RPC must be called on Master");
                return;
            }

            string message = "";
            for (int count = 0; count < messageArray.Length; count++)
            {

                message += messageArray[count];
                if (count < (messageArray.Length - 1))
                    message += " ";
            }

            HEVS.RPC.Call(RPCSendInput, target, message);
        }

        /// <summary>
        /// The RPC sent by SendRPC(). 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        [HEVS.RPC]
        void RPCSendInput(string target, string message)
        {
            if ( target == "all" || target == HEVS.Core.activeNode.id || (target == "client" && !HEVS.Cluster.isMaster) )
            {
                if (!isOpen)
                    Open();

                input = message;
                RegisterInput();
            }
        }


        // ---- DEFAULT TOKENS ----

        string TokenNodeName()
        {
            return HEVS.Core.activeNode.id;
        }

        string TokenTimeSinceStartup()
        {
            return UnityEngine.Time.realtimeSinceStartup.ToString();
        }

        string TokenDateTime()
        {
            return System.DateTime.Now.ToString();
        }

        string TokenHEVSVersion()
        {
            return Core.VERSION;
        }

        // ---- DEFAULT COMMANDS ----

        void ShowCommands(string[] messageArray)
        {
            string output = "Command List";
            foreach(string key in commands.Keys)
            {
                output += " - "+key;
            }

            WriteLine(Line.Type.output, output);
        }

        void ShowTokens(string[] messageArray)
        {
            string output = "Token List";
            foreach(string key in tokens.Keys)
            {
                output += " - " + key;
            }

            WriteLine(Line.Type.output, output);
        }

        void OpenNode(string[] messageArray)
        {
            if(messageArray.Length == 0)
                Open();
            else
            {
                string message = messageArray[0].ToLower();
                if(HEVS.Cluster.isMaster)
                {
                    if (message == "all")
                        HEVS.RPC.Call(RPCClientConsolesOpen, true, true);
                    else if ( message == "clients" || message == "client" )
                        HEVS.RPC.Call(RPCClientConsolesOpen, true, false);
                }   
            }
        }

        void CloseNode(string[] messageArray)
        {
            if (messageArray.Length == 0)
                Close();
            else
            {
                string message = messageArray[0].ToLower();
                if(HEVS.Cluster.isMaster)
                {
                    if (message == "all")
                        HEVS.RPC.Call(RPCClientConsolesOpen, false, true);
                    else if ( message == "client" || message == "clients" )
                        HEVS.RPC.Call(RPCClientConsolesOpen, false, false);
                }
            }
        }

        [HEVS.RPC]
        void RPCClientConsolesOpen(bool open, bool all)
        {
            if (!HEVS.Cluster.isMaster || all)
            {
                if (open)
                    Open();
                else
                    Close();
            }
        }

        void ClearConsole(string[] messageArray)
        {
            lineList.Clear();
        }

        void ClearLog(string[] messageArray)
        {
            logList.Clear();
            logMessageCount = 0;
            logWarningCount = 0;
            logErrorCount = 0;
        }

        void AddLog(string[] messageArray)
        {
            string logMessage = "";
            foreach (string message in messageArray)
                logMessage += message + " ";

            Debug.Log("HEVS: " + logMessage);
        }

        
        void ShowLog(string[] messageArray)
        {
            if(logList.Count == 0)
            {
                WriteLine("Log has no entries");
            }
            else if (messageArray.Length == 0)
            {
                string output = "Log Entries\n";
                foreach (Line line in logList)
                {
                    output += line.text + "\n";
                }

                WriteLine(output);
            }
            else
            {
                bool reversed = false;
                int limit = logList.Count;
                bool errors = false;
                bool warnings = false;
                bool messages = false;
                string search = "";

                foreach (string message in messageArray)
                {
                    int tryLimit;
                    string lowMessage = message.ToLower();
                    if (lowMessage == "last")
                    {
                        reversed = true;
                        limit = 1;
                    }
                    else if (lowMessage == "first")
                    {
                        reversed = false;
                        limit = 1;
                    }
                    else if ( (lowMessage == "errors") || (lowMessage == "error") )
                        errors = true;
                    else if ( (lowMessage == "warnings") || (lowMessage == "warning") )
                        warnings = true;
                    else if ( (lowMessage == "messages") || (lowMessage == "message") )
                        messages = true;
                    else if (int.TryParse(message, out tryLimit))
                    {
                        if (tryLimit >= 0)
                            limit = tryLimit;
                    }
                    else
                        search = message.ToLower();
                }

                if(!errors && !warnings && !messages)
                {
                    errors = true;
                    warnings = true;
                    messages = true; 
                }


                List<Line> outLogList = new List<Line>();
                foreach(Line log in logList)
                {
                    if ((search.Length > 0) && !log.text.ToLower().Contains(search))
                        continue;

                    bool valid = false;
                    if (errors && (log.type == Line.Type.error))
                        valid = true;
                    else if (warnings && (log.type == Line.Type.warning))
                        valid = true;
                    else if (messages && (log.type == Line.Type.message))
                        valid = true;

                    if (!valid)
                        continue;

                    outLogList.Add(log);
                }

                int startIndex;
                int endIndex;
                if (reversed)
                {
                    endIndex = outLogList.Count;
                    startIndex = Mathf.Max(0, outLogList.Count - limit);
                }
                else
                {
                    startIndex = 0;
                    endIndex = Mathf.Min(limit, outLogList.Count);
                }

               
                string output = "";
                for (int index = startIndex; index < endIndex; index++)
                    output += outLogList[index].text + "\n";

                if (output.Length == 0)
                    output = "No matching log entries";

                string header = "Log Search\n";

                if (errors)
                    header += "errors, ";
                if (warnings)
                    header += "warnings, ";
                if (messages)
                    header += "messages, ";

                if (search.Length > 0)
                    header += "including \'" + search+"', ";


                if (reversed)
                {
                    if (limit > 1)
                        header += "last " + limit + " entries";
                    else
                        header += "last entry";
                }
                else if (limit == 1)
                {
                    header += "first entry";
                }
                else if(limit < logList.Count)
                    header += "first " + limit + " entries";


                header += "\n-----\n";

                WriteLine(header+output);
            }
        }

        void SetConsole(string[] messageArray)
        {
            string errorOutput = "";

            for (int index = 0; index < messageArray.Length; index++)
            {
                string message = messageArray[index].ToLower();

                if (message == "world")
                    worldCanvas = true;
                else if (message == "screen")
                    worldCanvas = false;
                else if (message == "high")
                {
                    place = Place.High;
                    worldCanvas = false;
                }
                else if (message == "middle")
                {
                    place = Place.Middle;
                    worldCanvas = false;
                }
                else if (message == "low")
                {
                    place = Place.Low;
                    worldCanvas = false;
                }
                else
                    errorOutput += message + " not recognised";
            }

            if (errorOutput.Length > 0)
                WriteLine(errorOutput);

            if ((errorOutput.Length > 0) || (messageArray.Length == 0))
                WriteLine("Valid commands for Set Console are: screen, world, high, middle or low");

            CreateCanvas();
        }

        void SetConsoleFont(string[] messageArray)
        {
            string fontChoice = "";
            for(int index = 0; index < messageArray.Length; index++)
            {
                fontChoice += messageArray[index];
                if (index < (messageArray.Length - 1))
                    fontChoice += " ";
            }

            string[] validFonts = Font.GetOSInstalledFontNames();
            if(validFonts.Contains<string>(fontChoice))
            {
                font = fontChoice;
                CreateCanvas();
            }
            else
            {   
                WriteLine(Line.Type.output, "Font choice - " + fontChoice + " - not available");
            }
        }

        void SetConsoleFontSize(string[] messageArray)
        {
            int size;
            if(int.TryParse(messageArray[0], out size))
            {
                fontSize = Mathf.Clamp(size, 6, 72);
                CreateCanvas();
            }
            else
            {
                WriteLine("Invalid font size");
            }
        }

        
        void SetMasterConsole(string[] messageArray)
        {
            if(HEVS.Cluster.isMaster)
                HEVS.RPC.Call(RPCSetMasterConsole, true);
        }

        void SetNodeConsole(string[] messageArray)
        {
            if(HEVS.Cluster.isMaster)
                HEVS.RPC.Call(RPCSetMasterConsole, false);
        }

        [HEVS.RPC]
        void RPCSetMasterConsole(bool isMasterConsole)
        {
            masterConsole = isMasterConsole;
        }
        
        void Spawn(string[] messageArray)
        {
            HEVS.Core application = GameObject.FindObjectOfType<HEVS.Core>();

            if (messageArray.Length < 1)
            {
                if (application.spawnablePrefabList.Count > 0)
                {
                    string output = "Available objects to spawn ";
                    foreach (ClusterObject clusterObject in application.spawnablePrefabList)
                        output += " - " + clusterObject.name;
                    WriteLine(output);
                }
                else
                    WriteLine("No objects available to spawn. Add cluster object prefabs to Application's spawnable prefab list");
                
            }
            else
            {
                string prefabName = messageArray[0];
                ClusterObject prefab = null;

                Vector3 position = HEVS.Camera.main.transform.position;
                position += HEVS.Camera.main.transform.forward * 5;
                Quaternion rotation = HEVS.Camera.main.transform.rotation;


                foreach (HEVS.ClusterObject clusterObject in application.spawnablePrefabList)
                {
                    if (clusterObject.name == prefabName)
                    {
                        prefab = clusterObject;
                        break;
                    }
                }

                if (prefab)
                {
                    WriteLine("Spawning " + prefabName);
                    HEVS.Cluster.Spawn(prefab.gameObject, position, rotation);
                }
                else
                {
                    WriteLine("Object " + prefabName + " not found on Application's spawnable prefab list.");
                }
            }

        }

        void ListClusterObjects(string[] messageArray)
        {
            string line = "Cluster Objects ";

            foreach(int key in HEVS.Cluster.clusterObjects.Keys)
            {
                line += " - "+key + ") " + HEVS.Cluster.clusterObjects[key].name;
            }

            WriteLine(line);
        }

        void ToggleFPS(string[] messageArray)
        {
            showFPS = !showFPS;
            if (!showFPS)
                RemoveMessage("FPS");
        }

        void ToggleNetStats(string[] messageArray)
        {
            showNetStats = !showNetStats;
            if (!showNetStats)
                RemoveMessage("NETSTAT");
        }

        void Destroy(string[] messageArray)
        {
            if(messageArray.Length == 0)
            {
                WriteLine("You must provide an object name or cluster id to destroy");
                return;
            }

            int id;
            if(int.TryParse(messageArray[0], out id))
            {
                if (HEVS.Cluster.clusterObjects.ContainsKey(id))
                    Destroy(HEVS.Cluster.clusterObjects[id].gameObject);
                else
                    WriteLine("Cluster ID " + id + " not found");
            }
            else
            {
                GameObject target = GameObject.Find(messageArray[0]);
                if (target)
                    Destroy(target);
                else
                    WriteLine("GameObject " + messageArray[0] + " not found");
            }
        }
    }
}

