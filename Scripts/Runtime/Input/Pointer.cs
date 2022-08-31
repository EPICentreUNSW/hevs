using UnityEngine;
using System.Collections.Generic;

namespace HEVS
{
    /// <summary>
    /// A HEVS Pointer component for implementing a world-space pointing ray that can intersect with HEVS displays and UI elements.
    /// </summary>
    [AddComponentMenu("HEVS/Pointer")]
    public class Pointer : MonoBehaviour
    {
		/// <summary>
		/// A list of all pointers in the scene. 
		/// </summary>
        public static List<Pointer> pointerList = new List<Pointer>();

		/// <summary>
		/// The input name for button presses to submit interaction with the UI.
		/// CanvasManager will use this to check if the user has 'clicked' a UI element this pointer is pointing at.
		/// </summary>
        public string inputUISubmit = "Submit";

		/// <summary>
		/// The transform used to aim at things (self if not assigned)
		/// </summary>
		public Transform pointer;
        
		/// <summary>
		/// the world-space ray from the pointer
		/// </summary>
		public Ray pointerRay { get { return new Ray(pointer.position, pointer.forward); } }

		/// <summary>
		/// the "display-space" ray that should follow the visual projection
		/// </summary>
		public Ray pickRay;

		/// <summary>
		/// A transform which will have its position and facing matched to the pickRay origin and direction. 
		/// </summary>
		public Transform pickTransform;

		/// <summary>
		/// the display that has been intersected with
		/// </summary>
		public Display intersectedDisplay;

		/// <summary>
		/// a world-space point of intersection with a display
		/// </summary>
		[InspectorReadOnly]
        public Vector3 intersectedDisplayPoint;

		/// <summary>
		/// a display-space 2D intersection
		/// </summary>
		[InspectorReadOnly]
        public Vector2 intersectedDisplayPoint2D;

		/// <summary>
		/// Draw debug lines to show pickray directions?
		/// </summary>
        public bool debugDraw = true;

		/// <summary>
		/// mask objects for selection
		/// </summary>
		public LayerMask selectableLayerMask = 0;

		/// <summary>
		/// The last raycastHit made when raycasting with the pick ray each Update
		/// </summary>
        protected RaycastHit? _lastHit = null;

		/// <summary>
		/// Where a raycast made along the pickray stopped.
		/// Either an object it collided with or the main camera's far clip distance.
		/// </summary>
        public Vector3 pickRayEndPoint;

		/// <summary>
		/// The normal returned from a collision made by raycasting on the pickray. 
		/// </summary>
        public Vector3 pickRayHitNormal; 

        /// <summary>
        /// Access to the last RaycastHit result.
        /// </summary>
        public RaycastHit? lastHit
        {
            get { return _lastHit; }
        }

		/// <summary>
		/// Returns the pickray of the first pointer in the scene.
		/// Quick way to access pickray if you only have one.
		/// </summary>
		/// <returns>Returns the pickray for the first pointer.</returns>
        public static Ray GetPickRay()
        {
            return pointerList[0].pickRay;
        }

		/// <summary>
		/// Return the pick ray of the pointer from pointerList with the specified index.
		/// </summary>
		/// <param name="index">Index of the pointer to use.</param>
		/// <returns>Returns the pickray for the specified pointer.</returns>
        public static Ray GetPickRay(int index)
        {
            return pointerList[index].pickRay;
        }

        /// <summary>
        /// Return the pickray of the pointer with the specified name.
        /// </summary>
        /// <param name="pointerName">The name of the pointer to use.</param>
        /// <returns>Returns the pickray for the specified pointer.</returns>
        public static Ray GetPickRay(string pointerName)
        {
            foreach (Pointer pointer in pointerList)
            {
                if (pointer.gameObject.name == pointerName)
                    return pointer.pickRay;
            }

            return new Ray();
        }

        /// <summary>
        /// Returns the last ray hit GameObject, or null if there was no hit.
        /// </summary>
        /// <returns>Returns the last ray hit GameObject, or null if there was no hit.</returns>
        public GameObject SafeGetLastHitGameObject()
        {
            if (_lastHit.HasValue)
            {
                if (_lastHit.Value.collider != null)
                {
                    return _lastHit.Value.collider.gameObject;
                }
            }
            return null;
        }

        void Start()
        {
            pointerList.Add(this);

            if (pointer == null)
                pointer = transform;         

            pickRay = pointerRay;
        }

        void OnDestroy()
        {
            pointerList.Remove(this);
        }

        void Update()
        {
            intersectedDisplay = null;

            Vector3 sceneOrigin = SceneOrigin.position;
            Quaternion sceneOrientation = SceneOrigin.rotation;

            // if we're not on a known platform...
            // WAIT, THIS DOESN'T SEEM POSSIBLE
            if (Core.activePlatform == null)
            {
                // no display to intersect
                intersectedDisplay = null;

				// pick ray comes from pointer transform
				pickRay = new Ray(pointer.position, pointer.forward);

                // screen intersection point where the mouse is
                intersectedDisplayPoint2D = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                // world-space intersection point is on the near clip plane
                intersectedDisplayPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                float hitDistanceSqr = float.MaxValue;
                
                // attempt to intersect with displays
                foreach (Display display in Core.platformDisplays)
                {
                    var sp = sceneOrigin;
                    var sr = sceneOrientation;

                    if (display.config.transform != null)
                    {
                        if (display.config.transform.HasTranslation)
                            sp += sceneOrientation * display.config.transform.Translation;

                        if (display.config.transform.HasRotation)
                            sr *= display.config.transform.Rotation;
                    }

                    float d = float.MaxValue;
                    Vector2 hitPos2D = Vector2.zero;

                    if (display.Raycast(pointerRay, out d, out hitPos2D))
                    {
                        if ((d * d) < hitDistanceSqr)
                        {
                            hitDistanceSqr = d * d;

                            intersectedDisplay = display;
                            intersectedDisplayPoint2D = hitPos2D;
                            intersectedDisplayPoint = pointerRay.origin + pointerRay.direction * Mathf.Sqrt(hitDistanceSqr);

                            Vector3 camPos = intersectedDisplay.gameObject != null ? intersectedDisplay.gameObject.transform.position : Camera.main.transform.position;

                            pickRay = new Ray(camPos, Vector3.Normalize(intersectedDisplayPoint - camPos));
                        }
                    }
                    else
                        pickRay = pointerRay;
                }
            }

            if (debugDraw)
            {
                Debug.DrawLine(pickRay.origin, pickRay.origin + pickRay.direction * 10, Color.red);
                Debug.DrawLine(pointerRay.origin, pointerRay.origin + pointerRay.direction * 5);
            }

            if (pickTransform)
			{
				pickTransform.position = pickRay.origin;
				pickTransform.forward = pickRay.direction; 
			}

            // do pick
            RaycastHit[] hits = Physics.RaycastAll(pickRay, Camera.main.farClipPlane, selectableLayerMask);
            if (hits != null && hits.Length > 0)
            {
                _lastHit = hits[0];
                pickRayHitNormal = hits[0].normal;
                pickRayEndPoint = hits[0].point;
            }
            else
            {
                pickRayEndPoint = pickRay.origin + (pickRay.direction * Camera.main.farClipPlane); 
            }
        }
    }
}