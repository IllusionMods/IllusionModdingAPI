using UnityEngine;

namespace TransformGizmoAPI.Objects
{
	public struct IntersectPoints
	{
		public Vector3 first;
		public Vector3 second;

		public IntersectPoints(Vector3 first, Vector3 second)
		{
			this.first = first;
			this.second = second;
		}
	}
}