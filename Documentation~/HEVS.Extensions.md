## `ArrayExtensions`

Extension methods for `System.Array`.
```csharp
public static class HEVS.Extensions.ArrayExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerable<Int32[]>` | GetIndices(this `Array` self) | Returns a sequence of all valid indices for the array. | 
| `Int32[]` | GetShape(this `Array` self) | Returns the shape of the array as an array of `System.Int32`. | 


## `CameraExtensions`

A utility class for extension methods.
```csharp
public static class HEVS.Extensions.CameraExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | SetAndActivateTargetDisplay(this `Camera` camera, `Int32` display) | Activates a target display, if available, and assigns the camera to it. | 


## `DictionaryExtensions`

Extension methods for `System.Collections.Generic.IDictionary`2`.
```csharp
public static class HEVS.Extensions.DictionaryExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `TValue` | GetValueOrDefault(this `IDictionary<TKey, TValue>` dictionary, `TKey` key, `TValue` defaultValue = null) | Return the value for key if key is in the dictionary, else defaultValue. If defaultValue is not given, it defaults to <c>default(TValue)</c>, so that this method never raises a `System.Collections.Generic.KeyNotFoundException`. | 


## `EnumerableExtensions`

Extension methods for `System.Collections.IEnumerable` and `System.Collections.Generic.IEnumerable`1`.
```csharp
public static class HEVS.Extensions.EnumerableExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | All(this `IEnumerable<Boolean>` self) | Check whether all elements of a sequence of `System.Boolean` are true. | 
| `Boolean` | Any(this `IEnumerable<Boolean>` self) | Check whether any element of a sequence of `System.Boolean` is true. | 
| `IEnumerable<IEnumerable<T>>` | CartesianProduct(this `IEnumerable<IEnumerable<T>>` self) | Computes the cartesian product of sequences. | 
| `IEnumerable<TAccumulate>` | CumulativeAggregate(this `IEnumerable<TSource>` self, `TAccumulate` seed, `Func<TAccumulate, TSource, TAccumulate>` func) | Cumulatively applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value. | 
| `IEnumerable<Int32>` | CumulativeProduct(this `IEnumerable<Int32>` self) | Computes the cumulative product of a sequence of numeric values. | 
| `IEnumerable<Int64>` | CumulativeProduct(this `IEnumerable<Int64>` self) | Computes the cumulative product of a sequence of numeric values. | 
| `IEnumerable<Single>` | CumulativeProduct(this `IEnumerable<Single>` self) | Computes the cumulative product of a sequence of numeric values. | 
| `IEnumerable<Double>` | CumulativeProduct(this `IEnumerable<Double>` self) | Computes the cumulative product of a sequence of numeric values. | 
| `IEnumerable<Int32>` | CumulativeSum(this `IEnumerable<Int32>` self) | Computes the cumulative sum of a sequence of numeric values. | 
| `IEnumerable<Int64>` | CumulativeSum(this `IEnumerable<Int64>` self) | Computes the cumulative sum of a sequence of numeric values. | 
| `IEnumerable<Single>` | CumulativeSum(this `IEnumerable<Single>` self) | Computes the cumulative sum of a sequence of numeric values. | 
| `IEnumerable<Double>` | CumulativeSum(this `IEnumerable<Double>` self) | Computes the cumulative sum of a sequence of numeric values. | 
| `Int32` | Product(this `IEnumerable<Int32>` self) | Computes the product of a sequence of numeric values. | 
| `Int64` | Product(this `IEnumerable<Int64>` self) | Computes the product of a sequence of numeric values. | 
| `Double` | Product(this `IEnumerable<Single>` self) | Computes the product of a sequence of numeric values. | 
| `Double` | Product(this `IEnumerable<Double>` self) | Computes the product of a sequence of numeric values. | 
| `OrderedDictionary` | ToOrderedDictionary(this `IEnumerable<T>` self, `Func<T, TKey>` key, `Func<T, TValue>` value) | Creates a `System.Collections.Specialized.OrderedDictionary` from an `System.Collections.Generic.IEnumerable`1`. | 


## `GameObjectExtensions`

