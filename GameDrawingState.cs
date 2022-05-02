using System.Numerics;
using Raylib_cs;
namespace Game {
	class GameDrawingState: GameState {
		public GameDrawingState(Camera2D camera) {
			Vector2 a = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
			createdLedge = new Ledge(a, a, 1, 0.5f);
		}
		public override GameState Update(Level level, ref Camera2D camera, bool hasInput) {
			Vector2 mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
			createdLedge.b = mouse;
			createdLedge.k += Raylib.GetMouseWheelMove() * 0.02f;
			createdLedge.k = Math.Clamp(createdLedge.k, 0f, 1f);
			if(Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT)) {
				level.ledges.Add(createdLedge);
				return new GamePlayState();
			}
			return this;
		}
		public override void Draw(Level level) {
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
			var cledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), createdLedge.k));
			createdLedge.Draw(cledgeColor);
		}
		private Ledge createdLedge;
	}
}
