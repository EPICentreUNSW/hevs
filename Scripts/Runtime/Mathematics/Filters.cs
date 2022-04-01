using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HEVS.Extensions;

namespace HEVS.Filter
{
	/// <summary>
	/// Simple low-pass filter.
	/// </summary>
	public class LowPassFilter
	{
		/// <summary>
		/// Value from the previous filter step.
		/// </summary>
		public float previousValue;

		/// <summary>
		/// Alpha value for controlling the filter.
		/// </summary>
		public float alpha;

		/// <summary>
		/// Create a low-pass filter.
		/// </summary>
		/// <param name="alpha">The alpha value to control the filter.</param>
		/// <param name="initialValue">The initial starting value for the filter.</param>
		public LowPassFilter(float alpha, float initialValue)
		{
			previousValue = initialValue;
			this.alpha = alpha;
		}

		/// <summary>
		/// Apply a filter step.
		/// </summary>
		/// <param name="value">The current value to filter.</param>
		/// <returns>Returns the filtered value.</returns>
		public float Filter(float value)
		{
			float result = alpha * value + (1.0f - alpha) * previousValue;
			previousValue = value;
			return result;
		}

		/// <summary>
		/// Apply a filter step and modify the alpha value.
		/// </summary>
		/// <param name="value">The current value to filter.</param>
		/// <param name="alpha">The new alpha value.</param>
		/// <returns>Returns the filtered value.</returns>
		public float FilterWithAlpha(float value, float alpha)
		{
			this.alpha = alpha;
			return Filter(value);
		}
	}

/*	public class OneEuroFilter
	{
		public float frequency;
		public float mincutoff;
		public float beta;
		public float dcutoff;

		LowPassFilter x;
		LowPassFilter dx;

		float alpha(float cutoff)
		{
			float te = 1.0f / frequency;
			float tau = 1.0f / (2 * Mathf.PI * cutoff);
			return 1.0f / (1.0f + tau / te);
		}		

		public OneEuroFilter(float startingValue, float frequency, float mincutoff = 1.0f, float beta = 0.0f, float dcutoff = 1.0f)
		{
			this.frequency = frequency;
			this.mincutoff = mincutoff;
			this.beta = beta;
			this.dcutoff = dcutoff;
			x = new LowPassFilter(alpha(mincutoff), startingValue);
			dx = new LowPassFilter(alpha(dcutoff), 1.0f);
		}

		public float Filter(float value, float delta)
		{
			// update the sampling frequency based on timestamps
			frequency = 1.0f / delta;
			// estimate the current variation per second 
			float dvalue = (value - x.previousValue) * frequency;
			float edvalue = dx.FilterWithAlpha(dvalue, alpha(dcutoff));
			// use it to update the cutoff frequency
			float cutoff = mincutoff + beta * Math.Abs(edvalue);
			// filter the given value
			return x.FilterWithAlpha(value, alpha(cutoff));
		}
	}*/

	/// <summary>
	/// A One Euro Filter implementation.
	/// </summary>
	public class OneEuroFilter
	{
		/// <summary>
		/// The minimum cutoff for the filter.
		/// </summary>
		public float min_cutoff { get; private set; }
		/// <summary>
		/// The beta value for controlling the filter.
		/// </summary>
		public float beta { get; private set; }
		/// <summary>
		/// The delta cutoff or the filter.
		/// </summary>
		public float d_cutoff { get; private set; }

		float previousValue;
		float previousDerivative;

		/// <summary>
		/// Create a One Euro Filter.
		/// </summary>
		/// <param name="startingValue">The starting value for the filter.</param>
		/// <param name="startingDerivative">The starting derivative for the filter (default: 1.0)</param>
		/// <param name="min_cutoff">The minimum cutoff for the filter (default: 1.0)</param>
		/// <param name="beta">The filter's beta value (default: 0.0)</param>
		/// <param name="d_cutoff">The delta cutoff for the filter (default: 1.0)</param>
		public OneEuroFilter(float startingValue, float startingDerivative = 1.0f, float min_cutoff = 1.0f, float beta = 0.0f, float d_cutoff = 1.0f)
		{
			this.min_cutoff = min_cutoff;
			this.beta = beta;
			this.d_cutoff = d_cutoff;

			previousValue = startingValue;
			previousDerivative = startingDerivative;
		}

