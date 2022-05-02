using System.Numerics;
using Raylib_cs;

namespace Game {
	class GamePlayState: GameState {
		public override GameState Update(Level level, ref Camera2D camera, bool hasInput) {
			if(hasInput) {
				if(Raylib.IsMouseButtonPressed(0))
					return new GameDraggingState(camera);
				if(Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
					return new GameDrawingState(camera);
			}
			level.Update(Raylib.GetFrameTime());
			camera.zoom = Math.Clamp(camera.zoom + (float)Raylib.GetMouseWheelMove() * 0.5f, 1, 100);
			camera.target = Vector2.Lerp(camera.target, level.ball.position, 10f * Raylib.GetFrameTime());
			return this;
		}
		public override void Draw(Level level) {
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
		}
	}
}
