## `AnaglyphCameraExtension`

A HEVS camera extension for outputting stereo anaglyph.
```csharp
public class HEVS.AnaglyphCameraExtension
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `AnaglyphType` | anaglyphType |  | 
| `Texture` | leftEye |  | 
| `Texture` | rightEye |  | 


## `AnaglyphType`

```csharp
public enum HEVS.AnaglyphType
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | RedGreen |  | 
| `1` | RedBlue |  | 
| `2` | RedCyan |  | 


## `AspectScaleExtension`

Camera component that applies a scale to the camera's projection matrix aspect ratio. This is useful for dealing with stereoscopic or CAVE displays where the display's aspect ratio isn't suitable due to viewport sizes.
```csharp
public class HEVS.AspectScaleExtension
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | scale | The scale to apply to the projection transform. | 


## `Camera`

A helper for accessing the stored main camera.
```csharp
public class HEVS.Camera

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<Camera>` | displayCameras | Cameras created by HEVS to represents the current node's displays. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Color` | backgroundColor | Set the background clear colour for all of the active node's display cameras. | 
| `CameraClearFlags` | clearFlags | Set the camera clear flags for all of the active node's display cameras. | 
| `Camera` | main | Access to the original MainCamera, which will be disabled for most platforms but will have all display cameras  attached to it as children in the scene hierarchy. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Camera` | ConfigureCaptureCamera(`GameObject` parent, `Display` display, `Int32` depth, `StereoTargetEyeMask` eye) | Configures a camera to capture the scene based on settings from a specified HEVS DisplayConfig read from a JSON configuration file. | 
| `void` | ConfigureCaptureCamera(`Camera` camera, `Display` display, `StereoTargetEyeMask` eye) | Configures a camera to capture the scene based on settings from a specified HEVS DisplayConfig read from a JSON configuration file. | 
| `Camera[]` | ConfigureOutputCameras(`Display` display, `Camera` left, `Camera` right) |  | 
| `void` | ConfigureViewportForCamera(`Camera` camera, `Display` display, `Boolean` leftEye = True) | Configures camera to output to a specified display and viewport. | 
| `void` | ConfigureWarpAndBlend(`String` warpPath, `String` blendPath, `GameObject` left, `GameObject` right = null) |  | 


## `CameraBehaviour`

The base MonoBehaviour used for HEVS Camera extensions.
```csharp
public abstract class HEVS.CameraBehaviour
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Display` | display | Access to the HEVS DisplayConfig used by this camera. | 


## `CameraExtension`

A MonoBehaviour that can be used as a base class for Camera behaviours that need to  be copied from the Main Camera onto HEVS scene-capture camera rigs.
```csharp
public abstract class HEVS.CameraExtension
    : MonoBehaviour

```

## `CanvasManager`

Canvas Manager is attached to a UI canvas to make its UI elements interactable with a HEVS Pointer.
```csharp
public class HEVS.CanvasManager
    : MonoBehaviour

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | IsLookingAtRect(`Pointer` pointer, `RectTransform` rectTransform) | Check if pointer is pointing at a point inside rectTransform | 
| `Vector2` | PointerRectPosition(`Pointer` pointer, `RectTransform` rectTransform) | Finds the relative position of a pointer's pickRay inside the provided rectTransform. | 


## `Cluster`

The Cluster controller. This is a static class that controls the sockets and the broadcasting of cluster data. It also controls frame synchronisation.
```csharp
public class HEVS.Cluster

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | _nextClusterID |  | 
| `Dictionary<Int32, ClusterObject>` | clusterObjects | Collection of instantiated ClusterObject's, stored by their cluster ID. | 
| `Boolean` | enableDataBroadcast |  | 
| `Double` | fpsUpdateRate | The rate at which the FPS is updated per-second. | 
| `PostUpdateDeserializeDataDelegate` | PostUpdateDeserializationDelegate | Delegate for deserializing post-update data. | 
| `PostUpdateSerializeDataDelegate` | PostUpdateSerializationDelegate | Delegate for serializing post-update data. | 
| `PreUpdateDeserializeDataDelegate` | PreUpdateDeserializationDelegate | Delegate for deserializing pre-update data. | 
| `PreUpdateSerializeDataDelegate` | PreUpdateSerializationDelegate | Delegate for serializing pre-update data. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | active | Are the cluster sockets active. | 
| `Int32` | clientCount | The number of client nodes within the current platform. This number is equivalent to the total number of nodes, minus 1 for the master. | 
| `Int32` | clientDropoutCount | The number of clients which have dropped out of the cluster since startup. | 
| `Single` | deltaTime | The current Unity delta-time of the cluster's master. | 
| `Double` | fps | The current node's frames-per-second (FPS). | 
| `Int32` | frameCount | The current Unity frame count of the cluster's master. | 
| `Double` | frameSyncTimeMS | The time it takes a master to sync the cluster frame swap, or a client to receive and complete a frame swap, in milliseconds. | 
| `List<GameObject>` | gameObjectsSpawnedLastUpdateList | A list of all gameobject's spawned last update. | 
| `Boolean` | isClient |  | 
| `Boolean` | isCluster | Is the current platform running as a cluster. Will always report false when running within the Unity editor. | 
| `Boolean` | isMaster | Is this node the cluster's master node. | 
| `String` | masterAddress |  | 
| `Int32` | nextClusterID |  | 
| `Int32` | numberOfActiveClients | Current number of active client nodes. | 
| `Int32` | postSyncPacketSize | The size of the most recent post-sync packet (transforms, variables, RPC) in bytes. | 
| `Double` | postSyncTimeMS | The time it takes a master to complete the post-sync broadcast, or a client to receive the post-sync broadcast, in milliseconds. | 
| `Int32` | preSyncPacketSize |  | 
| `Double` | preSyncTimeMS | The time it takes a master to complete the pre-sync broadcast, or a client to receive the pre-sync broadcast, in milliseconds. | 
| `Single` | time | The current Unity time of the cluster's master. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | BroadcastData(`Int32` packetHeader, `Byte[]` data) |  | 
| `void` | ChangeClusterObjectState(`Int32` clusterID, `State` state) |  | 
| `void` | ChangeClusterObjectStates() |  | 
| `void` | DeregisterDataBroadcastReceiver(`DataBroadcastReceiver` receiver) |  | 
| `void` | DeregisterObject(`Int32` clusterID) |  | 
| `void` | DeregisterObject(`ClusterObject` obj) |  | 
| `void` | DeregisterSpawnFactoryMethod(`Int32` typeID) | Deregister a registered spawn handler for a specified type ID. | 
| `void` | DeserializeClusterObjectStates(`ByteBufferReader` reader) |  | 
| `void` | FrameSync() |  | 
| `void` | Initialise(`Cluster` config, `List<ClusterObject>` configurationSpawnablePrefabList, `Int32` randomSeed = 42, `Boolean` clusterInEditor = False) | Initialise the cluster.  Called from HEVSApplication Awake(). | 
| `void` | InitialisePhysics() | Initialises physics for the cluster by potentially disabling RigidBodies on clients,  and adding ClusterObjects so that physics objects sync from the master to clients. | 
| `void` | PostUpdate() | Broadcast ClusterTransforms, ClusterVariables and RPC, spawn objects, sync the frame. | 
| `void` | PreUpdate() | Broadcast time and input state for the following frame. | 
| `void` | RegisterDataBroadcastReceiver(`DataBroadcastReceiver` receiver) |  | 
| `void` | RegisterObject(`Int32` clusterID, `ClusterObject` obj) |  | 
| `void` | RegisterSpawnFactoryMethod(`Int32` typeID, `SpawnHandler` func) | Registers are spawn handler with the cluster, for an associated ID. | 
| `void` | RemoveSpawnedObjects() |  | 
| `void` | ResetState() | Remove cluster objects, spawned objects, and reset cluster ID. | 
| `void` | SerializeClusterObjectStates(`ByteBufferWriter` writer) |  | 
| `void` | Shutdown() | Close cluster sockets. | 
| `Int32` | Spawn(`Int32` typeID, `Object[]` arguments) | Called on the master node only. Creates a GameObject and passes it into a register SpawnHandler factory method. | 
| `Int32` | Spawn(`GameObject` prefab) | Called on the master node only. Creates a GameObject and passes it into a register SpawnHandler factory method. | 
| `Int32` | Spawn(`GameObject` prefab, `Vector3` position) | Called on the master node only. Creates a GameObject and passes it into a register SpawnHandler factory method. | 
| `Int32` | Spawn(`GameObject` prefab, `Vector3` position, `Quaternion` rotation) | Called on the master node only. Creates a GameObject and passes it into a register SpawnHandler factory method. | 


## `ClusterObject`

HEVS component that tags a GameObject as an object within a clustered environment.  The GameObject is given a cluster ID and can have its Transform synchronised from  the master node to all client nodes. The GameObject can also synchronise state  changes, such as when the GameObject is activated/deactivated/destroyed.
```csharp
public class HEVS.ClusterObject
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | clusterID | The unique cluster ID of this GameObject. | 
| `Int32` | transformFlags | Flags specifying which parts of the transform will be synched from the master to all clients. | 
| `Boolean` | updateStateOnClients | Flag for specifying if the GameObject's state should be synchronised. | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | parentChanged | Check if the GameObject's parent has changed since the last time we checked. | 
| `Int32` | parentID | The cluster ID of the GameObject's parent (must also contain a ClusterObject component). | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | SetClientState(`Int32` newState) | On a client, update a ClusterObject's state to match that of the Master. | 