		/// <summary>
		/// Apply a filter step.
		/// </summary>
		/// <param name="currentValue">The value to filter.</param>
		/// <param name="delta">The delta of the step.</param>
		/// <returns>Returns the filtered value.</returns>
		public float Filter(float currentValue, float delta)
		{
			// The filtered derivative of the signal.
			float a_d = alpha(delta, d_cutoff);
			float valueDerivative = (currentValue - previousValue) / delta;
			float dx_hat = lowPassFilter(a_d, valueDerivative, previousDerivative);

			// The filtered signal.
			float cutoff = min_cutoff + beta * Mathf.Abs(dx_hat);
			float a = alpha(delta, cutoff);
			float x_hat = lowPassFilter(a, currentValue, previousValue);

			// Memorize the previous values.
			previousValue = x_hat;
			previousDerivative = dx_hat;

			return x_hat;
		}

		float alpha(float t_e, float cutoff)
		{
			float r = 2 * Mathf.PI * cutoff * t_e;
			return r / (r + 1);
		}

		float lowPassFilter(float a, float x, float x_prev)
		{
			return a * x + (1 - a) * x_prev;
		}
	}

	/// <summary>
	/// A One Euro Filter applied to a Vector2.
	/// </summary>
	public class OneEuroFilterVector2
	{
		OneEuroFilter x, y;

		/// <summary>
		/// Constructs a One Euro Filter for a Vector2.
		/// </summary>
		/// <param name="startingValue">The starting value for the filter.</param>
		/// <param name="startingDerivative">The starting derivative for the filter (default: 1.0)</param>
		/// <param name="min_cutoff">The minimum cutoff for the filter (default: 1.0)</param>
		/// <param name="beta">The filter's beta value (default: 0.0)</param>
		/// <param name="d_cutoff">The delta cutoff for the filter (default: 1.0)</param>
		public OneEuroFilterVector2(Vector2 startingValue, float startingDerivative = 1.0f, float min_cutoff = 1.0f, float beta = 0.0f, float d_cutoff = 1.0f)
		{
			x = new OneEuroFilter(startingValue.x, startingDerivative, min_cutoff, beta, d_cutoff);
			y = new OneEuroFilter(startingValue.y, startingDerivative, min_cutoff, beta, d_cutoff);
		}

		/// <summary>
		/// Apply a filter step.
		/// </summary>
		/// <param name="currentValue">The value to filter.</param>
		/// <param name="delta">The delta of the step.</param>
		/// <returns>Returns the filtered value.</returns>
		public Vector2 Filter(Vector2 currentValue, float delta)
		{
			return new Vector2(x.Filter(currentValue.x, delta),
							   y.Filter(currentValue.y, delta));
		}
	}

	/// <summary>
	/// A One Euro Filter applied to a Vector3.
	/// </summary>
	public class OneEuroFilterVector3
	{
		OneEuroFilter x, y, z;

		/// <summary>
		/// Constructs a One Euro Filter for a Vector3.
		/// </summary>
		/// <param name="startingValue">The starting value for the filter.</param>
		/// <param name="startingDerivative">The starting derivative for the filter (default: 1.0)</param>
		/// <param name="min_cutoff">The minimum cutoff for the filter (default: 1.0)</param>
		/// <param name="beta">The filter's beta value (default: 0.0)</param>
		/// <param name="d_cutoff">The delta cutoff for the filter (default: 1.0)</param>
		public OneEuroFilterVector3(Vector3 startingValue, float startingDerivative = 1.0f, float min_cutoff = 1.0f, float beta = 0.0f, float d_cutoff = 1.0f)
		{
			x = new OneEuroFilter(startingValue.x, startingDerivative, min_cutoff, beta, d_cutoff);
			y = new OneEuroFilter(startingValue.y, startingDerivative, min_cutoff, beta, d_cutoff);
			z = new OneEuroFilter(startingValue.z, startingDerivative, min_cutoff, beta, d_cutoff);
		}

