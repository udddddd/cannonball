using System.Numerics;
using Raylib_cs;
namespace Game {
	class GameDraggingState: GameState {
		public GameDraggingState(Camera2D camera) {
			dragPoint = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
		}
		public override GameState Update(Level level, ref Camera2D camera, bool hasInput) {
			dragPoint = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
			if(Raylib.IsMouseButtonReleased(0)) {
				level.ball.velocity = (level.ball.position - dragPoint) * 2;
				return new GamePlayState();
			}
			return this;
		}
		public override void Draw(Level level) {
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Vector2 normal = level.ball.position - dragPoint;
			float t = normal.X;
			normal.X = normal.Y;
			normal.Y = -t;
			normal = Vector2.Normalize(normal) * level.ball.radius;
			Raylib.DrawTriangle(level.ball.position - normal, level.ball.position + normal, dragPoint, Color.BLUE);
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
		}
		private Vector2 dragPoint;
	}
}
