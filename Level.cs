using System.Numerics;
namespace Game {
	class Level {
		public Level() {
		}
		public List<Ledge> ledges { get; set; }
		public Ball ball { get; set; }
		public Vector2 gravity { get; set; }
		public void Update(float dt) {
			ball.velocity += gravity * dt;
			ball.Update(dt);
			foreach(var ledge in ledges)
				ball.CollideWith(ledge);
		}
	}
}
