using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace HEVS
{
	public class InputEditor : EditorWindow
	{
		enum AxisType { key, keyAxis, joystickAxis, joystickButton }

		enum ControlSet { standard, mouseKeyboard, logitechGamepad, viveController }

		/// <summary>
		/// Input Editor is an Editor Window for modifying Unity's Input Manager axes.
		/// Currently this is used to adding a list of all joystick axes and buttons. 
		/// </summary>
	//	[MenuItem("HEVS/Input Editor")]
		static void Init()
		{
			InputEditor window = (InputEditor)EditorWindow.GetWindow(typeof(InputEditor));
			window.Show();
		}

		void OnGUI()
		{
			GUILayout.Label("Modify Input Settings");
			if (GUILayout.Button("Clear Input"))
				ClearInput();

			if (GUILayout.Button("Create Joystick Axes and Buttons"))
				AddAxesSet();
			
			if (GUILayout.Button("Add Unity Dummy Bindings"))
				AddControlPreset(ControlSet.standard);

			GUILayout.Label("HEVS preset bindings");
			if(GUILayout.Button("Add Mouse and Keyboard"))
				AddControlPreset(ControlSet.mouseKeyboard);

			if (GUILayout.Button("Add Logitech Gamepad"))
				AddControlPreset(ControlSet.logitechGamepad);

			if (GUILayout.Button("Add Vive Controllers"))
				AddControlPreset(ControlSet.viveController); 
		}

		void ClearInput()
		{
			var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
			SerializedObject obj = new SerializedObject(inputManager);

			SerializedProperty axisArray = obj.FindProperty("m_Axes");
			axisArray.ClearArray();

			obj.ApplyModifiedProperties();
		}

		void AddControlPreset(ControlSet controlSet)
		{
			//Grab the input manager and open up its axes
			var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
			SerializedObject obj = new SerializedObject(inputManager);
			SerializedProperty axisArray = obj.FindProperty("m_Axes");

			switch (controlSet)
			{
				case ControlSet.standard:
					AddAxis(axisArray, "Vertical", AxisType.keyAxis, "");
					AddAxis(axisArray, "Horizontal", AxisType.keyAxis, "");
					AddAxis(axisArray, "Submit", AxisType.key, "");
					AddAxis(axisArray, "Cancel", AxisType.key, "");
					break;

				case ControlSet.mouseKeyboard:
					AddAxis(axisArray, "LeftX", AxisType.keyAxis, "d", "a");
					AddAxis(axisArray, "LeftY", AxisType.keyAxis, "w", "s");
					AddAxis(axisArray, "RightX", AxisType.keyAxis, "right", "left");
					AddAxis(axisArray, "RightY", AxisType.keyAxis, "down", "up");

					AddAxis(axisArray, "Alpha", AxisType.key, "space");
					AddAxis(axisArray, "Beta", AxisType.key, "left shift");
					AddAxis(axisArray, "Gamma", AxisType.key, "q");
					AddAxis(axisArray, "Delta", AxisType.key, "e");

					AddAxis(axisArray, "LeftPrimary", AxisType.key, "mouse 0");
					AddAxis(axisArray, "RightPrimary", AxisType.key, "mouse 1");

					AddAxis(axisArray, "LeftControl", AxisType.key, "tab");
					AddAxis(axisArray, "RightControl", AxisType.key, "mouse 2");

					break;

				case ControlSet.logitechGamepad:
					AddAxis(axisArray, "LeftX", AxisType.joystickAxis, 0);
					AddAxis(axisArray, "LeftY", AxisType.joystickAxis, 1);
					AddAxis(axisArray, "RightX", AxisType.joystickAxis, 2);
					AddAxis(axisArray, "RightY", AxisType.joystickAxis, 3);

					AddAxis(axisArray, "LeftZ", AxisType.joystickAxis, 4);
					AddAxis(axisArray, "RightZ", AxisType.joystickAxis, 5);

					AddAxis(axisArray, "Alpha", AxisType.joystickButton, "joystick button 0");
					AddAxis(axisArray, "Beta", AxisType.joystickButton, "joystick button 1");
					AddAxis(axisArray, "Gamma", AxisType.joystickButton, "joystick button 2");
					AddAxis(axisArray, "Delta", AxisType.joystickButton, "joystick button 3");

					AddAxis(axisArray, "LeftPrimary", AxisType.joystickButton, "joystick button 4");
					AddAxis(axisArray, "RightPrimary", AxisType.joystickButton, "joystick button 5");
					AddAxis(axisArray, "LeftSecondary", AxisType.joystickButton, "joystick button 6");
					AddAxis(axisArray, "RightSecondary", AxisType.joystickButton, "joystick button 7");

					AddAxis(axisArray, "LeftControl", AxisType.joystickButton, "joystick button 8");
					AddAxis(axisArray, "RightControl", AxisType.joystickButton, "joystick button 9");

					break;

				case ControlSet.viveController:
					AddAxis(axisArray, "LeftX", AxisType.joystickAxis, 1);
					AddAxis(axisArray, "LeftY", AxisType.joystickAxis, 2);
					AddAxis(axisArray, "RightX", AxisType.joystickAxis, 4);
					AddAxis(axisArray, "RightY", AxisType.joystickAxis, 5);

					AddAxis(axisArray, "LeftZ", AxisType.joystickAxis, 9);
					AddAxis(axisArray, "RightZ", AxisType.joystickAxis, 10);

					AddAxis(axisArray, "LeftPrimary", AxisType.joystickButton, "joystick button 14");
					AddAxis(axisArray, "RightPrimary", AxisType.joystickButton, "joystick button 15");

					AddAxis(axisArray, "LeftControl", AxisType.joystickButton, "joystick button 2");
					AddAxis(axisArray, "RightControl", AxisType.joystickButton, "joystick button 0");

					break;
			}

			//Apply changes to the Input Manager
			obj.ApplyModifiedProperties();
		}

		void AddAxis(SerializedProperty axisArray, string axisName, AxisType axisType, string positiveButton)
		{ AddAxis(axisArray, axisName, axisType, positiveButton, "", 0); }

		void AddAxis(SerializedProperty axisArray, string axisName, AxisType axisType, string positiveButton, string negativeButton)
		{ AddAxis(axisArray, axisName, axisType, positiveButton, negativeButton, 0); }

		void AddAxis(SerializedProperty axisArray, string axisName, AxisType axisType, int axis)
		{ AddAxis(axisArray, axisName, axisType, "", "", axis); }

		void AddAxis(SerializedProperty axisArray, string axisName, AxisType axisType, string positiveButton = "", string negativeButton = "", int axis = 0)
		{
			//Create new axis
			axisArray.arraySize++;
			var newAxis = axisArray.GetArrayElementAtIndex(axisArray.arraySize - 1);

			//Set name nad blank values
			newAxis.FindPropertyRelative("descriptiveName").stringValue = axisName;
			newAxis.FindPropertyRelative("descriptiveNegativeName").stringValue = "";

			newAxis.FindPropertyRelative("negativeButton").stringValue = "";
			newAxis.FindPropertyRelative("altNegativeButton").stringValue = "";
			newAxis.FindPropertyRelative("altPositiveButton").stringValue = "";

			//Set default values
			int type = 0;
			int joyNum = 0;
			float gravity = 0;
			float dead = 0;
			float sensitivity = 1f;
			bool snap = false;
			bool invert = false;

			//Overwrite values with axis type specific values
			switch (axisType)
			{
				case AxisType.key:
					gravity = 1000;
					dead = 0.001f;
					sensitivity = 1000;
					break;

				case AxisType.keyAxis:
					gravity = 3;
					dead = 0.001f;
					sensitivity = 3;
					snap = true;
					break; 

				case AxisType.joystickAxis:
					type = 2;
					joyNum = 0;
					gravity = 0f;
					dead = 0.19f;
					sensitivity = 1f;
					break;

				case AxisType.joystickButton:
					//positiveButton = axisName;
					break;
			}

			//Assign values to property. 
			newAxis.FindPropertyRelative("m_Name").stringValue = axisName;
			newAxis.FindPropertyRelative("positiveButton").stringValue = positiveButton;
			newAxis.FindPropertyRelative("negativeButton").stringValue = negativeButton; 

			newAxis.FindPropertyRelative("type").intValue = type;
			newAxis.FindPropertyRelative("axis").intValue = axis;
			newAxis.FindPropertyRelative("joyNum").intValue = joyNum;
			newAxis.FindPropertyRelative("gravity").floatValue = gravity;
			newAxis.FindPropertyRelative("dead").floatValue = dead;
			newAxis.FindPropertyRelative("sensitivity").floatValue = sensitivity;
			newAxis.FindPropertyRelative("snap").boolValue = snap;
			newAxis.FindPropertyRelative("invert").boolValue = invert;

		}

		void AddAxesSet()
		{
			//Grab the input manager and open up its axes
			var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
			SerializedObject obj = new SerializedObject(inputManager);
			SerializedProperty axisArray = obj.FindProperty("m_Axes");

			//Loop through each axis and button we want and add them. 
			int count;
			for (count = 0; count < 20; count++)
				AddAxis(axisArray, "joystick axis " + count, AxisType.joystickAxis, count);

			for (count = 0; count < 10; count++)
				AddAxis(axisArray, "joystick button " + count, AxisType.joystickButton, "joystick button 0");

			//Apply changes to the Input Manager
			obj.ApplyModifiedProperties();
		}
	}
}