## `ClusterVar<T>`

Generic Cluster Variable container.
```csharp
public class HEVS.ClusterVar<T>
    : ClusterVariable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `T` | GetValue() | Get the value of the variable this frame. | 


## `ClusterVariable`

Base class for variables that can be synchronised within a cluster from the master node to all client nodes.
```csharp
public abstract class HEVS.ClusterVariable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | isValid | Return true if this variable is valid and registered with HEVS internal systems. | 
| `String` | name | The name of the clustered variable. The name is used to identify the variable within the cluster. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Finalize() | Cleanup the variable. | 
| `Boolean` | GetValue(`Object&` value) | Return the current value of the variable. If the value has recently been changed with a call to SetValue() then  the returned variable will still return the old value and wont return the new value until after this frame's Update(). | 
| `void` | SetValue(`Object` value) | Set the value of the object. The new value wont take effect until the end of the frame. | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, RegisteredData>` | registeredVariables | A collection of registered variables. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CleanDirtyVariables() | Updates all registered ClusterVariables to their most recently assigned value. | 
| `void` | ClearRegisteredVariables() | Deregisters all ClusterVariables. | 
| `void` | DeserializeDirtyVariables(`ByteBufferReader` reader) | Deserialize updated ClusterVariable data from a buffer and apply the changes to the variables. | 
| `void` | SerializeDirtyVariables(`ByteBufferWriter` writer) | Serializes all registered ClusterVariables that have had their values updated, writing their ID, value type, and new value. | 


## `Config`

```csharp
public class HEVS.Config

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, Object>` | globals | Access to the globals, parsed from the JSON config file. | 
| `JSONNode` | json |  | 
| `String` | jsonPath |  | 
| `Dictionary<String, Platform>` | platforms | Access to all currently loaded platforms. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | FindPlatformAndNode(`String` platformName, `String` hostName, `Platform&` platform, `Node&` node, `Boolean` useDefaultIfNotFound = True) |  | 
| `Boolean` | ParseConfig(`String` path) |  | 


## `Console`

```csharp
public class HEVS.Console
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | caret |  | 
| `Single` | caretBlinkRate |  | 
| `KeyCode` | completeKey |  | 
| `KeyCode` | consoleKey |  | 
| `KeyCode` | deleteKey |  | 
| `String` | font |  | 
| `Int32` | fontSize |  | 
| `String` | input |  | 
| `List<String>` | inputList |  | 
| `Boolean` | isOpen |  | 
| `List<Line>` | lineList |  | 
| `Int32` | logErrorCount |  | 
| `List<Line>` | logList |  | 
| `Int32` | logMessageCount |  | 
| `Int32` | logWarningCount |  | 
| `Boolean` | masterConsole |  | 
| `Int32` | maximumStoredLines |  | 
| `Place` | place |  | 
| `KeyCode` | registerKey |  | 
| `KeyCode` | scrollDownKey |  | 
| `KeyCode` | scrollUpKey |  | 
| `Boolean` | showFPS |  | 
| `Boolean` | showNetStats |  | 
| `Int32` | visibleLines |  | 
| `Boolean` | worldCanvas |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Close() | Close the console. | 
| `void` | Open() | Open the console. It will appear on screen or in world, and listen for input. | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, Command>` | commands |  | 
| `Dictionary<String, String>` | messages |  | 
| `Console` | single |  | 
| `Dictionary<String, Token>` | tokens |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | AddCommand(`String` key, `Command` command) | Register a command delegate to the command list.  Key provided is the word entered into the console to call the command.  Command is the delegate that is invoked when key is provided.  Key must be unique to all other commands registered, otherwise error will be logged and command not registered.  Console registers default commands during Awake(), other scripts should only call AddCommand() in Start() or later. | 
| `void` | AddMessage(`String` key, `String` message) | Add a message to the top of the console. If a message with the same key already exists, it is overwritten.  Messages are printed at the top of the console each frame. Frequently calling AddMessage allows messages to update in realtime.  Call RemoveMessage to remove that message from the console. | 
| `void` | AddToken(`String` key, `Token` token) | Register a token to the token list.  Key provided must be unique to other token keys, and is the string entered to be replaced by the token delegate provided. | 
| `void` | RemoveMessage(`String` key) | Remove a particular message key from the message list. This will mean it is no longer displayed. | 
| `void` | WriteLine(`String` message) | Write a line to the console. Entire string passed will appear as a single entry.  Line will by displayed as output. | 


## `Core`

The HEVS Application components. This components acts as the "brain" for HEVS, tying together all systems.
```csharp
public class HEVS.Core
    : MonoBehaviour, IRPCInterface

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | clusterInEditor |  | 
| `String` | configFile | The path to the JSON file used for initialising HEVS. This can be overridden via command lines arguments or environment variables. | 
| `Boolean` | debugDrawCurrentOnly | An editor flag for displaying only the current node's Gizmos. | 
| `Boolean` | debugImpersonateNode | A flag specifying if HEVS should ignore the hostname of the running instance and instead  use the name of a selected node to impersonate. | 
| `Int32` | oscPort |  | 
| `List<OSCReceivers>` | oscReceivers |  | 
| `Boolean` | quitOnEscape |  | 
| `Int32` | randomSeed | The random seed used to initialise all nodes within the cluster.  This aids in enabling the use of random, however it is still reliant on calls  being made on the master and clients in the same order to maintain synchronisation.  It is recommended that you only use random on a master and then synchronise the result. | 
| `List<ClusterObject>` | spawnablePrefabList | A registered list of prefabs that can be instantiated by the Cluster. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | OnOSCReceived(`Object` sender, `OscMessageReceivedEventArgs` args) |  | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Camera` | mainCamera |  | 
| `PreUpdate` | OnPreUpdate | The PreUpdate delegate. | 
| `String` | VERSION |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<Display>` | activeDisplays |  | 
| `Node` | activeNode |  | 
| `Platform` | activePlatform |  | 
| `Config` | config |  | 
| `GameObject` | coreGameObject |  | 
| `Boolean` | isActive |  | 
| `Vector3Int` | nodeIndex |  | 
| `List<Display>` | platformDisplays |  | 
| `Display` | primaryActiveDisplay |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Mesh` | GenerateMeshForActiveNode() | Will generate a Unity Mesh for the active node's displays. | 
| `Mesh` | GenerateMeshForActivePlatform() | Will generate a Unity Mesh for all displays within the active platform, even if no node uses them. | 
| `Ray` | ScreenPointToRay(`Vector3` point) |  | 


## `CurvedDisplay`

Display class for projectors projecting onto a curved surface with a large field-of-view.  Options are read from the JSON configuration and then used to setup a display rig.  Note: The warp/blend configuration for this type of display is currently under development and will change in a future build.
```csharp
public class HEVS.CurvedDisplay
    : Display

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | angleEnd | The ending/right angle of the display, in degrees. | 
| `Single` | angleStart | The starting/left angle of the display, in degrees. | 
| `Single` | blackLevel | The black-level threshold. | 
| `String` | blackLevelPath | The optional file path location of the black-level texture mask used to help combat overbrightness from overlapping projectors. | 
| `Single` | groundOffset | The ground offset of the display, in meters. The entire display is offset upwards by this amount. | 
| `Single` | heading | The heading of this display, which is the angle half way between the startAngle and endAngle. | 
| `Single` | height | The height of the display, in meters. | 
| `Single` | projectorAngle | The total angle that this display represents, in degrees. | 
| `Single` | radius | The radius of a cylinder that this curved display would belong to. | 
| `Single` | renderTargetStretchFactor | Scale used to stretch the RenderTexture that this display used. | 
| `Single` | sliceAngle | The total angle that each individual slice takes up, in degrees. | 
| `Int32` | slices | The number of slices used to construct the display.  This is used to combat artefacts from rendering large fields-of-view. | 
| `String` | warpMeshPath | The optional file path location of the warp information used for this display.  If using a third-party warp/blend solution then this is not needed. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Display` | Clone() | Creates a cloned copy of this display. | 
| `void` | ConfigureDisplayForScene() | Sets up the Unity scene to use this display. | 
| `void` | GatherDisplayGeometry(`List<Vector3>` vertices, `List<Int32>` indices, `List<Vector2>` uvs) |  | 
| `Boolean` | Raycast(`Ray` ray, `Single&` distance, `Vector2&` hitPoint2D) | Cast a world-space ray into this display and return if there is an  intersection, along with distance to the intersection and the  relative 2D intersection point on the display. Only applies to displays  that exist within world-space. | 
| `void` | SetBackgroundColour(`Color` colour) |  | 
| `void` | SetClearFlags(`CameraClearFlags` flags) |  | 
| `Ray` | ViewportPointToRay(`Vector2` displayspacePoint) | Get a world-space ray from the display via a 2D display-space point. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | DrawGizmo(`Display` config) | A method to draw the display's Gizmo within the editor. | 


## `CurvedProjectorCameraExtension`

A HEVS camera extension used to apply black-level adjustments and post-effects to curved displays.  Black-level adjustment is used to reduce the colour discrepencies when projecting black with overlapping projectors.  Note: currently uses a mesh to apply warping for curved displays. This will be replaced with UV offsets for post-effects.
```csharp
public class HEVS.CurvedProjectorCameraExtension
    : CameraBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | blackLevel | The black-level threshold. | 
