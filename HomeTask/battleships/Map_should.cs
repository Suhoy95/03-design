using NUnit.Framework;

namespace battleships
{
	[TestFixture]
	public class Map_should
	{
		[Test]
		public void put_ship_inside_map_bounds()
		{
			var map = new Map(100, 10);
			Assert.IsTrue(map.SetShip(new Vector(0, 0), 5, true));
			Assert.IsTrue(map.SetShip(new Vector(95, 9), 5, true));
		}

		[Test]
		public void not_put_ship_outside_map()
		{
			var map = new Map(100, 10);
			Assert.IsFalse(map.SetShip(new Vector(99, 9), 2, true));
			Assert.IsFalse(map.SetShip(new Vector(99, 9), 2, false));
		}

		[Test]
		public void kill_ship()
		{
			var map = new Map(100, 10);
			map.SetShip(new Vector(0, 0), 1, true);
			Assert.AreEqual(ShootEffect.Kill, map.Shoot(new Vector(0, 0)));
			Assert.AreEqual(MapCell.DeadOrWoundedShip, map[new Vector(0, 0)]);
		}

		[Test]
		public void wound_ship()
		{
			var map = new Map(100, 10);
			map.SetShip(new Vector(0, 0), 2, true);
			Assert.AreEqual(ShootEffect.Wound, map.Shoot(new Vector(0, 0)));
			Assert.AreEqual(MapCell.DeadOrWoundedShip, map[new Vector(0, 0)]);
			Assert.AreEqual(MapCell.Ship, map[new Vector(1, 0)]);
		}
	}
}