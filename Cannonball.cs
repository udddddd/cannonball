using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

namespace Game
{
	static class Program
	{
		public static void Main() {
			Raylib.InitWindow(800, 480, "Hello World");
			rlImGui.Setup(true);
			Level level = new Level();

			level.ball = new Ball(new Vector2(5,5), 1f);
			level.gravity = new Vector2(0, 10);

			/* Room initialization */
			const float roomWidth = 50;
			const float roomHeight = 50;
			level.ledges = new List<Ledge>();
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(roomWidth, 0), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(roomWidth, 0), new Vector2(roomWidth, roomHeight), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(0, roomHeight), new Vector2(roomWidth, roomHeight), 0.5f, 0.5f));
			level.ledges.Add(new Ledge(new Vector2(0, 0), new Vector2(0, roomHeight), 0.5f, 0.5f));

			Camera2D camera = new Camera2D();
			camera.offset = new (Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2);
			camera.zoom = 20;

			GameState state = new GamePlayState();
			/* Main loop */
			while (!Raylib.WindowShouldClose())
			{
				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.WHITE);
				Raylib.BeginMode2D(camera);
				state.Draw(level);
				Raylib.EndMode2D();
				rlImGui.Begin();
				ImGui.SetNextWindowSize(new Vector2(300, 80));
				ImGui.SetNextWindowPos(new Vector2(800 - 300, 0));
				ImGui.Begin("Options");
				float radius = level.ball.radius;
				ImGui.SliderFloat("Radius", ref radius, 1, 10);
				float gravity = level.gravity.Y;
				ImGui.SliderFloat("Gravity", ref gravity, 0, 100);
				level.gravity = new Vector2(0, gravity);
				rlImGui.End();
				Raylib.EndDrawing();
				state = state.Update(level, ref camera, !ImGui.GetIO().WantCaptureMouse);
				level.ball.radius = radius;
			}
			rlImGui.Shutdown();
			Raylib.CloseWindow();
		}
	}
}