| `Texture2D` | blackLevelTexture | The black-level texture mask used for handling overlap brightness for multiple projectors. | 
| `Texture2D` | blendTexture | The Texture2D used for blending the output of this projector with others. | 
| `RenderTexture` | renderTexture |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | SetupAVIEProjector(`Int32` projectorID) |  | 


## `CurvedSliceCameraExtension`

A HEVS camera extension behaviour that adjusts the camera's view and projection transforms to capture a slice from a curved display.
```csharp
public class HEVS.CurvedSliceCameraExtension
    : CameraBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | sliceIndex | The index of this camera slice, out of the total slices that this display should use. | 


## `CustomDisplayAttribute`

Attribute used for creating custom display types that can be loaded from HEVS JSON configuration files.
```csharp
public class HEVS.CustomDisplayAttribute
    : Attribute, _Attribute

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | typeName | Name of the type read from the JSON configuration file. | 


## `CustomInputSourceAttribute`

Attribute used for creating custom input sources that can be loaded from HEVS JSON configuration files.
```csharp
public class HEVS.CustomInputSourceAttribute
    : Attribute, _Attribute

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | typeName | Name of the type read from the JSON configuration file. | 


## `CustomTrackerAttribute`

Attribute used for creating custom tracker types that can be loaded from HEVS JSON configuration files.
```csharp
public class HEVS.CustomTrackerAttribute
    : Attribute, _Attribute

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | typeName | Name of the type read from the JSON configuration file. | 


## `DataBroadcastReceiver`

```csharp
public abstract class HEVS.DataBroadcastReceiver
    : MonoBehaviour

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | OnDataReceived(`Int32` packetHeader, `Byte[]` data) |  | 


## `Display`

Base class for HEVS displays. Can be used for creating derived custom display types.
```csharp
public abstract class HEVS.Display

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | aspectScale | The aspect scale for the display. This is useful for displays that use a single projector/screen for multiple displays (or stereoscopic) and need to adjust the aspect to combat squashed aspects. | 
| `String` | blendPath | When using custom projector blending you can specify the location of the blend information that the projectors will use. | 
| `List<Camera>` | captureCameras | Access to the camera components created for this display to capture the scene. | 
| `Display` | config |  | 
| `Single` | eyeSeparation | The eye separation for this display's cameras.  Will use the data from the StereoConfig, or an overridden value. | 
| `Single` | farClip | The far plane distance that will be used for the cameras this display requires. Default = -1 (use the values from the Unity scene instead) | 
| `Single` | fov | The Field-of-View for the display. | 
| `GameObject` | gameObject | This display's GameObject within the scene heirarchy. | 
| `String` | id | The ID of the display. | 
| `Vector3Int` | index | A 3D integer-based "index" for the display. This is entirely user-defined and can be used to identifying displays within a grid layout. | 
| `LayerMask` | layerMask | The layermask for the display's cameras. | 
| `Int32` | monitor | The display adapter (connected output monitor/projector) that this display outputs to. | 
| `Single` | nearClip | The near plane distance that will be used for the cameras this display requires. Default = -1 (use the values from the Unity scene instead) | 
| `List<Camera>` | outputCameras | Access to the camera components created for this display to output the scene. | 
| `Camera` | primaryCaptureCamera | Access to the primary camera used to capture the scene for this display. | 
| `Camera` | primaryOutputCamera | Access to the primary camera used to output the captured display. | 
| `Boolean` | requiresDualCameras |  | 
| `Rect` | screenRect | Access to the viewport rectangle that this display uses. | 
| `StereoAlignment` | stereoAlignment | THe stereo alignment for this screen. Either Screen-aligned or Camera-aligned.  Will use the data from the StereoConfig, or an overridden value. | 
| `Stereo` | stereoConfig | The current platform's StereoConfig object. | 
| `StereoMode` | stereoMode | The stereo mode that this display uses.  Will use the data from the StereoConfig, or an overridden value. | 
| `Boolean` | swapEyes |  | 
| `Transform` | transform | An optional transform that is applied to the display's cameras and GameObjects when it is initialised. | 
| `String` | type |  | 
| `Viewport` | viewport | Access to the display's viewport config, or null if no custom viewport is set. | 
| `String` | warpPath | When using custom warp you can specify the location of the warp data that the projectors will use. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Display` | Clone() | Clones this display. | 
| `void` | ConfigureDisplayForScene() | Method for setting up the custom display. | 
| `void` | Finalize() |  | 
| `void` | GatherDisplayGeometry(`List<Vector3>` vertices, `List<Int32>` indices, `List<Vector2>` uvs) | A method to gather a triangle mesh to represent the surface of the display. | 
| `Mesh` | GenerateMeshForDisplay() |  | 
| `void` | InitialiseDisplay(`GameObject` gameObject = null) | Initialises a GameObject for the display then configures required componenents and behaviours. | 
| `Boolean` | Raycast(`Ray` ray, `Single&` distance, `Vector2&` hitPoint2D) | A method to cast a world-space ray into the screen to find a world-space hit, along with a 2D display-space point in the display. | 
| `void` | SetBackgroundColour(`Color` colour) | A method for setting the background clear colour for this display's cameras. | 
| `void` | SetClearFlags(`CameraClearFlags` flags) | A method for setting the camera clear flags for this display's cameras. | 
| `Ray` | ViewportPointToRay(`Vector2` displaySpacePoint) | Get a world-space ray from the display via a 2D display-space point. | 
| `Boolean` | ViewportPointToRay(`Vector2` viewspacePoint, `Ray&` ray) | Get a world-space ray from the display via a 2D display-space point. | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, Type>` | registeredDisplayTypes |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Display` | CreateDisplay(`Display` display) |  | 
| `void` | DrawGizmoForDisplay(`Display` config) |  | 
| `void` | GatherCustomDisplayTypes() |  | 


## `DomeDisplay`

Display class for Hemispherical Domes.  Options are read from the JSON configuration and then used to setup a display rig.  Note: the warp/blend settings are under development and will change in a future build.
```csharp
public class HEVS.DomeDisplay
    : Display

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | borderPercent | The size of the warp/blend border smoothing. | 
| `Vector2Int` | fisheyeResolution | The resolution of the RenderTexture used for capturing the fisheye image for the dome. | 
| `Vector2Int` | layout | The 2D layout of the projectors when using a single display to output to multiple pojectors. | 
| `Int32` | projectorCount | The number of projectors used to display the dome. | 
| `Int32[]` | projectorIDs | The projector IDs that this display uses. For example, for a dome that uses 8 total  projectors to display the dome, display A might use projectors 1, 2, 3 and 4, while  display B uses projectors 5, 6, 7 and 8. | 
| `Single` | radius | The physical radius of the dome. | 
| `Boolean` | smoothBorder | Should the warp/blend use border smoothing. | 
| `String` | viosoPath | When using VIOSO Anyblend, you can specify the path to the VIOSO vwf which HEVS can use  to configure the warp/blend for dome projectors.  See https://vioso.com/ for more details. | 
| `String` | warpMeshPath | When using custom warp you can specify the location of warp mesh csv data that the projectors will use. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Display` | Clone() | Creates a cloned copy of this display. | 
| `void` | ConfigureDisplayForScene() | Sets up the Unity scene to use this display. | 
| `void` | GatherDisplayGeometry(`List<Vector3>` vertices, `List<Int32>` indices, `List<Vector2>` uvs) |  | 
| `Boolean` | Raycast(`Ray` ray, `Single&` distance, `Vector2&` hitPoint2D) | Cast a world-space ray into this display and return if there is an  intersection, along with distance to the intersection and the  relative 2D intersection point on the display. Only applies to displays  that exist within world-space. | 
| `void` | SetBackgroundColour(`Color` colour) |  | 
| `void` | SetClearFlags(`CameraClearFlags` flags) |  | 
| `Ray` | ViewportPointToRay(`Vector2` displayspacePoint) | Get a world-space ray from the display via a 2D display-space point. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | DrawGizmo(`Display` config) | A method to draw the display's Gizmo within the editor. | 


## `DomeOutputCameraExtension`

A HEVS camera extension for outputting a fisheye capture, primarily for dome displays.
```csharp
public class HEVS.DomeOutputCameraExtension
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | aspectScale | Aspect ratio scale applied to width of the display. | 
| `Texture2D` | blendTexture | The texture used to specify overlap blend regions. | 
| `Single` | border | A percentage value specifying texture border thickness. | 
| `RenderTexture` | bottomTexture | The down quadrant capture. | 
| `RenderTexture` | leftTexture | The left quadrant capture. | 
| `RenderTexture` | rightTexture | The right quadrant capture. | 
| `RenderTexture` | topTexture | The top quadrant capture. | 
| `Texture2D` | warpTexture | The texture use to specify UV offsets for handling curved surface warping. | 