		/// <summary>
		/// Apply a filter step.
		/// </summary>
		/// <param name="currentValue">The value to filter.</param>
		/// <param name="delta">The delta of the step.</param>
		/// <returns>Returns the filtered value.</returns>
		public Vector3 Filter(Vector3 currentValue, float delta)
		{
			return new Vector3(x.Filter(currentValue.x, delta),
								y.Filter(currentValue.y, delta),
								z.Filter(currentValue.z, delta));
		}
	}

	/// <summary>
	/// A One Euro Filter applied to a Vector4.
	/// </summary>
	public class OneEuroFilterVector4
	{
		OneEuroFilter x, y, z, w;

		/// <summary>
		/// Constructs a One Euro Filter for a Vector4.
		/// </summary>
		/// <param name="startingValue">The starting value for the filter.</param>
		/// <param name="startingDerivative">The starting derivative for the filter (default: 1.0)</param>
		/// <param name="min_cutoff">The minimum cutoff for the filter (default: 1.0)</param>
		/// <param name="beta">The filter's beta value (default: 0.0)</param>
		/// <param name="d_cutoff">The delta cutoff for the filter (default: 1.0)</param>
		public OneEuroFilterVector4(Vector4 startingValue, float startingDerivative = 1.0f, float min_cutoff = 1.0f, float beta = 0.0f, float d_cutoff = 1.0f)
		{
			x = new OneEuroFilter(startingValue.x, startingDerivative, min_cutoff, beta, d_cutoff);
			y = new OneEuroFilter(startingValue.y, startingDerivative, min_cutoff, beta, d_cutoff);
			z = new OneEuroFilter(startingValue.z, startingDerivative, min_cutoff, beta, d_cutoff);
			w = new OneEuroFilter(startingValue.w, startingDerivative, min_cutoff, beta, d_cutoff);
		}

		/// <summary>
		/// Apply a filter step.
		/// </summary>
		/// <param name="currentValue">The value to filter.</param>
		/// <param name="delta">The delta of the step.</param>
		/// <returns>Returns the filtered value.</returns>
		public Vector4 Filter(Vector4 currentValue, float delta)
		{
			return new Vector4(x.Filter(currentValue.x, delta),
								y.Filter(currentValue.y, delta),
								z.Filter(currentValue.z, delta),
								w.Filter(currentValue.w, delta));
		}
	}

	/// <summary>
	/// A One Euro Filter applied to a Quaternion.
	/// Note: Does not apply spherical interpolation!
	/// </summary>
	public class OneEuroFilterQuaternion
	{
		OneEuroFilter x, y, z, w;

		/// <summary>
		/// Constructs a One Euro Filter for a Quaternion.
		/// </summary>
		/// <param name="startingValue">The starting value for the filter.</param>
		/// <param name="startingDerivative">The starting derivative for the filter (default: 1.0)</param>
		/// <param name="min_cutoff">The minimum cutoff for the filter (default: 1.0)</param>
		/// <param name="beta">The filter's beta value (default: 0.0)</param>
		/// <param name="d_cutoff">The delta cutoff for the filter (default: 1.0)</param>
		public OneEuroFilterQuaternion(Quaternion startingValue, float startingDerivative = 1.0f, float min_cutoff = 1.0f, float beta = 0.0f, float d_cutoff = 1.0f)
		{
			x = new OneEuroFilter(startingValue.x, startingDerivative, min_cutoff, beta, d_cutoff);
			y = new OneEuroFilter(startingValue.y, startingDerivative, min_cutoff, beta, d_cutoff);
			z = new OneEuroFilter(startingValue.z, startingDerivative, min_cutoff, beta, d_cutoff);
			w = new OneEuroFilter(startingValue.w, startingDerivative, min_cutoff, beta, d_cutoff);
		}

