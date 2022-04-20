using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Raylib_cs;

namespace HelloWorld
{
	abstract class GameState {
		public abstract GameState Update(Level level);
		public abstract void Draw(Level level);
	}
	class GameDraggingState: GameState {
		public override GameState Update(Level level) {
			if(Raylib.IsMouseButtonReleased(0)) {
				level.ball.velocity = (level.ball.position - Raylib.GetMousePosition()) * 10;
				return new GamePlayState();
			}
			return this;
		}
		public override void Draw(Level level) {
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.BLACK);
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			//Raylib.DrawLineEx(level.ball.position, Raylib.GetMousePosition(), 5, Color.BLUE);
			Vector2 normal = level.ball.position - Raylib.GetMousePosition();
			float t = normal.X;
			normal.X = normal.Y;
			normal.Y = -t;
			normal = Vector2.Normalize(normal) * level.ball.radius;
			Raylib.DrawTriangle(level.ball.position - normal, level.ball.position + normal, Raylib.GetMousePosition(), Color.BLUE);
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
			Raylib.DrawCircleV(Raylib.GetMousePosition(), 2, Color.WHITE);
			Raylib.EndDrawing();
		}
	}
	class GamePlayState: GameState {
		public override GameState Update(Level level) {
			Raylib.SetMousePosition(
				Math.Clamp(Raylib.GetMouseX(), 0, Raylib.GetScreenWidth()),
				Math.Clamp(Raylib.GetMouseY(), 0, Raylib.GetScreenHeight())
			);
			level.Update(Raylib.GetFrameTime());
			if(Raylib.IsMouseButtonPressed(0))
				return new GameDraggingState();
			if(Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
				return new GameDrawingState();
			return this;
		}
		public override void Draw(Level level) {
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.BLACK);
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
			Raylib.DrawCircleV(Raylib.GetMousePosition(), 2, Color.WHITE);
			Raylib.EndDrawing();
		}
	}

	class GameDrawingState: GameState {
		public GameDrawingState() {
			Vector2 mouse = new Vector2(Raylib.GetMouseX(), Raylib.GetMouseY());
			createdLedge = new Ledge(mouse, mouse, 1, 10);
		}
		public override GameState Update(Level level) {
			Raylib.SetMousePosition(
				Math.Clamp(Raylib.GetMouseX(), 0, Raylib.GetScreenWidth()),
				Math.Clamp(Raylib.GetMouseY(), 0, Raylib.GetScreenHeight())
			);
			Vector2 mouse = new Vector2(Raylib.GetMouseX(), Raylib.GetMouseY());
			createdLedge.b = mouse;
			createdLedge.k += Raylib.GetMouseWheelMove() / 100;
			createdLedge.k = Math.Clamp(createdLedge.k, 0f, 1f);
			if(Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT)) {
				level.ledges.Add(createdLedge);
				return new GamePlayState();
			}
			return this;
		}
		public override void Draw(Level level) {
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.BLACK);
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
			Raylib.DrawCircleV(Raylib.GetMousePosition(), 2, Color.WHITE);
			var cledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), createdLedge.k));
			createdLedge.Draw(cledgeColor);
			Raylib.EndDrawing();
		}
		private Ledge createdLedge;
	}

	class Ball {
		public Ball(Vector2 position, float radius, float mass, Vector2 velocity = default(Vector2)) {
			this.position = position;
			this.radius   = radius;
			this.mass	  = mass;
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
		public float mass		{ get; set; }
	}
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

	class Level {
		public Level() {
		}
		public List<Ledge> ledges { get; set; }
		public Ball ball { get; set; }
		public Vector2 gravity { get; set; }
		public void Update(float dt) {
			ball.velocity += ball.mass * gravity * dt;
			ball.Update(dt);
			foreach(var ledge in ledges)
				ball.CollideWith(ledge);
		}
	}

	static class Program
	{
		public static void Main() {
			Raylib.InitWindow(800, 480, "Hello World");
			// Will draw cursor manually
			Raylib.DisableCursor();

			Level level = new Level();

			level.ball = new Ball(new Vector2(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2), 20, 15);
			level.gravity = new Vector2(0, 10 * 10);

			/* Room initialization */
			level.ledges = new List<Ledge>();
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(Raylib.GetScreenWidth(), 0), 0.5f, 0f));
			level.ledges.Add(new Ledge(new Vector2(Raylib.GetScreenWidth(), 0), new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), 0.5f, 0f));
			level.ledges.Add(new Ledge(new Vector2(0, Raylib.GetScreenHeight()), new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), 0.5f, 0f));
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(0, Raylib.GetScreenHeight()), 0.5f, 0f));

			GameState state = new GamePlayState();
			/* Main loop */
			while (!Raylib.WindowShouldClose())
			{
				state = state.Update(level);
				state.Draw(level);
			}

			Raylib.CloseWindow();
		}
	}
}
