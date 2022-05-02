using System.Numerics;
namespace Game {
	class Ball {
		public Ball(Vector2 position, float radius, Vector2 velocity = default(Vector2)) {
			this.position = position;
			this.radius   = radius;
			this.velocity = velocity;
		}
		public void Update(float dt) {
			position += velocity * dt;
		}
		public bool CollideWith(Ledge ledge) {
			Vector2 dist = ledge.Distance(position);
			if(dist.LengthSquared() <= Math.Pow(radius, 2)) {
				Vector2 n = Vector2.Normalize(dist);
				// Move ball slightly above the ledge to prevent sticking
				position = position - dist + n * (radius + 1e-8f);
				float k = MathF.Sqrt(ledge.k);
				Vector2 vn = Vector2.Dot(n, velocity) * n;
				Vector2 vt = velocity - vn;
				velocity = vt - vn * k;
				return true;
			}
			return false;
		}
		public Vector2 position { get; set; }
		public Vector2 velocity { get; set; }
		public float radius		{ get; set; }
	}
}