## `Ease`

Basic library of easing functions.  Includes classic tween using t, b, c and d,  and includes simplified 1D variants for modifying a delta [0,1] range.  Also includes some utility coroutines.
```csharp
public class HEVS.Ease

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | backIn(`Single` t, `Single` b, `Single` c, `Single` d) | Back-In easing function. | 
| `Single` | backIn(`Single` t) | Back-In easing function. | 
| `Single` | backInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Back-In-Out easing function. | 
| `Single` | backInOut(`Single` t) | Back-In-Out easing function. | 
| `Single` | backOut(`Single` t, `Single` b, `Single` c, `Single` d) | Back-Out easing function. | 
| `Single` | backOut(`Single` t) | Back-Out easing function. | 
| `Single` | bounceIn(`Single` t, `Single` b, `Single` c, `Single` d) | Bounce-In easing function. | 
| `Single` | bounceIn(`Single` t) | Bounce-In easing function. | 
| `Single` | bounceInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Bounce-In-Out easing function. | 
| `Single` | bounceInOut(`Single` t) | Bounce-In-Out easing function. | 
| `Single` | bounceOut(`Single` t, `Single` b, `Single` c, `Single` d) | Bounce-Out easing function. | 
| `Single` | bounceOut(`Single` t) | Bounce-Out easing function. | 
| `Single` | circIn(`Single` t, `Single` b, `Single` c, `Single` d) | Circular-In easing function. | 
| `Single` | circInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Circular-In-Out easing function. | 
| `Single` | circOut(`Single` t, `Single` b, `Single` c, `Single` d) | Circular-Out easing function. | 
| `Single` | cubicIn(`Single` t, `Single` b, `Single` c, `Single` d) | Cubic-In easing function. | 
| `Single` | cubicIn(`Single` t) | Cubic-In easing function. | 
| `Single` | cubicInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Cubic-In-Out easing function. | 
| `Single` | cubicInOut(`Single` t) | Cubic-In-Out easing function. | 
| `Single` | cubicOut(`Single` t, `Single` b, `Single` c, `Single` d) | Cubic-Out easing function. | 
| `Single` | cubicOut(`Single` t) | Cubic-Out easing function. | 
| `Single` | elasticIn(`Single` t, `Single` b, `Single` c, `Single` d) | Elastic-In easing function. | 
| `Single` | elasticIn(`Single` t) | Elastic-In easing function. | 
| `Single` | elasticInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Elastic-In-Out easing function. | 
| `Single` | elasticInOut(`Single` t) | Elastic-In-Out easing function. | 
| `Single` | elasticOut(`Single` t, `Single` b, `Single` c, `Single` d) | Elastic-Out easing function. | 
| `Single` | elasticOut(`Single` t) | Elastic-Out easing function. | 
| `Single` | expIn(`Single` t, `Single` b, `Single` c, `Single` d) | Exponential-In easing function. | 
| `Single` | expIn(`Single` t) | Exponential-In easing function. | 
| `Single` | expInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Exponential-In-Out easing function. | 
| `Single` | expInOut(`Single` t) | Exponential-In-Out easing function. | 
| `Single` | expOut(`Single` t, `Single` b, `Single` c, `Single` d) | Exponential-Out easing function. | 
| `Single` | expOut(`Single` t) | Exponential-Out easing function. | 
| `Single` | linearIn(`Single` t, `Single` b, `Single` c, `Single` d) | Linear-In easing function. | 
| `Single` | linearIn(`Single` t) | Linear-In easing function. | 
| `Single` | linearInOut(`Single` t) | Linear-In-Out ease function to modify a 1D delta value from 0-to-1. | 
| `Single` | linearOut(`Single` t, `Single` b, `Single` c, `Single` d) | Linear-Out easing function. | 
| `Single` | linearOut(`Single` t) | Linear-Out easing function. | 
| `IEnumerator` | position(`GameObject` gameObject, `Vector3` newPosition, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `Single` | quadIn(`Single` t, `Single` b, `Single` c, `Single` d) | Quadratic-In easing function. | 
| `Single` | quadIn(`Single` t) | Quadratic-In easing function. | 
| `Single` | quadInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Quadratic-In-Out easing function. | 
| `Single` | quadInOut(`Single` t) | Quadratic-In-Out easing function. | 
| `Single` | quadOut(`Single` t, `Single` b, `Single` c, `Single` d) | Quadratic-Out easing function. | 
| `Single` | quadOut(`Single` t) | Quadratic-Out easing function. | 
| `IEnumerator` | rotate(`GameObject` gameObject, `Vector3` euler, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | rotate(`GameObject` gameObject, `Single` X, `Single` Y, `Single` Z, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | rotateX(`GameObject` gameObject, `Single` rotation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | rotateY(`GameObject` gameObject, `Single` rotation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | rotateZ(`GameObject` gameObject, `Single` rotation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | rotation(`GameObject` gameObject, `Quaternion` newRotation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | scale(`GameObject` gameObject, `Vector3` newScale, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | scale(`GameObject` gameObject, `Single` X, `Single` Y, `Single` Z, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | scale(`GameObject` gameObject, `Single` scale, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | scaleX(`GameObject` gameObject, `Single` scale, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | scaleY(`GameObject` gameObject, `Single` scale, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | scaleZ(`GameObject` gameObject, `Single` scale, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `Single` | sineIn(`Single` t, `Single` b, `Single` c, `Single` d) | Sine-In easing function. | 
| `Single` | sineIn(`Single` t) | Sine-In easing function. | 
| `Single` | sineInOut(`Single` t, `Single` b, `Single` c, `Single` d) | Sine-In-Out easing function. | 
| `Single` | sineInOut(`Single` t) | Sine-In-Out easing function. | 
| `Single` | sineOut(`Single` t, `Single` b, `Single` c, `Single` d) | Sine-Out easing function. | 
| `Single` | sineOut(`Single` t) | Sine-Out easing function. | 
| `IEnumerator` | translate(`GameObject` gameObject, `Vector3` translation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | translate(`GameObject` gameObject, `Single` X, `Single` Y, `Single` Z, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | translateX(`GameObject` gameObject, `Single` translation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | translateY(`GameObject` gameObject, `Single` translation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | translateZ(`GameObject` gameObject, `Single` translation, `Single` duration, `Func<Single, Single, Single, Single, Single>` function) |  | 
| `IEnumerator` | tween(`Single` start, `Single` end, `Single` duration, `Func<Single, Single, Single, Single, Single>` function, `Action<Single>` tick) | Interpolate a value from start to end for a set duration, using a specific Ease method, and call a tick method each step. | 


## `FisheyeTextures`

```csharp
public struct HEVS.FisheyeTextures

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Texture` | down |  | 
| `Texture` | left |  | 
| `Texture` | right |  | 
| `Texture` | up |  | 


## `FrameLockMode`

The type of frame syncing being used.
```csharp
public enum HEVS.FrameLockMode
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Software | Software frame-locking attempts to block all nodes from continuing past their update event until all nodes are ready.  Low frame-rates can cause sync issues. | 
| `1` | Hardware | Hardware frame-locking uses specific hardware features to prevent nodes from swapping their back-buffers until they are all ready.  This requires specific hardware running under OpenGL, such as NVidia Quadro GPUs. | 


