using System.Numerics;
using Raylib_cs;
namespace Game {
	class Ledge {
		public Ledge(Vector2 a, Vector2 b, float k, float thickness) {
			this.a	= a;
			this.b	= b;
			this.k	= k;
			this.thickness = thickness;
		}
		public Vector2 Distance(Vector2 point) {
			Vector2 line = this.b - this.a;
			float t = Vector2.Dot(point - this.a, line) / line.LengthSquared();
			t = Math.Clamp(t, 0f, 1f);
			Vector2 dist = point - this.a - line * t;
			return dist - Vector2.Normalize(dist) * thickness;
		}
		public void Draw(Color color) {
			if(thickness <= 0)
				return;
			Raylib.DrawCircleV(a, thickness, color);
			Raylib.DrawCircleV(b, thickness, color);
			Vector2 normal = b - a;
			float t = normal.X;
			normal.X = normal.Y;
			normal.Y = -t;
			normal = Vector2.Normalize(normal) * thickness;
			Raylib.DrawLineV(a + normal, b + normal, color);
			Raylib.DrawLineV(a - normal, b - normal, color);
		}
		public Vector2 a;
		public Vector2 b;
		public float k;
		public float thickness;
	}
}
