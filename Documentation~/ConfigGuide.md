| [Home](https://github.com/EPICentreUNSW/hevs/) | [User Guide](UserGuide.md) | [Configuration Guide](ConfigGuide.md) | [Porting Guide](PortingGuide.md) | [API](API.md) |
|-|-|-|-|-|
# Configuration Guide
## Getting Started
HEVS aims to be simple to integrate and easy to run on multiple platforms, without having to create separate builds, unless explicitly needed for a platform or application.

To achieve these aims, HEVS uses a JSON-based configuration file to describe the target platform and its architecture. One or more platforms can be defined within a single JSON file, which is explained further within the Configuring HEVS section on the following pages.

The JSON file needs to be in a path that is accessible by all running instances of your application. This may be on a shared network drive or duplicated locally on each node that will be running the application. 

HEVS defines the following elements within the JSON config, which are explained further on the following pages:

* **Platform**
  * Specifies a visualisation platform, such as a CAVE installation, display wall, or VR system. It encapsulates all hardware within the visualisation platform, which may or may not make use of multiple host machines.
* **Node**
  * Represents a running instance of a HEVS application. A host machine can run one or more Node instances, but typically only one Node instance runs per host machine within a visualisation Platform.
* **Display**
  * Represents a visual output for a Node. A Display does not represent a connected hardware monitor, but rather output to connected monitors. A Node may use a single Display, or multiple. For example, a Display might output to a connected VR Head-mounted Display (HMD), or multiple Displays might output through a connected hardware projector and make use of Viewports to output to specific parts of the projected display.
* **Cluster**
  * Options for Platforms that use multiple connected host machines running Nodes to create a clustered visualisation platform.
* **Stereo**
  * Options for Displays that make use of stereoscopic 3D.
* **Input**
  * Specifies alternate sources of keyboard and controller button presses for a Platform.
* **Tracker**
  * Specifies the source of a tracked transform, such as a tracked XR device or motion captured rigid body, which can be used to manipulate the transforms of GameObjects at run-time.
* **TUIO**
  * Specifies alternate touch sources, such as a phone or tablet, that a Platform can make use of.


## Configuring HEVS
HEVS configuration files utilise the following JSON structure:
```javascript
{
  // platforms contains an array of platform definitions
  "platforms": {
    "PlatformA": {
      "nodes": {
        "node0": { /* node 0 options */ },
        // ...
        "nodeN": { /* node N options */ }
      },
      "displays": {
        "display0": { /* display 0 options */ },
        // ...
        "displayN": { /* display N options */ }
      },
      "viewports": {
        "viewport0": { /* viewport 0 options */ },
        // ...
        "viewportN": { /* viewport N options */ }
      },
      "cluster": { },
      "stereo": { },
      "input": {
        "input0": { /* input source 0 options */ },
        // ...
        "inputN": { /* input source N options */ }
      },
      "trackers": {
        "tracker0": { /* tracker 0 options */ },
        // ...
        "trackerN": { /* tracker N options */ }
      },
      "tuio": {
        "tuio0": { /* tuio source 0 options */ },
        // ...
        "tuioN": { /* tuio source N options */ }
       }
    },
    "PlatformB": {
      // etc
    }
 }
}
```
Additionally, some options can include a transform object that uses the following structure:
```javascript
"transform": { "translate": [0,0,0], "rotate": [0,0,0], "scale": [0,0,0] }
```
#### Transform Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
| translate | Array (3 singles, meters) | [ 0,0,0 ] | |
| rotate | Array (3 Singles, degrees) | [ 0, 0, 0 ]	| (Angle around X, Y and Z) |
| scale	| Array (3 Singles, meters) | [ 1, 1, 1 ] | |

The config file can contain 0 or more Platforms, which can contain 0 or more Nodes. A Node can then make use of 0 or more displays (explained in further detail later in this document). On the following pages are listed the options for each config entry, including their default values.

### Platform Configuration
A platform defines everything about a visualisation system that the HEVS application will be running on.

It contains a collection of the nodes within the platform (0 or 1 if a single node platform, multiple if a cluster), a list of the displays that the platform contains, and then optional lists for viewports that the displays use (if a display does not specify a viewport then it assumes the viewport covers the whole display), a list of trackers that the platform has access to (such as wands or head-tracking), optional cluster settings that are only required if you want the application to run as a cluster (you can still have multiple nodes without any cluster options, but note that each node will run as a separate stand-alone instance) and optional stereoscopic settings. 

#### Platform Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
| inherit | String or Array of Strings | (optional)	
| globals | Dictionary | (optional)	
| nodes | Dictionary (Node Configs) | (optional)	
| displays | Dictionary (Display Configs) | (optional)	
| viewports | Dictionary (Viewport Configs) | (optional)	
| cluster | Object (Cluster Configs) | (optional)	
| stereo | Object (Stereo Configs) | (optional)	
| trackers | Array (Tracker Configs) | (optional)	
| input	| Dictionary (Input Config) | (optional	
| tuio | Dictionary (TUIO Config) | (optional)	

### Platform Inheritance
A platform can contain an optional inherit value:

```javascript
"PlatformA": { "inherit": "PlatformA" }
```

Inherit allows you to define a new platform that uses the exact same configuration as a previously defined platform. You can then add or replace settings to the new platform. Multiple inheritance is allowed, and each successive inherited platform will overwrite settings from earlier platforms.

For example, you might want to define a multi-display CAVE cluster that uses untracked stereoscopic so that the stereoscopic effect works for multiple viewers from various viewing angles, and then inherit from it to define a version that uses single-user head tracking without having to redefine all of the nodes and display settings; we simply modify the stereo options:

```javascript
"CAVE": {
  // other options removed for brevity
  "stereo": { "mode": "sidebyside", "alignment": "screen" }
},
"CAVETracked": {
  "inherit": "CAVE",
   "stereo": { "tracker": "Head", "alignment": "camera" }
}
```

### Viewport Configuration 
Viewports allow you to define and name a rectangular section of a display, using starting pixel coordinates and dimensions.

By themselves viewports do nothing, but when a display uses a viewport it renders into only the section of the display that the viewport specifies. They are useful for nodes that use multiple displays.

Viewports contain the following required settings:

#### Viewport Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
| x | Integer (pixels) | (required)	
| y | Integer (pixels) | (required)	
| width | Integer (pixels) | (required)	
| height | Integer (pixels) | (required)	
| absolute | Boolean | true | Defines if the units are in absolute pixel coordinates, or 0-to-1 range

```javascript
// example viewports for a 2-display single-node cave wall
"viewports": {
  "vp_forward": { "x": 0, "y": 1080, "width": 1920, "height": 1080 },
  "vp_down": { "x": 0, "y": 0, "width": 1920, "height": 1080 }
}
```

### Stereoscopic Configuration
HEVS supports various methods of implementing stereoscopic 3D.

Using stereo options we can define the way that the stereoscopic is to be displayed by a screen or projector. HEVS supports the following stereoscopic output configurations:

* **Mono** – no stereoscopic effect, full-viewport coverage
* **LeftOnly** – no stereoscopic effect, full-viewport coverage, however the output is from the perspective of the left eye
* **RightOnly** – no stereoscopic effect, full viewport coverage, however the output is from the perspective of the right eye
* **SideBySide** – left eye outputs into the left 50% of the viewport, right eye outputs into the right 50% of the viewport
* **TopBottom** – left eye outputs into the top 50% of the viewport, right eye outputs into the bottom 50% of the viewport 
* **Sequential** – left eye outputs to cover the entire viewport, and every alternate frame instead outputs the right eye to cover the entire viewport
* **QuadBuffered** – requires hardware quad buffer support 
* **RedGreen** – Single-image anaglyph using red-green tinting
* **RedBlue** – Single-image anaglyph using red-blue tinting
* **RedCyan** – Single-image anaglyph using red-cyan tinting

The following options are for stereoscopic displays that always project using stereoscopic enabled projectors but want to display a monoscopic image (Left and Right eye output the same image):

* **TopBottomMono** – no stereoscopic effect, outputs into the top 50% of the viewport and duplicates into the bottom 50% of the viewport
* **SideBySideMono–** no stereoscopic effect, outputs into the left 50% of the viewport and duplicates into the right 50% of the viewport
* **TopBottomLeftOnly** – no stereoscopic effect however the output is from the perspective of the left eye, outputs into the top 50% of the viewport and duplicates into the bottom 50% of the viewport
* **SideBySideLeftOnly** – no stereoscopic effect however the output is from the perspective of the left eye, outputs into the left 50% of the viewport and duplicates into the right 50% of the viewport 
* **TopBottomRightOnly** – no stereoscopic effect however the output is from the perspective of the left right, outputs into the top 50% of the viewport and duplicates into the bottom 50% of the viewport
* **SideBySideRightOnly** – no stereoscopic effect however the output is from the perspective of the left right, outputs into the left 50% of the viewport and duplicates into the right 50% of the viewport

We can also specify the eye separation. This value is the distance between each eye in meters.

When using stereoscopic you can specify the alignment of the cameras; if the virtual eyes used to render the scene should be orientated using the scene’s virtual camera (preferred for single-user stereoscopic experiences) or if the virtual camera should be orientated to face each display individually, regardless of the orientation of the scene’s main camera (this is the preferred option for multi-user stereoscopic experiences, such as a CAVE display with 2 or more users).

You can also swap the left and right eyes if needed, using the swap_eyes flag.

Additionally, we may specify a tracker that the main Camera will use to update its transform every frame. Trackers are described in further detail later in this document. Using camera alignment and a tracker would enable single-user stereoscopic where the 3D effect is correct based on the view direction of the tracked object, but the stereoscopic effect would be broken for any additional viewers. Using screen alignment, the position of the tracked object would be used for the camera, however the stereoscopic effect will look correct from any viewpoint within the view space.

Stereoscopic options have the following defaults and options:
#### Stereo Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
mode | String | Mono | Mono, LeftOnly, RightOnly, SideBySide,  TopBottom,  Sequential, QuadBuffered, SideBySideMono, SideBySideLeftOnly, SideBySideRightOnly, TopBottomMono, TopBottomLeftOnly, TopBottomRightOnly, RedGreen, RedBlue, RedCyan
separation | Single (meters) | 0.065	
swap_eyes | Boolean | False	
alignment | String / Enumeration | Camera | Camera, Screen
tracker | String | (optional)	

The following is an example stereo config:

```javascript
// example stereo options for single-user tracked quadbuffers
"stereo": { "mode": "quadbuffered", "separation": 0.065, "alignment": "camera", "tracker": "Head" }
```

### Display Configuration 

HEVS supports various display and screen configurations for flat and curved surfaces.

A platform can include as many displays as it likes, and nodes can be set up to use any number of displays, provided they are configured correctly.

HEVS supports the following display type definitions, which are defined using a string:

* **Standard** – standard perspective/orthographic projection display (i.e. standard desktop display or standard 2D flat-screen display)
* **Off-Axis** – The display uses physical screen properties (screen corners) to create an off-axis perspective projection (this would be typical of CAVE-like systems but can also be used for curved displays)
* **Curved** – This display uses starting and ending angles to create a camera to capture a specific field-of-view that may be larger than field-of-views used by standard displays. It is typically used to create cylindrical displays of one or more projectors. Additionally, can be split into sub-cameras to capture parts of the field-of-view and stitched back together to reduce stretching artefacts (this would be typical of cylindrical projector-based displays)
* **Dome** – Creates a camera rig to capture a fish-eye image of the scene that can then be projected using a single fish-eye projector, or projected as slices using multiple projectors (can also be configure to simply output a pre-computed fish-eye image)

Displays can inherit from previously defined displays within the same platform. Derived displays will start with all of the same settings as their inherited base display.

Displays can also include a 3-dimensional index. This index is not used within HEVS for any purpose, however you may find it useful to give displays an id for your specific project.

Layers can be specified that the display’s camera will use for its culling mask; the intention is that the camera will only render what you have placed within these layers. You may alternatively specify layers that are removed from the cameras culling mask; the camera will not display these layers. A good use-case for this would be a scene that contains a 2D canvas full of controls, in addition to a complex 3D scene; the controls could be within a layer that only the master node in a cluster will render, while the remaining nodes will only render the 3D scene.

Displays can also include a transform that is used to move and reorientate the camera within the scene. This can be useful for reorientating a dome camera, or rotating a cylinder display so that a certain feature is centred on the display.

The stereo config options listed earlier apply to all displays within a platform, however we can also override individual stereo options for each display. The options are the same as the stereo config, and if not specified for a display they will default to the stereo config options.

A display may also specify a viewport that it will output into. The default viewport matches the resolution of the node the display belongs to, but this can be overridden by listing the id of the viewport that it will use.
Displays can use post-processing “warp & blend” steps where the screen is warped based on offsets stored within a floating-point image. This is usually needed when using curved projectors.

Finally, depending on the display type, additional settings are used to correctly configure the display. These are listed in more details in their own sub-sections.
 
Additional options are available for specifying a field-of-view and aspect scale. The aspect scale is useful for scaling displays that might use smaller viewports that can result in a “squished” appearance, such as when using left-right stereoscopic. Rather than being a value to set the aspect ratio it is a value used to scale the aspect.

For multi-monitor/output systems we can also specify a specific monitor for the display to output into.

The following table details display config options that apply to all display types:
#### Display Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
inherit | String | (optional)	
type | String / Enumeration | Standard | Standard, OffAxis, Dome, Curved
index | Array (1-3 Integers) | [ 0, 0, 0 ] | (1, 2 or 3 integers can be specified)
transform | Object (Transform) | (optional)	
layers | Array (Strings) | Default | (Renders only these layers)
cull_layers | Array (Strings) | (optional)	(Removes these layers from visible layers)
viewport | String	(optional)	
stereo_mode | String / Enumeration | (uses Stereo Config) | (See Stereo Config mode options)
separate_eyes | Boolean | (uses Stereo Config)	
eye_separation | Single (meters) | (uses Stereo Config)	
stereo_alignment | String / Enumeration | (uses Stereo Config) | Camera, Screen
fov | Single (degrees) | 60	
aspect_scale | Single | 1	
near | Single | -1 | (uses scene main camera value)	Camera near clip distance
far | Single | -1 | (uses scene main camera value)	Camera far clip distance
monitor | Integer | 0	
warp_path | String | (optional)	(Specifies a path to a tif)
blend_path | String | (optional)	(Specifies a path to a greyscale image)

### Additional Options Depending on Type

### Standard Displays

Standard displays are those that represent generic flat-screen perspective/orthographic displays that don’t use off-axis projections. This includes flat-screen projectors. There are no additional options for this type of display.
Example standard displays are as follows:

```javascript
// example 3-screen single-node platform where the master uses one screen
// while the remaining two screens are used for stereoscopic.
// the master also renders its own layer that is ignored by the others
"displays": {
   "dsp_main": { "layers": "MainDisplay", "stereo_mode": "mono", "viewport": "vp_main" },
   "dsp_left": { "cull_layers": "MainDisplay", "viewport": "vp_l" },
   "dsp_right": { "cull_layers": "MainDisplay", "viewport": "vrp_r" }
}
```

### Off-Axis Displays

Off-Axis displays are typical of CAVE and VR environments where the perspective being rendered onto the display comes from an arbitrary position that does not sit at 0,0 in the X and Y axes. These display types require additional values that are used to construct a perspective projection matrix based from a view position and pass through specified corner points on the display; the upper-left corner (ul), lower-left corner (ll) and lower-right corner (lr). These points are typically the real-world physical coordinates of a screen, specified in meters. All three points must be specified for the projection matrices to be constructed correctly.

#### Additional Off-Axis Display Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
ul | Array (3 Singles, meters)		
ll | Array (3 Singles, meters)		
lr | Array (3 Singles, meters)		

```javascript
// example 3-screen CAVE where the screens are 2m x 2m dimension
"displays": {
   "dsp_left": { "type": "offaxis", "ul": [ -1, 2, -1 ], "ll": [ -1, 0, -1 ], "lr": [ -1, 0, 1 ] },
   "dsp_front": { "type ": "offaxis", "ul": [ -1, 2, 1 ], "ll": [ -1, 0, 1 ], "lr": [ 1, 0, 1 ] },
   "dsp_right": { "type": "offaxis", "ul": [ 1, 2, 1 ], "ll": [ 1, 0, 1 ], "lr": [ 1, 0, -1 ] }
}
```

### Dome Displays

Dome displays represent any form of dome surface that uses a hemisphere to project onto. They can be implemented using a single fish-eye projector or multiple projectors that each represent a portion of the fish-eye image.

Dome displays first create a camera rig to capture a fish-eye image.

The image is then projected in one of three ways; using a single fish-eye projector, or using one or more projectors that also use warp and blend information to correctly project onto the dome surface, or using one or more projectors that access warp and blend information from a Vioso configuration file (if using Vioso AnyBlend technologies from Vioso GmbH).

You may specify a resolution for the captured fish-eye image or use the default 1024^2 resolution. You can then specify the projector count for the display. Note that the projector count is how many projectors are connected to the current node.

You should also include the physical radius of the dome. This is used for mapping the physical space when using tracking and ray-casts from the physical world into the virtual environment.

If using a fish-eye projector then no further options are required. If using Vioso AnyBlend you should include the vioso_path to the AnyBlend config file. You may also specify optional border smoothing options including a border percentage size. These variables can be used to fix certain overlap issues.

If not using fish-eye projectors or Vioso AnyBlend then you can also specify paths to custom warp and blend information. This information must be generated and calibrated for your system. (Tools for creating these calibration files are being developed by EPICentre to be provided with a future release of HEVS). If no further options are set then it is assumed that the display uses all projectors from the projector_count. You may instead specify an array of projector ids that this display uses. For example, in a cluster of 2 nodes using 4 projectors node 1 might use projector 1 and 2, while node 2 uses projector 3 and 4. The values are zero-indexed. If a node uses multiple projectors, then it is recommended that you also specify a layout. The layout is a 2D integer value used to arrange the projectors within a grid. This grid then creates sub-viewports for each projector arranged by the layout.

#### Additional Dome Display Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
fisheye_resolution | Array (2 Integers, pixels) | [ 1024, 1024 ]	
projector_count | Integer | 1	
radius | Single (meters) | 1	

*The following options can be used when projector_count == 1, but MUST be used if projector_count > 1:*

| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
projectors | Array (Integers) | (optional)	
warp_mesh_path | String | (required)	
aspect_scale | Single | 1	
layout | Array (2 Integers) | (optional)	

*If projector_count == 1 the projector may use Vioso config file options instead:*

| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
| vioso_path | String | (required) 
smooth_border | Boolean | False	
border_percent | Single | 0	

The following is an example 5-projector single-node dome using custom warp/blend information, followed by a 2-projector 3-node clustered dome using Vioso AnyBlend:

```javascript
// example 8-projector single-node dome
// the node uses Nvidia Mosaic to tile the projected display
"displays": {
  "id": { "type": "dome", "fisheye_resolution": [ 4096, 4096 ], "projector_count": 8, "layout": [ 4, 2 ], "radius": 3.0, "warp_mesh_path": "D:/calibration/" }
}

// example 2-projector 3-node clustered dome
// the master uses a standard display, while the 2 dome nodes use Vioso
// each dome display has a single projector
"displays": {
  "dsp_master": { },
  "dsp_dome": { "type": "dome", "fisheye_resolution": [ 4096, 4096 ], "projector_count": 1, "radius": 3.0, "vioso_path": "C:/ProgramData/AV Stumpfl/Wings 6/Vioso/_Startup.vwf/", "smooth_borders": true, "border_percent": 2 }
}
```

### Curved Displays 

Curved displays simply use a starting and ending angle to calculate the facing of the camera and its total field-of-view.

Additionally, the camera can be split into sub-cameras that are used to capture a fraction of the field-of-view and renders into a corresponding portion of the viewport. Using sliced cameras helps reduce visual artefacts from using large fields-of-view.

The cameras do not use standard on-axis perspective projection, instead they also use a radius and height value to create off-axis projection matrices. The height and radius are typically real-world physical properties matching the surface that the display is projecting onto, such as cylindrical surface properties. All distances are in meters, while the view angles are in degrees.

#### Additional Curved Display Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
angle_start | Single (degrees) | (required)	
angle_end | Single (degrees) | (required)	
slices | Integer | 1	
radius | Single (meters) | 1	
height | Single (meters) | 1	
ground_offset | Single | (meters) | 0	
warp_mesh_path | String | (optional)	
black_level_path | String | (optional)	
black_level | Single | (required)	
rendertarget_stretch_factor | Single | (required)	

The following is an example of a 5-projector 10-node cluster that projects 2 nodes with each projector (using separate DisplayPort cables, so viewport options are not required) to create a seamless 360-degree cylinder display, using Immersaview SimVisuals:

```javascript
// Example 5-projector 10-node cylinder display.
// The base options are defined together, then each defines their angles.
// Each uses 4 sub-cameras and projectors overlap, using blending software.
"displays": {
  "dp_base": { "type": "curved", "slices": 4, "height": 3, "radius": 3 },

  "dp_0": { "inherit": "dp_base", "angle_start": -192, "angle_end": -144 },
  "dp_1": { "inherit": "dp_base", "angle_start": -144, "angle_end": -97 },
  "dp_2": { "inherit": "dp_base", "angle_start": -117, "angle_end": -71 },
  "dp_3": { "inherit": "dp_base", "angle_start": -71, "angle_end": -23 },
  "dp_4": { "inherit": "dp_base", "angle_start": -50, "angle_end": 0 },
  "dp_5": { "inherit": "dp_base", "angle_start": 0, "angle_end": 47 },
  "dp_6": { "inherit": "dp_base", "angle_start": 24, "angle_end": 73 },
  "dp_7": { "inherit": "dp_base", "angle_start": 72, "angle_end": 119 },
  "dp_8": { "inherit": "dp_base", "angle_start": 95, "angle_end": 143 },
  "dp_9": { "inherit": "dp_base", "angle_start": 143, "angle_end": 192 }
}
```

### Node Configuration

Nodes represent the individual machines/systems within a platform. They are defined as node objects, within a nodes group as part of the platform config, each containing specific options for that node. Platforms contain one or more nodes, which may operate as a synchronised cluster.

Nodes contain an id which must match the hostname of the machine that will be executing the HEVS application. You can specify localhost as the id and the application will convert that to the hostname of the node the application is running on. You should also specify the IP4 address of the machine within your local network. If this is a non-clustered node then you may ignore it and leave it as the default 127.0.0.1, however the address is used if the node is within a cluster and so should be correctly configured.

Nodes are also where to specify the screen/projector resolution, rather than within a display. This is because a node can contain multiple displays which it expects will fit within its resolution. You can also specify if you want the node to run the application in fullscreen or windowed. Fullscreen has the additional option of being either an exclusive fullscreen application or run as a windowed fullscreen application. This differentiation between exclusive and windowed is important as some third-party software conflicts when running in exclusive fullscreen.

Nodes can contain one or more displays, specified either as a single string of the display id to use from the platform’s display list, or as an array of strings. When using multiple displays, it is important to ensure you have also set up viewports for each display. You may also opt to not specify any displays, in which case the node creates a default standard display which uses a default viewport which fills the screen.

When operating within a cluster it is common to have the nodes display a frame-synced scene. HEVS supports software or hardware frame, which is detailed further in the clustered config section. When using hardware frame-sync you may want to exclude a node from the sync, such as the cluster master, that may be displaying a separate scene. A Boolean option is available to specify if the node is hardware synced or not. By default all nodes are included within any hardware frame-sync.

Finally, nodes may contain a 3D integer index. This value is not used for anything within HEVS but may be useful for your own applications.

#### Node Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
address | String (IP4 or hostname) | 127.0.0.1	
resolution | Array (2 Integers, pixels) | (optional) | (will use last-used or desktop resolution if not specified)
fullscreen | Boolean | True	
fullscreen_exclusive | Boolean | False	
displays | String OR Array (Strings) | (optional)	
hardware_synced | Boolean | True	
index | Array (1-3 Integers) | [ 0, 0, 0 ] | (1, 2 or 3 integers can be specified)

The following are examples of node configurations. First is a 3-node cluster that has a separate master and then 2 nodes that are used to create a dome display. The second is a single-node CAVE that uses 2 projectors to project onto a wall in front of the viewer and onto the floor:

```javascript
// example 3-node cluster with a master and 2 dome projector nodes
"nodes": {
  "master": { "address": "192.168.1.1", "display": "dsp_master", "resolution": [ 800, 600 ], "fullscreen": false,
    "hardware_synced": false },
  "ig1": { "address": "192.168.1.2", "display": "dsp_dome", "resolution": [ 4096, 2160 ] },
  "ig2": { "address": "192.168.1.3", "display": "dsp_dome", "resolution": [ 4096, 2160 ] }
}

// example single-node CAVE with 2 displays
"nodes": {
  "localhost": { "display": [ "dsp_forward", "dsp_down" ], "resolution": [ 2560, 3200 ] }
}
```

### Cluster Configuration

HEVS platforms can contain multiple nodes that are synced together, controlled by a master, or they can execute as individuals oblivious to other nodes within the platform.

In the case of a clustered platform we must include a cluster config object within the platform definition. We must specify the id of the node within the platform that will act as the cluster master.

Cluster masters control the state of the cluster. They keep the cluster synchronised so that they update at approximately the same time and swap their display’s back-buffers at approximately the same time.

HEVS uses two ports for broadcasting state and synchronisation, which are specified as integers using data_port and sync_port.

HEVS uses ZeroMQ for its internal networking, which contains certain restraints on how the network architecture is implemented. One of these constrains is the packet size, which you can specify via the config.
HEVS synchronises certain Cluster Variables and Cluster Objects. To simplify the process of developing applications with HEVS certain objects can be automatically setup to include ClusterObjects. Physics bodies can be tagged to be automatically synced. Physics can also be set to only run on a master and using this flag combined with automatic physics body syncing allows a master to run and update all physics objects and then simply sync the object transforms to the remaining nodes in the cluster.

In addition to synchronising sate within the cluster the cluster config is used to control frame-locking; ensuring each display swaps at the same time to allow for seamless rendering. By default, HEVS uses software frame-locking, which means the master node creates a barrier for all nodes within the cluster. The master waits for all cluster nodes to notify it that they are ready to render. Once the master has received notification from all nodes it broadcasts out a signal to allow all nodes to proceed to their render step, and then to the next frame to await the state data from the master.

HEVS can be configured to use hardware frame-locking when running under OpenGL and using Nvidia Quadro GPUs with hardware frame-lock capabilities. When HEVS starts up all hardware frame-locked nodes attempt to join a GL swap group. A frame-lock master can also be specified which will attempt to join the GL swap group first, then all other nodes will join. This is not always needed, and if used will make use of an additional socket port to create a network barrier to control the master joining before the remaining nodes.

#### Cluster Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
master | String | (required)	
data_port | Integer | 7777	
sync_port | Integer | 7778	
packet_limit | Integer | 65536	
auto_sync_rigid | Boolean | True	
disable_client_physics | Boolean | True	
framelock | String / Enumeration | Software | Software, Hardware
framelock_master | String | (optional)	
lock_port | Integer | 7779	
client_timeout | Integer | 5000 | Milliseconds that the master will wait for a response from a client before dropping it from the cluster

The following is a simple cluster config for a typical cluster using hardware frame-lock:

```javascript
// example cluster using hardware frame-lock
"cluster": { "master": "ig0", "address": "192.168.1.1", "framelock": "hardware", "data_port": 7777, "sync_port": 7778 }
```

### Tracking Configuration

HEVS can make use of various systems for tracking real-life objects and converting them to virtual space, such as Virtual Reality Peripheral Network (VRPN), Open Sound Control (OSC) and XR systems.

A platform can optionally contain configuration for one or more trackers.

Trackers with matching names can be defined in multiple platforms but might draw the device’s state from different sources, such as a “Wand” coming from a VRPN source for “PlatformA”, but “Wand” comes from an XR source for “PlatformB”. The running application won’t care where the state comes from, just as long as it can find a device with a matching id.

Because different tracking systems can use different coordinate spaces we need a way to transform the data into the space that the application is using. To do this we have three axis that we map; forward, up and right. We can specify which axis from the tracker represents which direction within our virtual world. Additionally, not all spaces use the same handedness, so we can specify if the tracker uses a left-handed or right-handed tracker space.

We can also specify an additional transform that is applied to the tracker’s coordinates after it has been transformed into our application’s space, but before it is used by our application. This is useful in cases where the platform’s tracking system is oriented differently, such as Z facing backwards rather than forward.

In some cases, we may only care about syncing the position of an object, rather than the rotation, so we are able to define additional transform flags to specify which parts of the object’s transform we care about, using the property sync.

Some tracking systems report noisy data, in many cases due to the tracking system not being correctly calibrated. To compensate for this, optional smoothing can be applied to the tracker. Note, this will cause a noticeable delay in the tracked object, which can be adjusted, however it is recommended that you ensure your tracking system reports smooth data to begin with.

Finally, depending on the tracking system being used, you must specify the source of the tracking data. In the case of VRPN it is an address for a tracked vrpn device, i.e. “Wand@tracker”. In the case of OSC it is a port that will receive OSC tracker strings. In the case of XR it is the name of a Unity3D XRNode, i.e. LeftHand.

#### Tracker Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
type | String | (required) | vrpn, xr, osc
forward | String / Enumeration | Z | X, Y, Z, -X, -Y, -Z
right | String / Enumeration | X | X, Y, Z, -X, -Y, -Z
up | String / Enumeration | Y | X, Y, Z, -X, -Y, -Z
handedness | String / Enumeration | Left | Left, Right
transform | Object (Transform) | translate: [ 0, 0, 0 ] rotate: [ 0, 0, 0 ]	
sync | String (comma separated) | PositionRotation | Position, Rotation, Scale, PositionRotation, All
smooth | Boolean | False	
smooth_multiplier | Single | 1	
address | String |  | VRPN Only
port | Integer |  | OSC Only
node | String / Enumeration |  | XR Only (see Unity3D XRNode for options)

Below is an example of a trackers:

```javascript
// example of a VRPN tracked head and OSC tracked wand
"trackers": {
 "Head": { "type": "vrpn", "address": "Head@viconserver", "forward": "Y", "right": "X", "up": "Z", "handedness": "right" },
 "Wand": { "type": "osc", "port": 8574, "forward": "Y", "right": "X", "up": "Z", "handedness": "right" }
}
```

### Input Configuration

HEVS encapsulates the InputManager from Unity3D. In the case of a cluster it will broadcast the master’s input to the clients to replace their own local input sources.

Additionally, a platform can define extra input sources, such as VRPN devices or OSC devices.

These sources define their source (a vrpn address or osc port) along with button and axis mappings.

#### Input Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
type | String | (required) | vrpn, osc
source | String or Integer | (required) | VRPN address or OSC port
buttons | Array (Axis Config)		
axes | Array (Axis Config)		

Buttons contain an index, which represents the index of the button on the device. They then contain mapping information, which is either a single string when mapped to a single input axis/button action, or an array of strings. Using an array allows us to map a single button to multiple actions.

Axes, similar to buttons, contain an index of the analog/axis on the device. We then can map it to an application input axis, using either a single string or array of strings, just like buttons.

We can also flag if the axis should be inverted. 

The axis value is typically within the range [-1,1] or [0,1], depending on the device, however this can be a much larger range.

#### Axis Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
index | Integer | (required)	
mapping | String or Array | (required) | (uses values from Unity3D Input Manager)
invert | Boolean | False	

See the following for example input configuration:

```javascript
// example of two alternative input sources
"input": {
 "Controller1": { "type": "vrpn", "source": "Controller1@viconserver",
   "buttons": [
      { "index": 1, "mapping": "Fire2" },
      { "index": 2, "mapping": "Back" },
      { "index": 33, "mapping": [ "Fire1", "Submit" ] },
      { "index": 32, "mapping": "Fire3" ] }
   ],
   "axes": [
      { "index": 0, "mapping": "Horizontal" },
      { "index": 1, "mapping": "Vertical" }
   ] },

 "Controller2": { "type": "osc", "source": 8574,
   "buttons": [
      { "index": 2, "mapping": "Back" }
   ],
   "axes": [
      { "index": 0, "mapping": "Yaw" },
      { "index": 1, "mapping": "Pitch" }
   ] }
}
```

### TUIO Configuration

Much like alternative input sources for buttons and axes, HEVS supports alternative sources for touch input, utilising the TUIO protocol to receive touch data that HEVS injects into its encapsulation of Unity3D’s input system. See the TUIO specification for further details (www.tuio.org).

Multiple TUIO sources can be defined, each with a single option; the port that the data will come through.

#### TUIO Config:
| Property | Type | Default | Options/Notes |
|----------|------|---------|---------------|
port | Integer | (required)	

```javascript
// example TUIO sources
"tuio": {
  "Tablet": { "port": 8574 },
  "Phone": { "port": 8574 }
}
```

## Example Configurations

Example 1: Single-Node 4-display CAVE 

```javascript
{
  "platforms": {
    "CAVE": {
      "nodes": {
        "localhost": "display": [ "dsp_front", "dsp_down", "dsp_left", "dsp_right" ], "resolution": [ 3840, 2160 ] }
      },
      "displays": {
          "dsp_left": { "type": "offaxis", "ul": [ -1, 2, -1 ], "ll": [ -1, 0, -1 ], "lr": [ -1, 0, 1 ], "viewport": "vp_left" },
        "dsp_front": { "type ": "offaxis", "ul": [ -1, 2, 1 ], "ll": [ -1, 0, 1 ], "lr": [ 1, 0, 1 ] , "viewport": "vp_front" },
        "dsp_right": { "type": "offaxis", "ul": [ 1, 2, 1 ], "ll": [ 1, 0, 1 ], "lr": [ 1, 0, -1 ] , "viewport": "vp_right" },
        "dsp_down": { "type": "offaxis", "ul": [ -1, 0, 1 ], "ll": [ -1, 0, -1 ], "lr": [ 1, 0, -1 ] , "viewport": "vp_down" }
      },
      "viewports": {
        "vp_front": { "x": 0, "y": 0, "width": 1920, "height": 1080 },
        "vp_down": { "x": 1920, "y": 0, "width": 1920, "height": 1080 },
        "vp_left": { "x": 0, "y": 1080, "width": 1920, "height": 1080 },
        "vp_right": { "x": 1920, "y": 1080, "width": 1920, "height": 1080 },
      },
      "stereo": { "mode": "sequential", "separation": 0.065, "separate_eyes": true, "alignment": "camera", "tracker": "Head" },
      "trackers": {
        "Head": { "type": "vrpn", "address": "Head@tracker", "forward": "Y", "right": "X", "up": "Z", "handedness": "right" }
      }
    }
  }
}
```

Example 2: Single-Node 8-projector Dome

```javascript
{
  "platforms": {
    "Dome": {
      "nodes": {
          "localhost": { "display": "dsp_dome", "resolution": [ 10240, 3200 ] }
      },
      "displays": {
         "dsp_dome": { "type": "dome", "radius": 3.0, "fisheye_resolution": [ 4096, 4096 ], "projector_count": 8, "layout": [ 4, 2 ], "warp_mesh_path": "C:/DomeCalibration/" }
      },
      "trackers": {
        "Wand1": { "type": "vrpn", "handedness": "right", "address": "Controller1@192.168.1.101", "forward": "-Z", "right": "X", "up": "Y" },
        "Wand2": { "type": "vrpn", "handedness": "right",  "address": "Controller2@192.168.1.101", "forward": "-Z", "right": "X", "up": "Y" }
      },
      "input": {
        "Wand1": { "type": "vrpn", "address": "Controller2@192.168.1.101",
            "buttons": [
               { "index": 1, "mapping": "Fire2" },
               { "index": 2, "mapping": "Back" },
               { "index": 33, "mapping": [ "Fire1", "Submit" ] },
               { "index": 32, "mapping": "Fire3" }
            ],
           "axes": [
              { "index": 0, "mapping": "Horizontal" },
              { "index": 1, "mapping": "Vertical" }
            ]
        },
        "Wand2": { "type": "vrpn", "address": "Controller2@192.168.1.101",
            "buttons": [
               { "index": 2, "mapping": "Back" }
            ],
           "axes": [
              { "index": 0, "mapping": "Yaw" },
              { "index": 1, "mapping": "Pitch" }
            ] 
        }
      }
    }
  }
}
```

Example 3: Clustered Curved-projector Platform

```javascript
{
  "platforms": {
    "Cylinder": {
      "nodes": {
        "master": { "address": "192.168.1.0", "resolution": [ 800, 600 ], "fullscreen": false, "hardware_synced": false },
        "ig1": { "address": "192.168.1.1", "display": "dp_ig1", "resolution": [ 4096, 2160 ] },
        "ig2": { "address": "192.168.1.2", "display": "dp_ig2", "resolution": [ 4096, 2160 ] },
        "ig3": { "address": "192.168.1.3", "display": "dp_ig3", "resolution": [ 4096, 2160 ] },
        "ig4": { "address": "192.168.1.4", "display": "dp_ig4", "resolution": [ 4096, 2160 ] },
        "ig5": { "address": "192.168.1.5", "display": "dp_ig5", "resolution": [ 4096, 2160 ] },
        "ig6": { "address": "192.168.1.6", "display": "dp_ig6", "resolution": [ 4096, 2160 ] },
        "ig7": { "address": "192.168.1.7", "display": "dp_ig7", "resolution": [ 4096, 2160 ] },
        "ig8": { "address": "192.168.1.8", "display": "dp_ig8", "resolution": [ 4096, 2160 ] },
        "ig9": { "address": "192.168.1.9", "display": "dp_ig9", "resolution": [ 4096, 2160 ] },
        "ig10": { "address": "192.168.1.10", "display": "dp_ig10", "resolution": [ 4096, 2160 ] }
      },
      "displays": {
        "dp_base": { "type": "curved", "slices": 4, "height": 3.3, "radius": 3.0 },
        "dp_ig1": { "inherit": "dp_base", "angle_start": -192, "angle_end": -144 },
        "dp_ig2": { "inherit": "dp_base", "angle_start": -144, "angle_end": -97 },
        "dp_ig3": { "inherit": "dp_base", "angle_start": -117, "angle_end": -71 },
        "dp_ig4": { "inherit": "dp_base", "angle_start": -71, "angle_end": -23 },
        "dp_ig5": { "inherit": "dp_base", "angle_start": -50, "angle_end": 0 },
        "dp_ig6": { "inherit": "dp_base", "angle_start": 0, "angle_end": 47 },
        "dp_ig7": { "inherit": "dp_base", "angle_start": 24, "angle_end": 73 },
        "dp_ig8": { "inherit": "dp_base", "angle_start": 72, "angle_end": 119 },
        "dp_ig9": { "inherit": "dp_base", "angle_start": 95, "angle_end": 143 },
        "dp_ig10": { "inherit": "dp_base", "angle_start": 143, "angle_end": 192 }
      },
      "stereo": { "mode": "sidebyside", "separation": 0.065 },
      "cluster": { "master": "ig0", "address": "192.168.1.1", "framelock": "hardware", "data_port": 7777, "sync_port": 7778 }
    }
  }
}
```