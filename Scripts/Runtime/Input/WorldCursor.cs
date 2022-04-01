using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// A HEVS component to add a world-space cursor to a GameObject that uses a HEVS Pointer to position itself.
    /// </summary>
    [AddComponentMenu("HEVS/Cursor")]
    public class WorldCursor : MonoBehaviour
    {
        /// <summary>
        /// An enumeration for specifying the type of collision to use for positioning.
        /// </summary>
        public enum CursorPosition
        {
            /// <summary>
            /// Screen-space collision.
            /// </summary>
            Screen,
            /// <summary>
            /// World-space cursor colliding with the scene.
            /// </summary>
            Collide
        }


        /// <summary>
        /// What the cursor's up vector should be based on. 
        /// </summary>
        public enum CursorOrientation
        {
            /// <summary>
            /// No orientation applied. 
            /// </summary>
            None,

            /// <summary>
            /// Cursor's up matches the pointer.
            /// </summary>
            Pointer,


            /// <summary>
            /// Cursor's up matches the transform root of the pointer. 
            /// </summary>
            PointerRoot,


            /// <summary>
            /// Cursor's up matches the world.
            /// </summary>
            World

        }


		/// <summary>
		/// Position the cursor appears at relative to the pickray.
		/// Screen - on the screen, where pick ray is pointing.
		/// Collide - before the pick ray collides with something in the world, and flat against that surface.
		/// </summary>
		public CursorPosition cursorPosition = CursorPosition.Screen;

        /// <summary>
        /// What the cursor's up vector should be based on. 
        /// </summary>
        public CursorOrientation cursorOrientation = CursorOrientation.World; 

		/// <summary>
		/// How far between the screen and the collision the cursor should appear, when cursorPosition is collide.
		/// 0 - screen, 1 - collison, 0.5 - halfway
		/// </summary>
        public float appearAtCollideDepth = 0.9f; 

		/// <summary>
		/// The pointer the cursor will mirror
		/// </summary>
        public Pointer pointer;

		/// <summary>
		/// If assigned the sprite and beam (line renderer) will be set to this color.
		/// </summary>
        public Color color = Color.black;

		/// <summary>
		/// A line renderer which will follow the pick ray's origin to end point.
		/// </summary>
        public LineRenderer beam;

		/// <summary>
		/// A sprite representing the cursor.
		/// If set will have its color property set to the cursor's color. (Sprite's color should be white for this to work well)
		/// Sprite is assumed to be parented to the cursor object, and thus be moved with it. 
		/// </summary>
        public SpriteRenderer sprite;

        /// <summary>
        /// Access to the Pointer's pick ray.
        /// </summary>
        public Ray pickRay { get {return pointer.pickRay;} } 

        void Awake()
        {
            if(sprite)
                sprite.color = color;

            if(!beam)
                beam = GetComponent<LineRenderer>();

            if(beam)
            {
                beam.positionCount = 2;
                beam.startColor = color;
                beam.endColor = color; 
            }
        }

        void LateUpdate()
        {
            switch(cursorPosition)
            {
                case CursorPosition.Screen:
                    transform.position = pointer.intersectedDisplayPoint; 
                    transform.forward = pointer.pickRay.direction;
                    break;
                
                case CursorPosition.Collide:
                    float distanceToCollide = Vector3.Distance(pointer.pickRay.origin, pointer.pickRayEndPoint);
                    float depth = distanceToCollide * appearAtCollideDepth; 

                    transform.position = Vector3.MoveTowards(pointer.pickRay.origin, pointer.pickRayEndPoint, depth); 
                    transform.forward = -pointer.pickRayHitNormal;
                    break; 
            }

            switch(cursorOrientation)
            {
                case CursorOrientation.World:
                    transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                    break;

                case CursorOrientation.Pointer:
                    transform.rotation = Quaternion.LookRotation(transform.forward, pointer.transform.up);
                    break;

                case CursorOrientation.PointerRoot:
                    transform.rotation = Quaternion.LookRotation(transform.forward, pointer.transform.root.up); 
                    break;
            }
            
            if(beam)
            {
                beam.SetPosition(0, pointer.pickRay.origin);
                beam.SetPosition(1, pointer.pickRayEndPoint); 
            } 
        }
    }   
}