## `Graphics`

HEVS low-level native graphics API.
```csharp
public class HEVS.Graphics

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `RenderTexture` | outputRenderTexture | The render texture being used by the output cameras when utilising quad-buffering or gen-lock (hardware frame sync). | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialise() |  | 
| `void` | SetupPluginRenderTarget(`Camera` left, `Camera` right = null) |  | 
| `void` | Shutdown() |  | 
| `void` | Sync() |  | 


## `HEVSCameraExtension`

```csharp
public class HEVS.HEVSCameraExtension
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `RenderTexture` | alternateSource |  | 
| `AnaglyphType` | anaglyphType |  | 
| `Single` | aspectScale | Aspect ratio scale applied to width of the display. | 
| `Single` | blackLevel |  | 
| `Texture` | blackLevelTexture |  | 
| `Texture` | blendTexture |  | 
| `Single` | border | A percentage value specifying texture border thickness. | 
| `FisheyeTextures` | fisheyeTextures |  | 
| `Texture` | leftEye |  | 
| `Texture` | rightEye |  | 
| `WarpStereoSplit` | warpStereoSplit |  | 
| `Texture` | warpTexture |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | useAnaglyph |  | 
| `Boolean` | useBlacklevel |  | 
| `Boolean` | useDome |  | 
| `Boolean` | useWarp |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | SetAnaglyph(`StereoMode` mode, `Texture` left, `Texture` right) |  | 


## `Input`

HEVS Input wrapper class, responsible for querying input and synchronising it within a cluster.  This class should be used instead of UnityEngine.Input.
```csharp
public class HEVS.Input

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | GatherCustomSourceTypes() |  | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `InputAccumulateFunc` | AccumulateCustomInput | Delegate for injecting your own user-defined input events into the input packet before it is syncrhonised within a cluster.  This method is called on the master node after standard input has been queried, but before the packet is broadcast. | 
| `Dictionary<String, Type>` | registeredSourceTypes |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | IsCaps | True if capslock is "locked". | 
| `Boolean` | IsControl | True if any control key is "down". | 
| `Boolean` | IsShift | True if any shift key is "down". | 
| `Boolean` | IsShiftOrCaps | True if capslock is "locked" or either shift key is "down". | 
| `Vector3` | mouseDelta | The delta of the mouse cursor movement. | 
| `Vector3` | mousePosition | The position of the mouse cursor. | 
| `Vector2` | mouseScrollDelta | The delta of the mouse scrollwheel. | 
| `Int32` | touchCount | The current number of active touches. | 
| `List<Touch>` | touches | A list of all current touches. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | AccumulateAxisThisFrame(`String` axis, `Single` value) | Accumulate a value on an input axis this frame only.  Typically called within a user-defined InputAccumulateFunc(). | 
| `void` | Deserialize(`ByteBufferReader` reader) |  | 
| `void` | ForceButtonThisFrame(`String` buttonID) | Force an input button to be "down" this frame only.  Typically called within a user-defined InputAccumulateFunc(). | 
| `void` | ForceKeyThisFrame(`KeyCode` keyCode) | Force an input key to be "down" this frame only.  Typically called within a user-defined InputAccumulateFunc(). | 
| `Single` | GetAxis(`String` axis) |  | 
| `Boolean` | GetButton(`String` button) | Get the state of a button. | 
| `Boolean` | GetButtonDown(`String` button) | Query if a button is "down". | 
| `Boolean` | GetButtonUp(`String` button) | Query if a button is "up". | 
| `Boolean` | GetKey(`KeyCode` key) | Get the state of a key. | 
| `Boolean` | GetKeyDown(`KeyCode` key) | Query if a key is "down". | 
| `Boolean` | GetKeyUp(`KeyCode` key) | Query if a key is "up". | 
| `Boolean` | GetMouseButton(`Int32` button) | Query the state of a mouse button. | 
| `Boolean` | GetMouseButtonDown(`Int32` button) | Query if a mouse button is "down". | 
| `Boolean` | GetMouseButtonUp(`Int32` button) | Query if a mouse button is "up". | 
| `Touch` | GetTouch(`Int32` index) | Get a specific touch. | 
| `Boolean` | IsKeyLocked(`KeyCode` key) | Query if a lock key is "locked". | 
| `void` | Serialize(`ByteBufferWriter` writer) |  | 
| `void` | Update() |  | 


## `InputSource`

```csharp
public abstract class HEVS.InputSource

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `InputSource` | config |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | GatherDownButtons(`List<String>` buttons) |  | 
| `void` | GatherNonZeroAxes(`Dictionary<String, Single>` axes) |  | 


## `Intersection`

A utility class for simple intersection tests.
```csharp
public class HEVS.Intersection

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Vector3[]` | RayCylinderIntersection(`Ray` ray, `Vector3` cylBottom, `Vector3` cylTop, `Single` radius) | Intersect an infinite ray with a cylinder. | 
| `Vector3[]` | RaySphereIntersection(`Ray` ray, `Vector3` center, `Single` radius) | Intersect an infinite ray with a sphere. | 


## `IRPCInterface`

Interface used for registering classes, which aren't MonoBehaviours attached to GameObjects with  a ClusterObject components, as containing static RPC methods.
```csharp
public interface HEVS.IRPCInterface

```

## `OffAxisCameraExtension`

The component responsible for overriding a Camera's projection transform to enable  off-axis rendering (i.e. for CAVE-like immersive rendering).
```csharp
public class HEVS.OffAxisCameraExtension
    : CameraBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `StereoTargetEyeMask` | eye |  | 


## `OffAxisDisplay`

Display class for off-axis flat-panel displays, typical of CAVE installations.  Options are read from the JSON configuration and then used to setup a display rig.
```csharp
public class HEVS.OffAxisDisplay
    : Display

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Vector3` | center | The physical center of the display. | 
| `Vector3` | facing | The facing direction of the display. | 
| `Vector3` | ll | The physical lower-left corner of the display. | 
| `Vector3` | lr | The physical lower-right corner of the display. | 
| `Quaternion` | orientation | The orientation of the display. | 
| `Vector3` | right | The right direction of the display. | 
| `Vector2` | size | The physical width and height of the display. | 
| `Vector3` | ul | The physical upper-left corner of the display. | 
| `Vector3` | up | The up direction of the display. | 
| `Vector3` | ur | The physical upper-right corner of the display. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Display` | Clone() | Creates a cloned copy of this display. | 
| `void` | ConfigureDisplayForScene() | Sets up the Unity scene to use this display. | 
| `void` | GatherDisplayGeometry(`List<Vector3>` vertices, `List<Int32>` indices, `List<Vector2>` uvs) |  | 
| `Vector3` | GetCenter(`Vector3` offset, `Quaternion` orientation) | Get the display's center position. | 
| `Vector3` | GetFacing(`Vector3` offset, `Quaternion` orientation) | Get the display's "forward" direction. | 
| `Matrix4x4` | GetProjectionFrom(`Transform` viewer, `Single` near, `Single` far) | Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance. | 
| `Matrix4x4` | GetProjectionFrom(`Vector3` eyePos, `Single` near, `Single` far) | Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance. | 
| `Matrix4x4` | GetProjectionFrom(`Transform` viewer, `Single` near, `Single` far, `Vector3` offset, `Quaternion` facing) | Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance. | 
| `Matrix4x4` | GetProjectionFrom(`Vector3` eyePos, `Single` near, `Single` far, `Vector3` offset, `Quaternion` facing) | Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance. | 
| `Vector3` | GetRight(`Vector3` offset, `Quaternion` orientation) | Get the display's "right" direction. | 
| `Matrix4x4` | GetViewFrom(`Vector3` eyePos) | Get the display's view matrix for a specific viewer. | 
| `Matrix4x4` | GetViewFrom(`Transform` viewer) | Get the display's view matrix for a specific viewer. | 
| `Matrix4x4` | GetViewFrom(`Transform` viewer, `Vector3` offset, `Quaternion` facing) | Get the display's view matrix for a specific viewer. | 
| `Matrix4x4` | GetViewFrom(`Vector3` eyePos, `Vector3` offset, `Quaternion` facing) | Get the display's view matrix for a specific viewer. | 
| `Boolean` | Raycast(`Ray` ray, `Single&` distance, `Vector2&` hitPoint2D) | Cast a world-space ray into this display and return if there is an  intersection, along with distance to the intersection and the  relative 2D intersection point on the display. Only applies to displays  that exist within world-space. | 
| `void` | SetBackgroundColour(`Color` colour) |  | 
| `void` | SetClearFlags(`CameraClearFlags` flags) |  | 
| `Ray` | ViewportPointToRay(`Vector2` displayspacePoint) | Get a world-space ray from the display via a 2D display-space point. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | DrawGizmo(`Display` config) | A method to draw the display's Gizmo within the editor. | 