A utility class for extension methods.
```csharp
public static class HEVS.Extensions.GameObjectExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CopyComponentsOfTypeFrom(this `GameObject` target, `GameObject` source) | Copy MonoBehavour components of a certain type to a target GameObject from a source GameObject. | 
| `void` | CopyComponentsOfTypeTo(this `GameObject` source, `GameObject` target) | Copy MonoBehavour components of a certain type from a source GameObject to a target GameObject. | 
| `Transform` | FindChildIncludingDeactivated(this `Transform` parent, `String` name) | Searches for a child GameObject on a specified Transform, including searching through deactivated children. | 
| `T` | GetComponentFromChildrenInactive(this `GameObject` go) | Searches for the first instance of a MonoBehaviour in a GameObject's children, including searching through deactivated children. | 
| `List<T>` | GetComponentFromList(this `List<Transform>` items) | Gathers all instances of a component from a list of Transforms. | 
| `List<T>` | GetComponentsInChildrenFromList(this `List<Transform>` items) | Gathers all instances of a component from the children of a list of Transforms. | 
| `T` | GetOrAddComponent(this `GameObject` go) | Attempts to get a component from a GameObject. If the component doesn't exist then one is added. | 
| `void` | PlayAudio(this `GameObject` gameObject, `AudioClip` clip) | Plays a looping AudioClip on a specified GameObject. If the GameObject doesn't have an AudioSource then one is added. | 
| `void` | PlayOneShot(this `GameObject` gameObject, `AudioClip` clip) | Plays a non-looping AudioClip on a specified GameObject. If the GameObject doesn't have an AudioSource then one is added. | 


## `MathematicsExtensions`

An extension utility class.
```csharp
public static class HEVS.Extensions.MathematicsExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Vector3` | Abs(this `Vector3` v) |  | 
| `Quaternion` | conjugate(this `Quaternion` q) | Find the Quaternion's conjugate. | 
| `Single` | magnitude(this `Quaternion` q) | Find the Quaternion's magnitude. | 
| `Color` | MoveTowards(this `Color` orig, `Color` target, `Single` amount) | Interpolates the color towards a target color. | 
| `Single` | PositiveMod(this `Single` x, `Single` m = 1) | A positive modulo for floats. | 
| `Int32` | PositiveMod(this `Int32` x, `Int32` m) | A positive modulo for floats. | 
| `Vector3` | rgb(this `Vector4` v) | Sqizzles the vector. | 
| `Vector2` | xx(this `Vector2` v) | Sqizzles the vector. | 
| `Vector2` | xx(this `Vector3` v) | Sqizzles the vector. | 
| `Vector3` | xxx(this `Vector3` v) | Sqizzles the vector. | 
| `Vector4` | xxyy(this `Vector4` v) | Sqizzles the vector. | 
| `Vector2` | xy(this `Vector3` v) | Sqizzles the vector. | 
| `Vector2` | xy(this `Vector4` v) | Sqizzles the vector. | 
| `Vector3` | xyz(this `Vector4` v) | Sqizzles the vector. | 
| `Vector2` | xz(this `Vector3` v) | Sqizzles the vector. | 
| `Vector2` | xz(this `Vector4` v) | Sqizzles the vector. | 
| `Vector2` | yx(this `Vector2` v) | Sqizzles the vector. | 
| `Vector2` | yy(this `Vector2` v) | Sqizzles the vector. | 
| `Vector2` | yz(this `Vector3` v) | Sqizzles the vector. | 
| `Vector4` | zzww(this `Vector4` v) | Sqizzles the vector. | 


## `Texture2DExtensions`

A utility class for extension methods.
```csharp
public static class HEVS.Extensions.Texture2DExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | LoadFloatingpointTiff(this `Texture2D` tex, `String` filename) | Loads a 32bit floating point RGB Tiff into the texture | 


## `TypeExtensions`

Extension methods for `System.Type`.
```csharp
public static class HEVS.Extensions.TypeExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Object` | GetDefaultValue(this `Type` self) | Runtime equivalent of <c>default(T)</c>. | 
| `Int32` | GetSize(this `Type` self) |  | 
| `Boolean` | IsUnManaged(this `Type` self) |  | 


