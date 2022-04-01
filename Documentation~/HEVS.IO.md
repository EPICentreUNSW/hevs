## `ObjFile`

```csharp
public class HEVS.IO.ObjFile

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<ObjMaterial>` | materials |  | 
| `List<ObjMesh>` | meshes |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | CreateSceneGraph() |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Mesh` | Load(`TextAsset` objFile) | Loads an OBJ from a Unity TextAsset. | 
| `Mesh` | Load(`TextAsset` objFile, `Boolean` flipV) | Loads an OBJ from a Unity TextAsset. | 


## `ObjGeometryMode`

```csharp
public enum HEVS.IO.ObjGeometryMode
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Point |  | 
| `1` | Line |  | 
| `2` | Triangle |  | 


## `ObjIlluminationMode`

```csharp
public enum HEVS.IO.ObjIlluminationMode
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | ColorOnAmbientOff |  | 
| `1` | ColorOnAmbientOn |  | 
| `2` | HighlightOn |  | 
| `3` | ReflectionOnRayTraceOn |  | 
| `4` | GlassOnRayTraceOn |  | 
| `5` | FresnelOnRayTraceOn |  | 
| `6` | RefractionOnFresnelOffRayTraceOn |  | 
| `7` | RefractionOnFresnelOnRayTraceOn |  | 
| `8` | ReflectionOnRayTraceOff |  | 
| `9` | GlassOnRayTraceOff |  | 
| `10` | CastShadowsOntoInvisible |  | 


## `ObjMaterial`

```csharp
public class HEVS.IO.ObjMaterial

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Texture` | alphaTexture |  | 
| `Color` | ambient |  | 
| `Texture` | ambientTexture |  | 
| `Texture` | bumpTexture |  | 
| `Texture` | decalTexture |  | 
| `Color` | diffuse |  | 
| `Texture` | diffuseTexture |  | 
| `Texture` | displacementTexture |  | 
| `ObjIlluminationMode` | illum |  | 
| `String` | name |  | 
| `Single` | opacity |  | 
| `Color` | specular |  | 
| `Single` | specularExponent |  | 
| `Texture` | specularHighlightTexture |  | 
| `Texture` | specularTexture |  | 


## `ObjMesh`

```csharp
public class HEVS.IO.ObjMesh

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<Int32>` | indices |  | 
| `ObjMaterial` | material |  | 
| `List<Vector3>` | normals |  | 
| `List<Vector2>` | uvs |  | 
| `List<Vector3>` | vertices |  | 


## `PotreeMesh`

```csharp
public class HEVS.IO.PotreeMesh

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Bounds` | boundingBox |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Vector3` | boundingBoxPosition() |  | 
| `Vector3` | boundingBoxScale() |  | 
| `void` | cleanup() |  | 
| `GameObject` | loadInBackground() |  | 
| `void` | updateMeshes(`Material` cloudMaterial) |  | 