## `OSCReceiverBehaviour`

```csharp
public abstract class HEVS.OSCReceiverBehaviour
    : MonoBehaviour

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | OnOSCReceived(`String` address, `IList<Object>` data) |  | 


## `OSCTransformTransmitter`

An OSC transmitter that transmits this GameObject's local transform, either  combined as /transform using 9 floats for position, rotation (euler), scale,  or separately as /position, /rotation and /scale respectively.
```csharp
public class HEVS.OSCTransformTransmitter
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | address | The IP address / hostname that the transmitter will send to. | 
| `Boolean` | broadcastOnStart | Should the transmitter start broadcasting as soon as it has loaded into the scene. | 
| `String` | id | The ID of the broadcaster. | 
| `Int32` | port | The port that the transmitter will send to. | 
| `Int32` | transformFlags | Flags to control which parts of the transform will be transmitted. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | StartBroadcasting() | Start broadcasting to the default address and port. | 
| `void` | StartBroadcasting(`String` address, `Int32` port = 7890) | Start broadcasting to the default address and port. | 
| `void` | StopBroadcasting() | Stop broadcasting from this transmitter. | 


## `PlatformType`

A helper enumeration of potential platform types that HEVS may use.  This is not a strict list, but can be used to help identify particular platform designs.
```csharp
public enum HEVS.PlatformType
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Undefined | Undefined platform type. | 
| `1` | Desktop | Standard desktop platform. | 
| `2` | XR | A VR/AR platform. | 
| `3` | Cylinder | A cylindrical platform. The platform might use Curved or Off-Axis displays to define a cylinder shape. | 
| `4` | Dome | A dome platform, typically using one or more projectors. | 
| `5` | CAVE | A CAVE platform, of unknown configuration, that typically uses Off-Axis displays. | 
| `6` | Handheld | A handheld platform, such as phone or tablet. | 


## `Pointer`

A HEVS Pointer component for implementing a world-space pointing ray that can intersect with HEVS displays and UI elements.
```csharp
public class HEVS.Pointer
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Nullable<RaycastHit>` | _lastHit | The last raycastHit made when raycasting with the pick ray each Update | 
| `Boolean` | debugDraw | Draw debug lines to show pickray directions? | 
| `String` | inputUISubmit | The input name for button presses to submit interaction with the UI.  CanvasManager will use this to check if the user has 'clicked' a UI element this pointer is pointing at. | 
| `Display` | intersectedDisplay | the display that has been intersected with | 
| `Vector3` | intersectedDisplayPoint | a world-space point of intersection with a display | 
| `Vector2` | intersectedDisplayPoint2D | a display-space 2D intersection | 
| `Ray` | pickRay | the "display-space" ray that should follow the visual projection | 
| `Vector3` | pickRayEndPoint | Where a raycast made along the pickray stopped.  Either an object it collided with or the main camera's far clip distance. | 
| `Vector3` | pickRayHitNormal | The normal returned from a collision made by raycasting on the pickray. | 
| `Transform` | pickTransform | A transform which will have its position and facing matched to the pickRay origin and direction. | 
| `Transform` | pointer | The transform used to aim at things (self if not assigned) | 
| `LayerMask` | selectableLayerMask | mask objects for selection | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Nullable<RaycastHit>` | lastHit | Access to the last RaycastHit result. | 
| `Ray` | pointerRay | the world-space ray from the pointer | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | SafeGetLastHitGameObject() | Returns the last ray hit GameObject, or null if there was no hit. | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<Pointer>` | pointerList | A list of all pointers in the scene. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Ray` | GetPickRay() | Returns the pickray of the first pointer in the scene.  Quick way to access pickray if you only have one. | 
| `Ray` | GetPickRay(`Int32` index) | Returns the pickray of the first pointer in the scene.  Quick way to access pickray if you only have one. | 
| `Ray` | GetPickRay(`String` pointerName) | Returns the pickray of the first pointer in the scene.  Quick way to access pickray if you only have one. | 


## `Replay`

```csharp
public class HEVS.Replay

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | currentFrame |  | 
| `Single` | currentTime |  | 
| `Single` | duration |  | 
| `String` | filePath |  | 
| `Boolean` | isPaused |  | 
| `Boolean` | isPlaybackDone |  | 
| `Boolean` | isPlaying |  | 
| `Boolean` | isReady |  | 
| `Boolean` | isRecording |  | 
| `ByteBufferReader` | playbackStream |  | 
| `ByteBufferWriter` | recordStream |  | 
| `Single` | startTime |  | 
| `ReplayState` | state |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Finalize() |  | 
| `void` | Flush() |  | 
| `void` | PausePlayback(`Boolean` pause = True) |  | 
| `void` | PauseRecording(`Boolean` pause = False) |  | 
| `void` | SetReplayPath(`String` path) |  | 
| `void` | StartPlayback() |  | 
| `void` | StartRecording() |  | 
| `void` | StopPlayback() |  | 
| `void` | StopRecording() |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Replay` | active |  | 


## `ReplayState`

```csharp
public enum HEVS.ReplayState
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Invalid |  | 
| `1` | Ready |  | 
| `2` | Recording |  | 
| `3` | RecordingPaused |  | 
| `4` | Playing |  | 
| `5` | PlayingPaused |  | 


## `RPC`

HEVS RPC manager class used to execute RPC within a cluster.
```csharp
public class HEVS.RPC

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | numberOfCallsForAll | The number of calls made from the master to the clients this frame. | 
| `Int32` | numberOfCallsLastFrame | The number of calls made from the master to the clients last frame. | 
| `Int32` | numberOfMasterCalls | The number of calls made to the master this frame from this client node. | 
| `Int32` | numberOfMasterCallsLastFrame | The number of calls made to the master last frame from clients.  If queried on a client node the number is how many calls this client made. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Call(`Action` method) |  | 
| `void` | Call(`Action<T1>` method, `T1` arg1) |  | 
| `void` | Call(`Action<T1, T2>` method, `T1` arg1, `T2` arg2) |  | 
| `void` | Call(`Action<T1, T2, T3>` method, `T1` arg1, `T2` arg2, `T3` arg3) |  | 
| `void` | Call(`Action<T1, T2, T3, T4>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13, `T14` arg14) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13, `T14` arg14, `T15` arg15) |  | 
| `void` | Call(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13, `T14` arg14, `T15` arg15, `T16` arg16) |  | 
| `void` | Call_Internal(`Delegate` method, `Object[]` arguments) | Call a method on mirror Cluster Objects on each client.  The call should be made from a monobehaviour on a Cluster Object on the Master. | 
| `void` | CallOnMaster(`Action` method) |  | 
| `void` | CallOnMaster(`Action<T1>` method, `T1` arg1) |  | 
| `void` | CallOnMaster(`Action<T1, T2>` method, `T1` arg1, `T2` arg2) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3>` method, `T1` arg1, `T2` arg2, `T3` arg3) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13, `T14` arg14) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13, `T14` arg14, `T15` arg15) |  | 
| `void` | CallOnMaster(`Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>` method, `T1` arg1, `T2` arg2, `T3` arg3, `T4` arg4, `T5` arg5, `T6` arg6, `T7` arg7, `T8` arg8, `T9` arg9, `T10` arg10, `T11` arg11, `T12` arg12, `T13` arg13, `T14` arg14, `T15` arg15, `T16` arg16) |  | 
| `void` | CallOnMaster_Internal(`Delegate` method, `Object[]` arguments) | Call a method on the master's mirror Cluster Object.  The call should be made from a monobehaviour on a client Cluster Object.  The method will not be invoked on any of the clients. | 
| `void` | DeregisterObject(`ClusterObject` obj) | Deregister a Cluster Object, so that we don't search through it when making an RPC.  Typically because it has been destroyed. | 
| `void` | DeserializeCallsOnAll(`ByteBufferReader` reader) |  | 
| `void` | DeserializeCallsOnMaster(`ByteBufferReader` reader) |  | 
| `void` | InvokeCallsOnAll() | Loop through all the calls for a client and invoke them.  Should only be called on a client. | 
| `void` | InvokeCallsOnMaster() | Loop through all the calls for a Master and invoke them.  Should only be called on the master. | 
| `void` | RegisterObject(`ClusterObject` obj) | Register a Cluster Object with our dictionary so we can invoke methods on it later using RPC. | 
| `void` | RegisterStaticRPCalls() | Finds and registers all static RPCAttribute methods on types that inherit IRPCInterface. | 
| `void` | ResetState() |  | 
| `void` | SerializeCallsOnAll(`ByteBufferWriter` writer) |  | 
| `void` | SerializeCallsOnMaster(`ByteBufferWriter` writer) |  | 


