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
#if false
	class GamePlayState: GameState {
		public GamePlayState() {
		}
		public override GameState Update(Level level) {
			Raylib.SetMousePosition(
				Math.Clamp(Raylib.GetMouseX(), 0, Raylib.GetScreenWidth()),
				Math.Clamp(Raylib.GetMouseY(), 0, Raylib.GetScreenHeight())
			);
			//if(Raylib.IsMouseButtonPressed(0))
			//return ;
			//return new GameDraggingState();
			
		}
		public override void Draw(Level level) {
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.WHITE);

			DrawHelp();

			foreach(var ledge in level.ledges) {
				var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
				Raylib.DrawLineEx(ledge.a, ledge.b, 5, ledgeColor);
			}
			Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
			if(dragging)
				Raylib.DrawLineEx(level.ball.position, Raylib.GetMousePosition(), 5, Color.YELLOW);
			Raylib.DrawCircleV(Raylib.GetMousePosition(), 2, Color.BLACK);
			Raylib.EndDrawing();
		}
	}
#endif

	class Ball {
		public Ball(Vector2 position, float radius, float mass, Vector2 velocity = default(Vector2)) {
			this.position = position;
			this.radius   = radius;
			this.mass     = mass;
			this.velocity = velocity;
		}
		public void Update(float dt) {
			position += velocity * dt;
		}
		public bool CollideWith(Ledge ledge) {
			Vector2 dist = ledge.Distance(position);
			if(dist.LengthSquared() <= radius * radius) {
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
		public float radius     { get; set; }
		public float mass       { get; set; }
	}
	class Ledge {
		public Ledge(Vector2 a, Vector2 b, float k, float mu) {
			this.a  = a;
			this.b  = b;
			this.k  = k;
		}
		public Vector2 Distance(Vector2 point) {
			Vector2 line = this.b - this.a;
			float t = Vector2.Dot(point - this.a, line) / line.LengthSquared();
			t = Math.Clamp(t, 0f, 1f);
			return point - this.a - line * t;
		}
		public Vector2 a;
		public Vector2 b;
		public float k;
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
		public static void DrawHelp() {
			Raylib.DrawText("LMB:   Hold to choose impulse", 10, 10, 30, Color.LIGHTGRAY);
			Raylib.DrawText("Wheel: Change ledge's restitution while drawing", 10, 40, 30, Color.LIGHTGRAY);
			Raylib.DrawText("RMB:   Draw ledge", 10, 70, 30, Color.LIGHTGRAY);
		}
        public static void Main()
        {
            Raylib.InitWindow(800, 480, "Hello World");
			// Will draw cursor manually
			Raylib.DisableCursor();

			Level level = new Level();

			level.ball = new Ball(new Vector2(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2), 20, 15);
			level.gravity = new Vector2(0, 10 * 10);

			/* Room initialization */
			level.ledges = new List<Ledge>();
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(Raylib.GetScreenWidth(), 0), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(Raylib.GetScreenWidth(), 0), new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(0, Raylib.GetScreenHeight()), new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(0, Raylib.GetScreenHeight()), 0.5f, 0.5f));

			/* Game states */
			bool dragging = false;
			bool drawing = false;

			/* Main loop */
            while (!Raylib.WindowShouldClose())
            {
				if(Raylib.IsMouseButtonPressed(0))
					dragging = true;
				if(Raylib.IsMouseButtonReleased(0) && dragging) {
					dragging = false;
					level.ball.velocity = (level.ball.position - Raylib.GetMousePosition()) * 10;
				}
				if(!dragging) {
					Raylib.SetMousePosition(
						Math.Clamp(Raylib.GetMouseX(), 0, Raylib.GetScreenWidth()),
						Math.Clamp(Raylib.GetMouseY(), 0, Raylib.GetScreenHeight())
					);
				}
				if(Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) {
					level.ledges.Add(new Ledge(Raylib.GetMousePosition(), Raylib.GetMousePosition(), 1f, 0.5f));
					drawing = true;
				}
				if(drawing) {
					level.ledges.Last().b = Raylib.GetMousePosition();
					level.ledges.Last().k += Raylib.GetMouseWheelMove() / 100;
					level.ledges.Last().k = Math.Clamp(level.ledges.Last().k, 0f, 1f);
				}
				if(Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT))
					drawing = false;
				if(!dragging && !drawing)
					level.Update(Raylib.GetFrameTime());
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);

				DrawHelp();

				foreach(var ledge in level.ledges) {
					var ledgeColor = Raylib.ColorFromNormalized(Vector4.Lerp(new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), ledge.k));
					Raylib.DrawLineEx(ledge.a, ledge.b, 5, ledgeColor);
				}
				Raylib.DrawCircleV(level.ball.position, level.ball.radius, Color.BLUE);
				if(dragging)
					Raylib.DrawLineEx(level.ball.position, Raylib.GetMousePosition(), 5, Color.YELLOW);
				Raylib.DrawCircleV(Raylib.GetMousePosition(), 2, Color.BLACK);
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
