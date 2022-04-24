using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Raylib_CsLo;

namespace HelloWorld
{
	abstract class GameState {
		public abstract GameState Update(Level level, ref Camera2D camera);
		public abstract void Draw(Level level);
	}
	class GameDraggingState: GameState {
		public GameDraggingState(Camera2D camera) {
			dragPoint = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
		}
		public override GameState Update(Level level, ref Camera2D camera) {
			dragPoint = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
			if(Raylib.IsMouseButtonReleased(0)) {
				level.ball.velocity = (level.ball.position - dragPoint) * 10;
				return new GamePlayState();
			}
			return this;
		}
		public override void Draw(Level level) {
			Raylib.ClearBackground(Raylib.BLACK);
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Vector2 normal = level.ball.position - dragPoint;
			float t = normal.X;
			normal.X = normal.Y;
			normal.Y = -t;
			normal = Vector2.Normalize(normal) * level.ball.radius;
			Raylib.DrawTriangle(level.ball.position - normal, level.ball.position + normal, dragPoint, Raylib.BLUE);
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Raylib.BLUE);
		}
		private Vector2 dragPoint;
	}
	class GamePlayState: GameState {
		public override GameState Update(Level level, ref Camera2D camera) {
			Raylib.SetMousePosition(
				Math.Clamp(Raylib.GetMouseX(), 0, Raylib.GetScreenWidth()),
				Math.Clamp(Raylib.GetMouseY(), 0, Raylib.GetScreenHeight())
			);
			level.Update(Raylib.GetFrameTime());
			if(Raylib.IsMouseButtonPressed(0))
				return new GameDraggingState(camera);
			if(Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
				return new GameDrawingState(camera);
			camera.zoom += (float)Raylib.GetMouseWheelMove() / 100;
			Console.WriteLine(camera.zoom);
			return this;
		}
		public override void Draw(Level level) {
			Raylib.ClearBackground(Raylib.BLACK);
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Raylib.BLUE);
		}
	}

	class GameDrawingState: GameState {
		public GameDrawingState(Camera2D camera) {
			Vector2 a = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
			createdLedge = new Ledge(a, a, 1, 0.5f);
		}
		public override GameState Update(Level level, ref Camera2D camera) {
			Raylib.SetMousePosition(
				Math.Clamp(Raylib.GetMouseX(), 0, Raylib.GetScreenWidth()),
				Math.Clamp(Raylib.GetMouseY(), 0, Raylib.GetScreenHeight())
			);
			Vector2 mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
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
			Raylib.ClearBackground(Raylib.BLACK);
			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				ledge.Draw(ledgeColor);
			}
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Raylib.BLUE);
			var cledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), createdLedge.k));
			createdLedge.Draw(cledgeColor);
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
			RayGui.GuiLoadStyleDefault();
			// Will draw cursor manually
			Raylib.DisableCursor();

			Level level = new Level();

			level.ball = new Ball(new Vector2(5,5), 1, 10);
			level.gravity = new Vector2(0, 10);

			/* Room initialization */
			const float roomWidth = 10;
			const float roomHeight = 10;
			level.ledges = new List<Ledge>();
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(10, 0), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(roomWidth, 0), new Vector2(roomWidth, roomHeight), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(0, roomHeight), new Vector2(roomWidth, roomHeight), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(0, roomHeight), 0.5f, 0.5f));

			bool edit = false;
			string text = new string("");

			Camera2D camera = new Camera2D();
			camera.offset = new (Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2);
			camera.zoom = Raylib.GetScreenHeight() / 20;

			GameState state = new GamePlayState();
			/* Main loop */
			while (!Raylib.WindowShouldClose())
			{
				state = state.Update(level, ref camera);
				camera.target = level.ball.position;
				Raylib.BeginDrawing();
				Raylib.ClearBackground(Raylib.BLACK);
				Raylib.BeginMode2D(camera);
				state.Draw(level);
				Raylib.EndMode2D();
				if(RayGui.GuiTextBox(new Rectangle(25, 25, 100, 50), text, 128, edit))
					edit = !edit;
				level.ball.radius = RayGui.GuiSliderBar(new Rectangle(640, 40, 105, 20), "Width", level.ball.radius.ToString(), level.ball.radius, 0, (float)Raylib.GetScreenWidth() - 300);
				Raylib.DrawCircleV(Raylib.GetMousePosition(), 2, Raylib.WHITE);
				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
		}
	}
}