## `RPCAttribute`

HEVS.RPC attribute is needed to identify functions which can be called as remote procedures.
```csharp
public class HEVS.RPCAttribute
    : Attribute, _Attribute

```

## `SceneOrigin`

Component that signals that the owning GameObject's transform represents the scene origin  for HEVS, which all trackers and displays are relative to.  Also used as a static class to access the scene origin position and orientation.
```csharp
public class HEVS.SceneOrigin
    : MonoBehaviour

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | gameObject | Access to the Scene Object's GameObject, if valid. | 
| `Vector3` | position | Returns the scene origin's position. If there is no SceneOrigin component in the active scene then it returns Vector3.zero. | 
| `Quaternion` | rotation | Returns the scene origin's orientation. If there is no SceneOrigin component in the active scene then it returns Quaternion.identity. | 


## `SelectableUIElement`

A Unity UI Selectable component, and the means to manipulate it.  On creation we store what type of Selectable it is, so later we can adjust behaviour accordingly.
```csharp
public class HEVS.SelectableUIElement

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `RectTransform` | rectTransform | The rect transform that this UI element uses. | 
| `Selectable` | selectable | Access to the selectable element. | 
| `Type` | type | The type of this selectable UI element. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | UpdateSelected(`Pointer` pointer) | Called each Update() the provided pointer is pointing at this selectable's Rect Transform.  If the pointer's submit button is pressed we want to use the Selectable. | 


## `SequentialStereoExtension`

A component that controls toggling between two cameras for sequential stereoscopic 3D on certain hardware.
```csharp
public class HEVS.SequentialStereoExtension
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Camera` | left | The camera used for the left eye capture. | 
| `Camera` | right | The camera used for the right eye capture. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | SwapEyes() | Manually swap which eye is active. | 


## `StandardDisplay`

Display class for a standard on-axis flat-panel display, typical of a desktop computer.  Options are read from the JSON configuration and then used to setup a display rig.
```csharp
public class HEVS.StandardDisplay
    : Display

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Display` | Clone() | Creates a cloned copy of this display. | 
| `void` | ConfigureDisplayForScene() | Sets up the Unity scene to use this display. | 
| `void` | GatherDisplayGeometry(`List<Vector3>` vertices, `List<Int32>` indices, `List<Vector2>` uvs) |  | 
| `Boolean` | Parse(`JSONNode` json) | Setup this display from JSON data.  Note: StandardDisplays have no options so this method does nothing. | 
| `Boolean` | Raycast(`Ray` ray, `Single&` distance, `Vector2&` hitPoint2D) | Cast a world-space ray into this display and return if there is an  intersection, along with distance to the intersection and the  relative 2D intersection point on the display. Only applies to displays  that exist within world-space. | 
| `void` | SetBackgroundColour(`Color` colour) |  | 
| `void` | SetClearFlags(`CameraClearFlags` flags) |  | 
| `Ray` | ViewportPointToRay(`Vector2` displaySpacePoint) | Get a world-space ray from the display via a 2D display-space point. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | DrawGizmo(`Display` config) | A method to draw the display's Gizmo within the editor. | 


## `StereoAlignment`

An enumeration specifying the alignment of stereoscopic cameras.
```csharp
public enum HEVS.StereoAlignment
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Screen | Screen-aligned means that the left and right eye cameras are positioned  based on the display's right and up axes. This primarily applies of Off-Axis  displays where you want the 3D stereoscopic effect to work for multiple viewers. | 
| `1` | Camera | Camera-aligned means hat the left and right eye cameras are positioned  relative to the MainCamera's orientation. | 


## `StereoMode`

An enumeration of different stereoscopic implementations available to HEVS.
```csharp
public enum HEVS.StereoMode
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Mono | A standard monoscopic mode outputting into the entire viewport. | 
| `1` | LeftOnly | Only the left eye's content should be displayed to the entire viewport. | 
| `2` | RightOnly | Only the right eye's content should be displayed to the entire viewport. | 
| `3` | SideBySide | 3D stereoscopic where the left eye's content will be displayed on the left half of the viewport,  and the right eye's content will be displayed on the right half of the viewport. | 
| `4` | TopBottom | 3D stereoscopic where the left eye's content will be displayed on the top half of the viewport,  and the right eye's content will be displayed on the bottom half of the viewport. | 
| `5` | Sequential | 3D stereoscopic where the left eye's content will be displayed within the entire viewport for one  frame, and then the right eye's content will be displayed within the entire viewport in the subsequent frame,  and then the process will repeat. This mode is not recommended. | 
| `6` | QuadBuffered | 3D stereoscopic using hardware Quadbuffering for specific compatible hardware, such as NVidia Quadro. Requires OpenGL. | 
| `7` | TopBottomMono | A monoscopic mode that will render the same view into the top half of the viewport and into the bottom half  of the viewport. This mode is useful for platforms that output TopBottom/BottomTop stereoscopic but want to  output monoscopic without having to change hardware configurations. | 
| `8` | SideBySideMono | A monoscopic mode that will render the same view into the left half of the viewport and into the right half  of the viewport. This mode is useful for platforms that output LeftRight/RightLeft stereoscopic but want to  output monoscopic without having to change hardware configurations. | 
| `9` | TopBottomLeftOnly | A mode that will render the left eye's content into the top half of the viewport and into the bottom half  of the viewport. This mode is useful for platforms that output TopBottom/BottomTop stereoscopic but want to  output just the left eye without having to change hardware configurations. | 
| `10` | SideBySideLeftOnly | A mode that will render the left eye's content into the left half of the viewport and into the right half  of the viewport. This mode is useful for platforms that output LeftRight/RightLeft stereoscopic but want to  output just the left eye without having to change hardware configurations. | 
| `11` | TopBottomRightOnly | A mode that will render the right eye's content into the top half of the viewport and into the bottom half  of the viewport. This mode is useful for platforms that output TopBottom/BottomTop stereoscopic but want to  output just the right eye without having to change hardware configurations. | 
| `12` | SideBySideRightOnly | A mode that will render the right eye's content into the left half of the viewport and into the right half  of the viewport. This mode is useful for platforms that output LeftRight/RightLeft stereoscopic but want to  output just the right eye without having to change hardware configurations. | 
| `13` | RedGreen | Anaglyph for red-green glasses. | 
| `14` | RedBlue | Anaglyph for red-blue glasses. | 
| `15` | RedCyan | Anaglyph for red-cyan glasses. | 


## `Time`

Helper class to access cluster time via HEVS.Time
```csharp
public class HEVS.Time

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | deltaTime | The current Unity delta-time of the cluster's master. | 
| `Double` | fps | The current node's frames-per-second (FPS). | 
| `Int32` | frameCount | The current Unity frame count of the cluster's master. | 
| `Single` | time | The current Unity time of the cluster's master. | 


## `Touch`

A HEVS Touch object.
```csharp
public class HEVS.Touch

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | source | The source of the touch. | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Vector2` | deltaPosition | The normalized delta-position of this touch since its last position. | 
| `Single` | deltaTime | The delta-time of this touch. | 
| `Int32` | fingerId | The finger ID of this touch. | 
| `TouchPhase` | phase | The phase of the touch. | 
| `Vector2` | position | The normalized position of this touch. | 
| `Int32` | tapCount | The tap count of this touch. | 
| `TouchType` | type | The touch type. | 


## `TrackedBool`

A TrackedVariable for tracking a boolean.
```csharp
public class HEVS.TrackedBool
    : TrackedVariable<Boolean>

```

## `TrackedColor`

A TrackedVariable for tracking a Color.
```csharp
public class HEVS.TrackedColor
    : TrackedVariable<Color>

```

## `TrackedDouble`

A TrackedVariable for tracking a double.
```csharp
public class HEVS.TrackedDouble
    : TrackedVariable<Double>

```

## `TrackedFloat`

A TrackedVariable for tracking a float.
```csharp
public class HEVS.TrackedFloat
    : TrackedVariable<Single>

```

## `TrackedInt`

A TrackedVariable for tracking a integer.
```csharp
public class HEVS.TrackedInt
    : TrackedVariable<Int32>

