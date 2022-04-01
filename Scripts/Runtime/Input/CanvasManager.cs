using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HEVS
{
    /// <summary>
    /// Canvas Manager is attached to a UI canvas to make its UI elements interactable with a HEVS Pointer.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [AddComponentMenu("HEVS/Canvas Manager")]
    public class CanvasManager : MonoBehaviour
    {
        //list of all UI elements on this canvas that can be interacted with
        List<SelectableUIElement> elementList;

        //The rect transform of this canvas
        RectTransform rectTransform; 

        bool lookedAtCanvasLastUpdate = false;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            bool lookingAtCanvas = false;

            //Check if any pointers are looking at the canvas
            //Also make a list of pointers which are looking at this canvas
            List<Pointer> activePointerList = new List<Pointer>();
            foreach(Pointer pointer in Pointer.pointerList)
            {
                if(IsLookingAtRect(pointer, rectTransform))
                {
                    lookingAtCanvas = true;
                    activePointerList.Add(pointer);
                }
            }

            //If pointers are looking, and weren't last frame, build a list of current UI elements
            //Building now allows us to track elements created dynamically
            if(lookingAtCanvas && !lookedAtCanvasLastUpdate)
                BuildElementList();

            //Loop through pointers looking at the canvas to see if they are looking at
            //at specific UI elements and update those elements if so
            foreach(Pointer pointer in activePointerList)
            {
				foreach(SelectableUIElement element in elementList)
                {
                    if(!element.selectable)
                        continue;
                    
                    if(element.selectable.interactable && element.selectable.gameObject.activeInHierarchy)
                    {
                        if(IsLookingAtRect(pointer, element.rectTransform))
                            element.UpdateSelected(pointer);
                    }
                }

                //If the user makes a submit, the UI might change
                //so pretend they weren't looking at this canvas last update
                //and we'll rebuild the canvas next update
                if(Input.GetButton(pointer.inputUISubmit))
                    lookingAtCanvas = false;            
            }

            lookedAtCanvasLastUpdate = lookingAtCanvas; 

        }

        //clears the element list and rebuilds it
        //calls a recursive function to crawl through child transforms
        void BuildElementList()
        {
			elementList = new List<SelectableUIElement>();
            foreach(Transform child in transform)
                BuildElementList(child);
        }

        //Recursive function to find Selectable components and add them to elementsList
        //Creates a SelectableUIElement to track each Selectable 
        void BuildElementList(Transform target)
        {
			if(target.gameObject.activeInHierarchy)
            {
                Selectable selectable = target.GetComponent<Selectable>();
                if(selectable)
                    elementList.Add(new SelectableUIElement(selectable));
            }

            foreach(Transform child in target)
                BuildElementList(child);
        }

        /// <summary>
        /// Check if pointer is pointing at a point inside rectTransform
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="rectTransform"></param>
        /// <returns>Returns true if the Pointer is looking at the RecTransform.</returns>
        public static bool IsLookingAtRect(Pointer pointer, RectTransform rectTransform)
        {
            Vector2 position = PointerRectPosition(pointer, rectTransform);

            if( 
                (position.y <= 1) && (position.y >= 0) 
                &&
                (position.x <= 1) && (position.x >= 0)
            )
                return true;
            else
                return false;
        }

        /// <summary>
        /// Finds the relative position of a pointer's pickRay inside the provided rectTransform.
        /// </summary>
        /// <param name="pointer">The pointer to query.</param>
        /// <param name="rectTransform">The rect transform to check the pointer with.</param>
        /// <returns>Returns postion as Vector2.</returns>
        public static Vector2 PointerRectPosition(Pointer pointer, RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4]; 
            rectTransform.GetWorldCorners(corners);
            Plane plane = new Plane(corners[0], corners[1], corners[2]);

			if (plane.Raycast(pointer.pickRay, out float distanceToPlane))
			{
				Vector2 position = Vector2.zero;
				Vector3 pickPoint = pointer.pickRay.origin + (pointer.pickRay.direction * distanceToPlane);
				pickPoint = rectTransform.InverseTransformPoint(pickPoint);

				Vector3 lowerLeft = rectTransform.InverseTransformPoint(corners[0]);
				Vector3 upperRight = rectTransform.InverseTransformPoint(corners[2]);

				position.x = (pickPoint.x - lowerLeft.x) / (upperRight.x - lowerLeft.x);
				position.y = (pickPoint.y - lowerLeft.y) / (upperRight.y - lowerLeft.y);

				return position;
			}
			else
				return new Vector2(-1, -1);
		}

    }

    /// <summary>
    /// A Unity UI Selectable component, and the means to manipulate it.
    /// On creation we store what type of Selectable it is, so later we can adjust behaviour accordingly.
    /// </summary>
    public class SelectableUIElement
    {
        /// <summary>
        /// Enumeration of possible selectable UI elements.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Unknown element type.
            /// </summary>
            Other,
            /// <summary>
            /// Button element type.
            /// </summary>
            Button,
            /// <summary>
            /// ScrollBar element type.
            /// </summary>
            ScollBar,
            /// <summary>
            /// Toggle element type.
            /// </summary>
            Toggle,
            /// <summary>
            /// Dropdown element type.
            /// </summary>
            Dropdown,
            /// <summary>
            /// Slider element type.
            /// </summary>
            Slider,
            /// <summary>
            /// Input Field element type. 
            /// </summary>
            InputField,
            /// <summary>
            /// Dropdown element type.
            /// </summary>
            TMPDropdown
        };

        /// <summary>
        /// The type of this selectable UI element.
        /// </summary>
        public Type type;

		/// <summary>
		/// Access to the selectable element.
		/// </summary>
		public Selectable selectable;

        /// <summary>
        /// The rect transform that this UI element uses.
        /// </summary>
        public RectTransform rectTransform;


		/// <summary>
		/// Construct a SelectableUIElement with a particular Selectable object.
		/// </summary>
		/// <param name="inElement">The selectable object.</param>
		public SelectableUIElement(Selectable inElement)
        {
            selectable = inElement;
            rectTransform = selectable.GetComponent<RectTransform>();

            FindType();
        }

        /// <summary>
        /// Called each Update() the provided pointer is pointing at this selectable's Rect Transform.
        /// If the pointer's submit button is pressed we want to use the Selectable.
        /// </summary>
        /// <param name="pointer">The Pointer to use when updating this selectable.</param>
        public void UpdateSelected(Pointer pointer)
        {
			if (type == Type.InputField)
			{
				if (Input.GetButtonDown(pointer.inputUISubmit) && selectable.interactable)
					selectable.Select(); 
			}
			else
			{
				selectable.Select();

				if (Input.GetButtonDown(pointer.inputUISubmit) && selectable.interactable)
					Submit(pointer);

				if (Input.GetButton(pointer.inputUISubmit) && selectable.interactable)
					Hold(pointer);
			}
        }

        //The Selectable was clicked by provided pointer
        void Submit(Pointer pointer)
        {

			switch(type)
            {
				case Type.Button:
                    UnityEngine.UI.Button button = selectable as UnityEngine.UI.Button;
                    button.onClick.Invoke();
                    break;
                
                case Type.Dropdown:
                    UnityEngine.UI.Dropdown dropdown = selectable as UnityEngine.UI.Dropdown;
                    dropdown.Show(); 
                    break;

                case Type.TMPDropdown:
                    TMPro.TMP_Dropdown tmp_dropdown = selectable as TMPro.TMP_Dropdown;
                    tmp_dropdown.Show();
                    break;

                case Type.Toggle:
                    UnityEngine.UI.Toggle toggle = selectable as UnityEngine.UI.Toggle;
                    toggle.isOn = !toggle.isOn;
                    break;
            }
        }

        //The Selectable is being held down this Update()
        void Hold(Pointer pointer)
        {
            switch(type)
            {
                case Type.Slider:
                    HoldSlider(pointer);
                    break;
                
                case Type.ScollBar:
                    HoldScrollbar(pointer);
                    break;
            }
        }

        //The Slider is being dragged by the pointer
        //We can grap the relative postion of the pointer inside the Rect Transform and
        //convert that to the slider's value, according to the Slider's direction.
        void HoldSlider(Pointer pointer)
        {
			Vector2 position = CanvasManager.PointerRectPosition(pointer, rectTransform);
            UnityEngine.UI.Slider slider = selectable as UnityEngine.UI.Slider;

            switch(slider.direction)
            {
                case UnityEngine.UI.Slider.Direction.LeftToRight:
                    slider.value = position.x; 
                    break;
                
                case UnityEngine.UI.Slider.Direction.RightToLeft:
                    slider.value = 1 - position.x; 
                    break;
                
                case UnityEngine.UI.Slider.Direction.TopToBottom:
                    slider.value = 1 - position.y; 
                    break;
                
                case UnityEngine.UI.Slider.Direction.BottomToTop:
                    slider.value = position.y; 
                    break;
            }
        }

        //Same as Slider, but it's a scrollbar. 
        void HoldScrollbar(Pointer pointer)
        {
			Vector2 position = CanvasManager.PointerRectPosition(pointer, rectTransform);
            Scrollbar scrollbar = selectable as Scrollbar; 

            switch(scrollbar.direction)
            {
                case Scrollbar.Direction.LeftToRight:
                    scrollbar.value = position.x; 
                    break;
                
                case Scrollbar.Direction.RightToLeft:
                    scrollbar.value = 1 - position.x; 
                    break;
                
                case Scrollbar.Direction.TopToBottom:
                    scrollbar.value = 1 - position.y; 
                    break;
                
                case Scrollbar.Direction.BottomToTop:
                    scrollbar.value = position.y; 
                    break;
            }
        }

        void FindType()
        {
			switch(selectable.GetType().ToString())
            {
                case "UnityEngine.UI.Button":
                    type = Type.Button;
                    break;
                
                case "UnityEngine.UI.Scrollbar":
                    type = Type.ScollBar; 
                    break;
                
                case "UnityEngine.UI.Toggle":
                    type = Type.Toggle;
                    break;

                case "UnityEngine.UI.Dropdown":
                    type = Type.Dropdown;
                    break;
                
                case "UnityEngine.UI.Slider":
                    type = Type.Slider;
                    break;

				case "UnityEngine.UI.InputField":
					type = Type.InputField;
					break;

                case "TMPro.TMP_Dropdown":
                    type = Type.TMPDropdown;
                    break;

                default:
                    Debug.Log($"HEVS: {selectable.name}'s type of {selectable.GetType().ToString()} not supported.");
                    type = Type.Other;
                    break;
            }
        }
    }
}