		/// <summary>
		/// Apply a filter step.
		/// </summary>
		/// <param name="currentValue">The value to filter.</param>
		/// <param name="delta">The delta of the step.</param>
		/// <returns>Returns the filtered value.</returns>
		public Quaternion Filter(Quaternion currentValue, float delta)
		{
			return new Quaternion(x.Filter(currentValue.x, delta),
								y.Filter(currentValue.y, delta),
								z.Filter(currentValue.z, delta),
								w.Filter(currentValue.w, delta));
		}
	}

	/// <summary>
	/// A One Euro Filter applied to a Quaternion using Euler angles (pitch-yaw-roll).
	/// </summary>
	public class OneEuroFilterEuler
	{
		OneEuroFilter yaw, pitch, roll;

		/// <summary>
		/// Constructs a One Euro Filter for a Quaternion that filters via pitch-yaw-roll.
		/// </summary>
		/// <param name="startingValue">The starting value for the filter.</param>
		/// <param name="startingDerivative">The starting derivative for the filter (default: 1.0)</param>
		/// <param name="min_cutoff">The minimum cutoff for the filter (default: 1.0)</param>
		/// <param name="beta">The filter's beta value (default: 0.0)</param>
		/// <param name="d_cutoff">The delta cutoff for the filter (default: 1.0)</param>
		public OneEuroFilterEuler(Quaternion startingValue, float startingDerivative = 1.0f, float min_cutoff = 1.0f, float beta = 0.0f, float d_cutoff = 1.0f)
		{
			yaw = new OneEuroFilter(startingValue.eulerAngles.y, startingDerivative, min_cutoff, beta, d_cutoff);
			pitch = new OneEuroFilter(startingValue.eulerAngles.x, startingDerivative, min_cutoff, beta, d_cutoff);
			roll = new OneEuroFilter(startingValue.eulerAngles.z, startingDerivative, min_cutoff, beta, d_cutoff);
		}

		/// <summary>
		/// Apply a filter step.
		/// </summary>
		/// <param name="currentValue">The value to filter.</param>
		/// <param name="delta">The delta of the step.</param>
		/// <returns>Returns the filtered value.</returns>
		public Quaternion Filter(Quaternion currentValue, float delta)
		{
			return Quaternion.Euler(pitch.Filter(currentValue.eulerAngles.x, delta),
								yaw.Filter(currentValue.eulerAngles.y, delta),
								roll.Filter(currentValue.eulerAngles.z, delta));
		}
	}

