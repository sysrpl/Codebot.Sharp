using System;
using System.IO;
using ManagedBass;
using static ManagedBass.Bass;

namespace Codebot.Bass.Test
{
	class MainClass
	{
		static string contentPath = "Content";

		static string GetContentPath(string fileName)
		{
			return Path.Combine(contentPath, fileName);
		}

		static int Time(TimeSpan t)
		{
			return (int)t.TotalSeconds * 1000 + t.Milliseconds;
		}

		static void Stop()
		{
			ManagedBass.Bass.Stop();
		}

		static void Play(string fileName)
		{
			using (var player = new MediaPlayer())
			{
				var task = player.LoadAsync(GetContentPath(fileName));
				if (task.Result)
				{
					Console.WriteLine("Playing stream duration of {0}ms", Time(player.Duration));
					player.Play();
					while (player.State != PlaybackState.Stopped)
					{
						System.Threading.Thread.Sleep(500);
						Console.WriteLine("Position at {0}ms", Time(player.Position));
					}
					Console.WriteLine("Completed");
				}
				else
					Console.WriteLine("Could not open stream");
			}
		}

		public static void Main(string[] args)
		{
			Play("flash-gordon.ogg");
			ManagedBass.Bass.Free();
			Console.WriteLine("done");
		}
	}
}
