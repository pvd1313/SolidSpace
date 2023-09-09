Unity 2D top down pixel physics game prototype.

![1](https://github.com/pvd1313/SolidSpace/assets/5685763/299f7248-0197-4daa-ba39-ad6883f31efd) ![2](https://github.com/pvd1313/SolidSpace/assets/5685763/0e3d9366-a7b6-4f2a-9e4f-e562e663d1b0)

# All in all
This project is a way far from being finished, but I believe there is something particually interesting you can scavenge for your very own game. Definitely, not all features are documented. Definitely, there is some fabrication. Originally this project uses Odin inspector / validator, add it into the local copy of repository to make it work.

# Tech stack
- Unity `2020.3.13`
- Jobs `0.8.0-preview.23`
- ECS `0.17.0-preview.41`
- UIElements `1.0.0-preview.14`
- Odin inspector / validator https://odininspector.com/
- Extenject (Zenject) https://github.com/Mathijs-Bakker/Extenject

# FuchaTools
Plugin, which allows:
- Open any editor window: `Window > Open...`
- Create any asset: `Create > Any...`
- Having path in project browser window title.

# SolidSpace.DependencyInjection
All DI containers in project are serialized as ScriptableObject `Assets/SolidSpace/Installers/`.

There is a tool for auto naming: `Assets/SolidSpace/Automation/AssetNameTool.asset`, it uses a `Type.Name` as a basis.

Plugin `Assets/Plugin/Zenject` is heavely cutted version of original. I recommend using original, get it here https://github.com/Mathijs-Bakker/Extenject

# SolidSpace.DataValidation
Runtime / button-time inspector / asset validation. It is based on Odin's validator, and disabled without it:

1. Impement `IDataValidator<T>` with generic type you want to validate.
2. Add `[InspectorDataValidatorAttribute]` to your class.
3. Forget about Odin's API.

Null references by default are threated as non acceptable and will trigger error, can be tweaked in `SerializeAttributeValidator`. Also there are checks for `Enums`. Look at implementations of `IDataValidator<T>` to inspect other validators.

# SolidSpace.Debugging
An editor window to display custom runtime values. Especially useful for values that change quickly.

`Window > Open... > SolidSpaceDebugWindow`

# SolidSpace.GameCycle
Controls update order. A great example of how you ***should not*** do in your game. There are two interfaces `IInitializable` and `IUpdatable`. There core problem here is in  `SolidSpace.GameCycle.asset`. It is a great misery to maintain such unreadable list. It would be better to define such thing with code, just create an array with all types. And it would be even better to make auto resolving order.

# SolidSpace.Profiling
A custom profiler: `Window > Open... > ProfilingTreeWindow`
- Allows you to estimate performance without pausing a game.
- Works faster than Unity profiler.
- Shows clear error if there is a mistmatch in Begin / End samples.
- Uses jobs and Burst.

# SolidSpace.Entities
Major part of this prototype is about ECS. Project does not use Unity' ECS `SystemBase`, everything is done directly with jobs, no linq like style is used to process data. 

### SolidSpace.Entities.Atlases
Every pixel is destructable, so every game sprite has to have unique texture. Creating texture for each sprite is not performance wise, so single texture with allocatable regions are used, it is like RAM, but for 2D textures. Texture is called an atlas and devided into squares called chunks, each chunk contains several sprites. It allows to allocate / release sprite texture with cheap constant time. Albeit, it will waste space when big sprites are used.

### SolidSpace.Entities.Physics.Colliders
Raycasting / detecting collision by iterating all entities is overhelming, so colliders are baked into spatial hash map, than accessed via grid. Process looks like:
- Collect all data from ECS
- Compute grid bounds
- Bind each collider to cell
- Create an optimized linear list containing all colliders

### SolidSpace.Entities.Physics.Raycasting
Allows ship sprite pixels be hitted by projectile pixels. It assumes that there no overlaping sprites. 

### SolidSpace.Entities.Physics.Rigidbody
Stops ship sprite from overlapping. Fancy (basic) physics stuff like inertia tensor is not implemented. Originally the key reason to create own physics system was in a requirement to create proper physics pixel to pixel interaction. What I mean is, continous collision detection, which allows bunch of pixels to be non-instantly destroyed when several rigidbodies interact.

### SolidSpace.Entities.Splitting
A pain in the ass, major feature of prototype, if entity is splitted in half, than there should be two or more entites with remaining sprites. It uses self invented flood fill algorithm, which recognize new shapes in two passes. 
