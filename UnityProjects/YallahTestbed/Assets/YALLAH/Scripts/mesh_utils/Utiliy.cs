using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Utiliy class for debugging methods.
 */
public class Utiliy {

	public static void DrawPlane(Vector3 position, Vector3 normal) {
		Vector3 v3;
		
		float stretchFactor = 0.5f;
		if (normal.normalized != Vector3.forward && normal.normalized != -Vector3.forward) {
			v3 = Vector3.Cross(normal, Vector3.forward).normalized * stretchFactor;
		} else {
			v3 = Vector3.Cross(normal, Vector3.up).normalized * stretchFactor;
		}
			
		Vector3 corner0 = position + v3;
		Vector3 corner2 = position - v3;

		Quaternion q = Quaternion.AngleAxis(90.0f, normal);
		v3 = q * v3;
		Vector3 corner1 = position + v3;
		Vector3 corner3 = position - v3;
		
		float duration = 5.0f;
		Debug.DrawLine(corner0, corner2, Color.green, duration);
		Debug.DrawLine(corner1, corner3, Color.green, duration);
		Debug.DrawLine(corner0, corner1, Color.green, duration);
		Debug.DrawLine(corner1, corner2, Color.green, duration);
		Debug.DrawLine(corner2, corner3, Color.green, duration);
		Debug.DrawLine(corner3, corner0, Color.green, duration);
		//Debug.DrawRay(position, normal, Color.red, duration);
	}

	/*
	 * Returns a nullable hit point of a given plane, spawing point and direction.
	 * 
	 * The hit onto the plane of the direction vector spawned at the spawning point
	 * is tried to be computed. Vector3 is a not nullable type, therefore it is
	 * wrapped with Nullable and the caller can check, whether a hit exists.
	 */
	public static Nullable<Vector3> GetHitPoint(Plane plane, Vector3 spawningPoint, Vector3 dir) {
		dir = dir.normalized;
		Ray ray = new Ray(spawningPoint, dir);
		float dist;
		Nullable<Vector3> hit = null;
		if (plane.Raycast(ray, out dist)) {
			hit = spawningPoint + dist * dir;
		}

		return hit;
	}

	/**
	 * Debug method.
	 */
	public static void drawPitchTriangle(Vector3 targetPoint, Vector3 basePoint) {
		float duration = 5.0f;
		Vector3 targetTmp = new Vector3(basePoint.x, targetPoint.y, targetPoint.z);
		Debug.DrawLine(targetTmp, basePoint, Color.red, duration);
		Debug.DrawLine(basePoint, new Vector3(basePoint.x, basePoint.y, targetPoint.z), Color.red, duration);
		Debug.DrawLine(new Vector3(basePoint.x, basePoint.y, targetPoint.z), targetTmp, Color.red, duration);
	}

	/**
	 * Debug method.
	 */
	public static void drawTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		Debug.DrawLine(v1, v2, Color.red);
		Debug.DrawLine(v2, v3, Color.red);
		Debug.DrawLine(v3, v1, Color.red);
	}

	/*
	 * Returns a string containing each component of a Vector3.
	 *
	 * The implemented ToString method of Vector3 rounds the
	 * entries and is therefore sometimes not useful.
	 */
	public static string GetCompleteVector3(Vector3 v) {
		return v.x + " " + v.y + " " + v.z;
	}


    public static Color darker(Color c) {
        return new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, c.a);
    }
}