	/* public class OneEuroFilterQuaternion
	 {
		 OneEuroFilter x, y, z;

		 public OneEuroFilterQuaternion(Quaternion startingValue, Vector3 startingDerivative, float min_cutoff = 1.0f, float beta = 0.0f, float d_cutoff = 1.0f)
		 {
			 x = new OneEuroFilter(startingValue.x, startingDerivative.x, min_cutoff, beta, d_cutoff);
			 y = new OneEuroFilter(startingValue.y, startingDerivative.y, min_cutoff, beta, d_cutoff);
			 z = new OneEuroFilter(startingValue.z, startingDerivative.z, min_cutoff, beta, d_cutoff);
		 }

		 public Quaternion Filter(Quaternion currentValue, float alpha)
		 {
			 return new Vector3(x.Filter(currentValue.x, alpha),
								 y.Filter(currentValue.y, alpha),
								 z.Filter(currentValue.z, alpha));
		 }
	 }*/
	/*
	class LowPassFilterB
	{
		float y, a, s;
		bool initialized;

		public void setAlpha(float _alpha)
		{
			if (_alpha <= 0.0f || _alpha > 1.0f)
			{
				Debug.LogError("alpha should be in (0.0., 1.0]");
				return;
			}
			a = _alpha;
		}

		public LowPassFilterB(float _alpha, float _initval = 0.0f)
		{
			y = s = _initval;
			setAlpha(_alpha);
			initialized = false;
		}

		public float Filter(float _value)
		{
			float result;
			if (initialized)
				result = a * _value + (1.0f - a) * s;
			else
			{
				result = _value;
				initialized = true;
			}
			y = _value;
			s = result;
			return result;
		}

		public float filterWithAlpha(float _value, float _alpha)
		{
			setAlpha(_alpha);
			return Filter(_value);
		}

		public bool hasLastRawValue()
		{
			return initialized;
		}

		public float lastRawValue()
		{
			return y;
		}

	};

	// -----------------------------------------------------------------

	class OneEuroFilterB
	{
		float freq;
		float mincutoff;
		float beta;
		float dcutoff;
		LowPassFilterB x;
		LowPassFilterB dx;
		float lasttime;

		// currValue contains the latest value which have been succesfully filtered
		// prevValue contains the previous filtered value
		public float currValue { get; protected set; }
		public float prevValue { get; protected set; }

		float alpha(float _cutoff)
		{
			float te = 1.0f / freq;
			float tau = 1.0f / (2.0f * Mathf.PI * _cutoff);
			return 1.0f / (1.0f + tau / te);
		}

		void setFrequency(float _f)
		{
			if (_f <= 0.0f)
			{
				Debug.LogError("freq should be > 0");
				return;
			}
			freq = _f;
		}

		void setMinCutoff(float _mc)
		{
			if (_mc <= 0.0f)
			{
				Debug.LogError("mincutoff should be > 0");
				return;
			}
			mincutoff = _mc;
		}

		void setBeta(float _b)
		{
			beta = _b;
		}

		void setDerivateCutoff(float _dc)
		{
			if (_dc <= 0.0f)
			{
				Debug.LogError("dcutoff should be > 0");
				return;
			}
			dcutoff = _dc;
		}

		public OneEuroFilterB(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
		{
			setFrequency(_freq);
			setMinCutoff(_mincutoff);
			setBeta(_beta);
			setDerivateCutoff(_dcutoff);
			x = new LowPassFilterB(alpha(mincutoff));
			dx = new LowPassFilterB(alpha(dcutoff));
			lasttime = -1.0f;

			currValue = 0.0f;
			prevValue = currValue;
		}

		public void UpdateParams(float _freq, float _mincutoff, float _beta, float _dcutoff)
		{
			setFrequency(_freq);
			setMinCutoff(_mincutoff);
			setBeta(_beta);
			setDerivateCutoff(_dcutoff);
			x.setAlpha(alpha(mincutoff));
			dx.setAlpha(alpha(dcutoff));
		}

		public float Filter(float value, float timestamp = -1.0f)
		{
			prevValue = currValue;

			// update the sampling frequency based on timestamps
			if (lasttime != -1.0f && timestamp != -1.0f)
				freq = 1.0f / (timestamp - lasttime);
			lasttime = timestamp;
			// estimate the current variation per second 
			float dvalue = x.hasLastRawValue() ? (value - x.lastRawValue()) * freq : 0.0f; // FIXME: 0.0 or value? 
			float edvalue = dx.filterWithAlpha(dvalue, alpha(dcutoff));
			// use it to update the cutoff frequency
			float cutoff = mincutoff + beta * Mathf.Abs(edvalue);
			// filter the given value
			currValue = x.filterWithAlpha(value, alpha(cutoff));

			return currValue;
		}
	};


	// this class instantiates an array of OneEuroFilter objects to filter each component of Vector2, Vector3, Vector4 or Quaternion types
	public class OneEuroFilterB<T> where T : struct
	{
		// containst the type of T
		Type type;
		// the array of filters
		OneEuroFilterB[] oneEuroFilters;

		// filter parameters
		public float freq { get; protected set; }
		public float mincutoff { get; protected set; }
		public float beta { get; protected set; }
		public float dcutoff { get; protected set; }

		// currValue contains the latest value which have been succesfully filtered
		// prevValue contains the previous filtered value
		public T currValue { get; protected set; }
		public T prevValue { get; protected set; }

		// initialization of our filter(s)
		public OneEuroFilterB(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
		{
			type = typeof(T);
			currValue = new T();
			prevValue = new T();

			freq = _freq;
			mincutoff = _mincutoff;
			beta = _beta;
			dcutoff = _dcutoff;

			if (type == typeof(Vector2))
				oneEuroFilters = new OneEuroFilterB[2];

			else if (type == typeof(Vector3))
				oneEuroFilters = new OneEuroFilterB[3];

			else if (type == typeof(Vector4) || type == typeof(Quaternion))
				oneEuroFilters = new OneEuroFilterB[4];
			else
			{
				Debug.LogError(type + " is not a supported type");
				return;
			}

			for (int i = 0; i < oneEuroFilters.Length; i++)
				oneEuroFilters[i] = new OneEuroFilterB(freq, mincutoff, beta, dcutoff);
		}

		// updates the filter parameters
		public void UpdateParams(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
		{
			freq = _freq;
			mincutoff = _mincutoff;
			beta = _beta;
			dcutoff = _dcutoff;

			for (int i = 0; i < oneEuroFilters.Length; i++)
				oneEuroFilters[i].UpdateParams(freq, mincutoff, beta, dcutoff);
		}


		// filters the provided _value and returns the result.
		// Note: a timestamp can also be provided - will override filter frequency.
		public T Filter<U>(U _value, float timestamp = -1.0f) where U : struct
		{
			prevValue = currValue;

			if (typeof(U) != type)
			{
				Debug.LogError("WARNING! " + typeof(U) + " when " + type + " is expected!\nReturning previous filtered value");
				currValue = prevValue;

				return (T)Convert.ChangeType(currValue, typeof(T));
			}

			if (type == typeof(Vector2))
			{
				Vector2 output = Vector2.zero;
				Vector2 input = (Vector2)Convert.ChangeType(_value, typeof(Vector2));

				for (int i = 0; i < oneEuroFilters.Length; i++)
					output[i] = oneEuroFilters[i].Filter(input[i], timestamp);

				currValue = (T)Convert.ChangeType(output, typeof(T));
			}

			else if (type == typeof(Vector3))
			{
				Vector3 output = Vector3.zero;
				Vector3 input = (Vector3)Convert.ChangeType(_value, typeof(Vector3));

				for (int i = 0; i < oneEuroFilters.Length; i++)
					output[i] = oneEuroFilters[i].Filter(input[i], timestamp);

				currValue = (T)Convert.ChangeType(output, typeof(T));
			}

			else if (type == typeof(Vector4))
			{
				Vector4 output = Vector4.zero;
				Vector4 input = (Vector4)Convert.ChangeType(_value, typeof(Vector4));

				for (int i = 0; i < oneEuroFilters.Length; i++)
					output[i] = oneEuroFilters[i].Filter(input[i], timestamp);

				currValue = (T)Convert.ChangeType(output, typeof(T));
			}

			else
			{
				Quaternion output = Quaternion.identity;
				Quaternion input = (Quaternion)Convert.ChangeType(_value, typeof(Quaternion));

				// Workaround that take into account that some input device sends
				// quaternion that represent only a half of all possible values.
				// this piece of code does not affect normal behaviour (when the
				// input use the full range of possible values).
				if (Vector4.SqrMagnitude(new Vector4(oneEuroFilters[0].currValue, oneEuroFilters[1].currValue, oneEuroFilters[2].currValue, oneEuroFilters[3].currValue).normalized
					- new Vector4(input[0], input[1], input[2], input[3]).normalized) > 2)
				{
					input = new Quaternion(-input.x, -input.y, -input.z, -input.w);
				}

				for (int i = 0; i < oneEuroFilters.Length; i++)
					output[i] = oneEuroFilters[i].Filter(input[i], timestamp);

				currValue = (T)Convert.ChangeType(output, typeof(T));
			}

			return currValue;// (T)Convert.ChangeType(currValue, typeof(T));
		}
	}*/
}
