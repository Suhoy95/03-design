using System;
using System.Collections.Generic;
using System.Linq;

namespace battleships
{
	public enum MapCell{   
                        Empty,
                        Ship,
                        DeadOrWoundedShip,
	                    Miss
	}

	public enum ShootEffect{
		                 Miss,
                         Wound,
                         Kill
	}

	public class Ship
	{
        public int Size { get; private set; }
        public bool Direction { get; private set; }
        public Vector Location { get; private set; }
        public HashSet<Vector> AliveCells;
        public bool Alive { get { return AliveCells.Any(); } }
        
		public Ship(Vector location, int size, bool direction)
		{
			Location = location;
			Size = size;
			Direction = direction;
			AliveCells = new HashSet<Vector>(GetShipCells());
		}

        public List<Vector> GetShipCells()
		{
			var d = Direction ? new Vector(1, 0) : new Vector(0, 1);
			var list1 = new List<Vector>();
			for (int i = 0; i < Size; i++)
			{
				var shipCell = d.Mult(i).Add(Location);
				list1.Add(shipCell);
			}
			return list1;
		}	        
	}

	public class Map
	{
		private static MapCell[,] cells;
		public static Ship[,] shipsMap;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Ship> Ships = new List<Ship>();
        public MapCell this[Vector p]
        {
            get
            {
                return CheckBounds(p) ? cells[p.X, p.Y] : MapCell.Empty; // Благодаря этому трюку иногда можно будет не проверять на выход за пределы поля. 
            }
            private set
            {
                if (!CheckBounds(p))
                    throw new IndexOutOfRangeException(p + " is not in the map borders"); // Поможет отлавливать ошибки в коде.
                cells[p.X, p.Y] = value;
            }
        }


		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			cells = new MapCell[width, height];
			shipsMap = new Ship[width, height];
		}

		

		public bool SetShip(Vector location, int size, bool direction)
		{
			var ship = new Ship(location, size, direction);
			var shipCells = ship.GetShipCells();
		
            if (shipCells.SelectMany(Near).Any(c => this[c] != MapCell.Empty)) return false;
			
            if (!shipCells.All(CheckBounds)) return false;

			foreach (var cell in shipCells)
			{
				this[cell] = MapCell.Ship;
				shipsMap[cell.X, cell.Y] = ship;
			}

			Ships.Add(ship);
			return true;
		}

	    public ShootEffect Shoot(Vector target)
		{
			if(CheckBounds(target) && this[target] == MapCell.Ship)
			{
				var ship = shipsMap[target.X, target.Y];
				ship.AliveCells.Remove(target);
				this[target] = MapCell.DeadOrWoundedShip;
				return ship.Alive ? ShootEffect.Wound : ShootEffect.Kill;
			}

	        if (this[target] == MapCell.Empty)
	        {
	            this[target] = MapCell.Miss;
	        }

            return ShootEffect.Miss;
		}

		public IEnumerable<Vector> Near(Vector cell)
		{
			return
				from x in new[] {-1, 0, 1} 
				from y in new[] {-1, 0, 1} 
				let c = cell.Add(new Vector(x, y))
				where CheckBounds(c)
				select c;
		}

		public bool CheckBounds(Vector p)
		{
			return p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
		}
		
		public bool HasAliveShips()
		{
		    return Ships.Any(s => s.Alive);
		}
	}
}