using Raylib_cs;
namespace Game {
	abstract class GameState {
		public abstract GameState Update(Level level, ref Camera2D camera, bool hasInput);
		public abstract void Draw(Level level);
	}
}
