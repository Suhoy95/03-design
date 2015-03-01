using System;
using System.Linq;
using NUnit.Framework;

namespace battleships
{
    public interface IMapGenerator
    {
        Map GenerateMap();
    }

	public class MapGenerator : IMapGenerator
	{
		private readonly Settings settings;
		private readonly Random random;

		public MapGenerator(Settings settings, Random random)
		{
			this.settings = settings;
			this.random = random;
		}

		public Map GenerateMap()
		{
			var map = new Map(settings.Width, settings.Height);
			foreach (var size in settings.Ships.OrderByDescending(s => s))
				PlaceShip(map, size);
			return map;
		}

		private void PlaceShip(Map map, int size)
		{
			var cells = Vector.Rect(0, 0, settings.Width, settings.Height).OrderBy(v => random.Next());
			foreach (var loc in cells)
			{
				var horizontal = random.Next(2) == 0;
				if (map.SetShip(loc, size, horizontal) || map.SetShip(loc, size, !horizontal)) return;
			}
			throw new Exception("Can't put next ship on map. No free space");
		}
	}

	[TestFixture]
	public class MapGenerator_should
	{
		[Test]
		public void always_succeed_on_standard_map()
		{
			var settings = new Settings { Width = 10, Height = 10, Ships = new[] { 1, 1, 1, 1, 2, 2, 2, 3, 3, 4 } };
			var gen = new MapGenerator(settings, new Random());
			for (var i = 0; i < 10000; i++)
				gen.GenerateMap();
		}
	}
}