```

## `TrackedQuaternion`

A TrackedVariable for tracking a Quaternion.
```csharp
public class HEVS.TrackedQuaternion
    : TrackedVariable<Quaternion>

```

## `TrackedVariable<T>`

A templated type that can be used to track changes in base variables.
```csharp
public class HEVS.TrackedVariable<T>

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | hasChanged | Check if the value has changed since the last call to this method. | 
| `T` | lastValue | Access to the previous value of the variable. | 


## `TrackedVector2`

A TrackedVariable for tracking a Vector2.
```csharp
public class HEVS.TrackedVector2
    : TrackedVariable<Vector2>

```

## `TrackedVector3`

A TrackedVariable for tracking a Vector3.
```csharp
public class HEVS.TrackedVector3
    : TrackedVariable<Vector3>

```

## `TrackedVector4`

A TrackedVariable for tracking a Vector4.
```csharp
public class HEVS.TrackedVector4
    : TrackedVariable<Vector4>

```

## `Tracker`

A HEVS component used to apply tracking to a GameObject.  It will first use its ID to check the HEVS platform config for a matching tracker and then use  the settings from the config file. If it is unable to find a matching tracker then it will  fall back to the component values.
```csharp
public class HEVS.Tracker
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | configId | The ID of the tracker to search for within the HEVS platform config. | 
| `Boolean` | disableIfNotFound | Should this GameOBject be deactivated if it cannot find a matching tracker ID within the config file? | 
| `Boolean` | forceMouseInEditor | Specifies if the tracker should always fallback to the mouse within the editor. | 
| `Boolean` | masterOnly |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | address | The address used by the tracker (i.e. name@IP for VRPN, etc) | 
| `Tracker` | config | Access to the config used to create this tracker. | 
| `DefaultType` | defaultType | The default tracker type to use if the matching config entry cannot be found. | 
| `TrackerAxis` | forward |  | 
| `TrackerHandedness` | handedness |  | 
| `Transform` | offsetTransform |  | 
| `Int32` | port | The port that OSC will listen on. | 
| `TrackerAxis` | right |  | 
| `Boolean` | smoothing |  | 
| `Single` | smoothMultiplier |  | 
| `Int32` | transformFlags | Flags to control which parts of the transform will synchronise. | 
| `TrackerAxis` | up |  | 
| `XRNode` | xrNode |  | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, Type>` | registeredTrackerDevices |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | GatherCustomTrackerTypes() |  | 


## `TrackerAxis`

An enumeration of potential axes, including negative axes.
```csharp
public enum HEVS.TrackerAxis
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | X | Position X axis (1,0,0). | 
| `1` | Y | Position Y axis (0,1,0). | 
| `2` | Z | Position Z axis (0,0,1). | 
| `3` | NEG_X | negative X axis (-1,0,0). | 
| `4` | NEG_Y | Negative Y axis (0,-1,0). | 
| `5` | NEG_Z | Negative Z axis (0,0,-1). | 


## `TrackerHandedness`

An enumeration for coordinate space handedness, which can be either Left-handed or Right-handed.  See https://en.wikipedia.org/wiki/Right-hand_rule for reference.
```csharp
public enum HEVS.TrackerHandedness
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Left | Left-handed coordinate space (if (1,0,0) is Right, and (0,1,0) is Up, then (0,0,1) is Forward). | 
| `1` | Right | Right-handed coordinate space (if (1,0,0) is Right, and (0,1,0) is Up, then (0,0,-1) is Forward). | 


## `TransformFlags`

Flags used to mask which parts of a transform should be synched or used.
```csharp
public enum HEVS.TransformFlags
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `-1` | All | All. | 
| `0` | None | None of the transform. | 
| `1` | Position | Translation only. | 
| `2` | Rotation | Rotation only. | 
| `3` | PositionRotation | Translation and Rotation only. | 
| `4` | Scale | Scale only. | 


## `TUIODevice`

Processes TUIO 1.1 input.
```csharp
public class HEVS.TUIODevice

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<TuioBlob>` | blobs | A list of current active blobs from this TUIO connection. | 
| `List<TuioCursor>` | cursors | A list of current active cursors from this TUIO connection. | 
| `List<TuioObject>` | objects | A list of current active objects from this TUIO connection. | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `TUIODevice` | config | Access to the HEVS TUIO config settings for this TUIO connection. | 
| `Boolean` | connected | Is this TUIO connection "connected". | 
| `String` | id | The ID of this TUIO connection. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Connect(`TUIODevice` config) | Connect TUIO through a specific port from config settings. | 
| `void` | Disconnect() | Disconnect this TUIO connection. | 


## `TUIOInputType`

The type of the TUIO input message.
```csharp
public enum HEVS.TUIOInputType
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `1` | Cursors | Pointer. | 
| `2` | Blobs | Shape. | 
| `4` | Objects | Tagged object. | 


## `Utils`

A Utility class containing various utility sub-classes and methods.
```csharp
public class HEVS.Utils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateFolder(`String` folder) | Recursively create a folder structure.  If parent folder doesn't exist, create as well. | 
| `UInt32` | DecodeIntRGBA(`Color` color) | Convert a UnityEngine.Color to a 32bit unsigned int. | 
| `void` | DeleteFolder(`String` folder, `Boolean` recursive = False) | Delete all files from a folder, and optionally delete sub-folders.  If the folder is empty after its contents have been deleted then it is also deleted. | 
| `String` | GetCleanFileName(`String` originalFileName) | Clean filename and convert to WWW format (starting with file:// and using only forward slashes). | 
| `String` | IndexArrayToString(`UInt32[]` array) | Convert an index array to a comma separated values string. | 
| `Dictionary<String, Object>` | JSONToDictionary(`JSONNode` node) | Convert a JSONNode to a Dictionary. | 
| `Object` | JSONToValue(`JSONNode` node) | Convert a JSONNode to an object. | 
| `Texture2D` | LoadTexture(`String` filePath) | Load a texture from disc, reading the contents to memory then creates a UnityEngine.Texture2D. | 


## `VRPN`

Virtual-Reality Peripheral Netwrok (VRPN) access.
```csharp
public static class HEVS.VRPN

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IDevice` | AddOrGetDevice(`String` address) | Adds or retrieves a VRPN device based on its full address. | 
| `IDevice` | AddOrGetDevice(`String` name, `String` host) | Adds or retrieves a VRPN device based on its full address. | 
| `void` | CheckVRPNAvailable() |  | 
| `Double` | GetAxisState(`String` name, `String` host, `Int32` axis) | Get the current value of a VRPN axis using a device name and host address. | 
| `Double` | GetAxisState(`String` address, `Int32` axis) | Get the current value of a VRPN axis using a device name and host address. | 
| `Boolean` | GetButtonState(`String` name, `String` host, `Int32` button) | Get the button state of a VRPN button based on a device name and host address. | 
| `Boolean` | GetButtonState(`String` address, `Int32` button) | Get the button state of a VRPN button based on a device name and host address. | 
| `IDevice` | GetDevice(`String` address) | Retrieves an existing device based on its full address. | 
| `void` | GetTrackerState(`String` name, `String` host, `Vector3&` position, `Quaternion&` rotation, `Int32` frame) | Retrieve the state of the tracker based on its name and host. | 
| `void` | GetTrackerState(`String` address, `Vector3&` position, `Quaternion&` rotation, `Int32` frame) | Retrieve the state of the tracker based on its name and host. | 
| `void` | UpdateDevices() |  | 


## `WorldCursor`

A HEVS component to add a world-space cursor to a GameObject that uses a HEVS Pointer to position itself.
```csharp
public class HEVS.WorldCursor
    : MonoBehaviour

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | appearAtCollideDepth | How far between the screen and the collision the cursor should appear, when cursorPosition is collide.  0 - screen, 1 - collison, 0.5 - halfway | 
| `LineRenderer` | beam | A line renderer which will follow the pick ray's origin to end point. | 
| `Color` | color | If assigned the sprite and beam (line renderer) will be set to this color. | 
| `CursorOrientation` | cursorOrientation | What the cursor's up vector should be based on. | 
| `CursorPosition` | cursorPosition | Position the cursor appears at relative to the pickray.  Screen - on the screen, where pick ray is pointing.  Collide - before the pick ray collides with something in the world, and flat against that surface. | 
| `Pointer` | pointer | The pointer the cursor will mirror | 
| `SpriteRenderer` | sprite | A sprite representing the cursor.  If set will have its color property set to the cursor's color. (Sprite's color should be white for this to work well)  Sprite is assumed to be parented to the cursor object, and thus be moved with it. | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Ray` | pickRay | Access to the Pointer's pick ray. | 


