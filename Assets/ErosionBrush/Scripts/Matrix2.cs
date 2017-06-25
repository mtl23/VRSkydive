using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ErosionBrushPlugin 
{
	[System.Serializable]
	public struct Coord
	{
		public int x;
		public int z;

		public Coord (int x, int z) { this.x=x; this.z=z; }

		public static bool operator > (Coord c1, Coord c2) { return c1.x>c2.x && c1.z>c2.z; }
		public static bool operator < (Coord c1, Coord c2) { return c1.x<c2.x && c1.z<c2.z; }
		public static bool operator == (Coord c1, Coord c2) { return c1.x==c2.x && c1.z==c2.z; }
		public static bool operator != (Coord c1, Coord c2) { return c1.x!=c2.x || c1.z!=c2.z; }
		public static Coord operator + (Coord c, int s) { return  new Coord(c.x+s, c.z+s); }
		public static Coord operator + (Coord c1, Coord c2) { return  new Coord(c1.x+c2.x, c1.z+c2.z); }
		public static Coord operator - (Coord c, int s) { return  new Coord(c.x-s, c.z-s); }
		public static Coord operator - (Coord c1, Coord c2) { return  new Coord(c1.x-c2.x, c1.z-c2.z); }
		public static Coord operator * (Coord c, int s) { return  new Coord(c.x*s, c.z*s); }
		public static Vector2 operator * (Coord c, Vector2 s) { return  new Vector2(c.x*s.x, c.z*s.y); }
		public static Vector3 operator * (Coord c, Vector3 s) { return  new Vector3(c.x*s.x, s.y, c.z*s.z); }
		public static Coord operator * (Coord c, float s) { return  new Coord((int)(c.x*s), (int)(c.z*s)); }
		public static Coord operator / (Coord c, int s) { return  new Coord(c.x/s, c.z/s); }
		public static Coord operator / (Coord c, float s) { return  new Coord((int)(c.x/s), (int)(c.z/s)); }

		public override bool Equals(object obj) { return base.Equals(obj); }
		public override int GetHashCode() {return x*10000000 + z;}

		public int Minimal {get{ return Mathf.Min(x,z); } }
		public int SqrMagnitude {get{ return x*x + z*z; } }

		public Vector3 vector3 {get{ return new Vector3(x,0,z); } }
		
		public static Coord zero {get{ return new Coord(0,0); }}

		/*public void Divide (float val, bool ceil=false) 
		{ 
			if (ceil) { x = Mathf.FloorToInt(x/val); z = Mathf.FloorToInt(z/val); }
			else { x = Mathf.CeilToInt(x/val); z = Mathf.CeilToInt(z/val); }
		}*/

		public void Round (int val, bool ceil=false) //make a coord divisible by val
		{ 
			x = (ceil ? Mathf.CeilToInt(1f*x/val) : Mathf.FloorToInt(1f*x/val)) * val;
			z = (ceil ? Mathf.CeilToInt(1f*z/val) : Mathf.FloorToInt(1f*z/val)) * val;
		}
		public void Round (Coord c, bool ceil=false)
		{ 
			x = (ceil ? Mathf.FloorToInt(1f*x/c.x) : Mathf.CeilToInt(1f*x/c.x)) * c.x;
			z = (ceil ? Mathf.FloorToInt(1f*z/c.z) : Mathf.CeilToInt(1f*z/c.z)) * c.z;
		}

		public void ClampPositive ()
			{ x = Mathf.Max(0,x); z = Mathf.Max(0,z); }

		public void ClampByRect (CoordRect rect)
		{ 
			if (x<rect.offset.x) x = rect.offset.x; if (x>=rect.offset.x+rect.size.x) x = rect.offset.x+rect.size.x-1;
			if (z<rect.offset.z) z = rect.offset.z; if (z>=rect.offset.z+rect.size.z) z = rect.offset.z+rect.size.z-1;
		}

		static public Coord Min (Coord c1, Coord c2) { return new Coord(Mathf.Min(c1.x,c2.x), Mathf.Min(c1.z,c2.z)); }
		static public Coord Max (Coord c1, Coord c2) { return new Coord(Mathf.Max(c1.x,c2.x), Mathf.Max(c1.z,c2.z)); }
		static public float Distance (Coord c1, Coord c2) { return Mathf.Sqrt((c1.x-c2.x)*(c1.x-c2.x) + (c1.z-c2.z)*(c1.z-c2.z)); }
		static public float Distance (Coord c1, int x, int z) { return Mathf.Sqrt((c1.x-x)*(c1.x-x) + (c1.z-z)*(c1.z-z)); }

		public override string ToString()
		{
			return (base.ToString() + " x:" + x + " z:" + z);
		}


		public IEnumerable<Coord> DistanceStep (int i, int dist) //4+4 terrains, no need to use separetely
		{
			yield return new Coord(x-i, z-dist);
			yield return new Coord(x-dist, z+i);
			yield return new Coord(x+i, z+dist);
			yield return new Coord(x+dist, z-i);

			yield return new Coord(x+i+1, z-dist);
			yield return new Coord(x-dist, z-i-1);
			yield return new Coord(x-i-1, z+dist);
			yield return new Coord(x+dist, z+i+1);
		}

		public IEnumerable<Coord> DistancePerimeter (int dist) //a circular square border sorted by distance
		{
			for (int i=0; i<dist; i++)
				foreach (Coord c in DistanceStep(i,dist)) yield return c;
		}

		public IEnumerable<Coord> DistanceArea (int maxDist)
		{
			yield return this;
			for (int i=0; i<maxDist; i++)
				foreach (Coord c in DistancePerimeter(i)) yield return c;
		}

		public IEnumerable<Coord> DistanceArea (CoordRect rect) //same as distance are, but clamped by rect
		{
			int maxDist = Mathf.Max(x-rect.offset.x, rect.Max.x-x, z-rect.offset.z, rect.Max.z-z) + 1;

			if (rect.CheckInRange(this)) yield return this;
			for (int i=0; i<maxDist; i++)
				foreach (Coord c in DistancePerimeter(i)) 
					if (rect.CheckInRange(c)) yield return c;
		}

		public static IEnumerable<Coord> MultiDistanceArea (Coord[] coords, int maxDist)
		{
			for (int c=0; c<coords.Length; c++) yield return coords[c];
			
			for (int dist=0; dist<maxDist; dist++)
				for (int i=0; i<dist; i++)
					for (int c=0; c<coords.Length; c++)
						foreach (Coord c2 in coords[c].DistanceStep(i,dist)) yield return c2;
		}

		public Vector3 ToVector3 (float cellSize) { return new Vector3(x*cellSize, 0, z*cellSize); }
		public Vector2 ToVector2 (float cellSize) { return new Vector2(x*cellSize, z*cellSize); }
		public Rect ToRect (float cellSize) { return new Rect(x*cellSize, z*cellSize, cellSize, cellSize); }
	}
	
	[System.Serializable]
	public struct CoordRect
	{
		public Coord offset;
		public Coord size;

		public bool isZero { get{ return size.x==0 || size.z==0; }}

		//public int radius; //not related with size, because a clamped CoordRect should have non-changed radius

		public CoordRect (Coord offset, Coord size) { this.offset = offset; this.size = size; }
		public CoordRect (int offsetX, int offsetZ, int sizeX, int sizeZ) { this.offset = new Coord(offsetX,offsetZ); this.size = new Coord(sizeX,sizeZ);  }
		public CoordRect (float offsetX, float offsetZ, float sizeX, float sizeZ) { this.offset = new Coord((int)offsetX,(int)offsetZ); this.size = new Coord((int)sizeX,(int)sizeZ);  }
		public CoordRect (Rect r) { offset = new Coord((int)r.x, (int)r.y); size = new Coord((int)r.width, (int)r.height); }

		public int GetPos(int x, int z) { return (z-offset.z)*size.x + x - offset.x; }

		public Coord Max { get { return offset+size; } set { offset = value-size; } }
		public Coord Min { get { return offset; } set { offset = value; } }
		public Coord Center { get { return offset + size/2; } } 

		public static bool operator > (CoordRect c1, CoordRect c2) { return c1.size>c2.size; }
		public static bool operator < (CoordRect c1, CoordRect c2) { return c1.size<c2.size; }
		public static bool operator == (CoordRect c1, CoordRect c2) { return c1.offset==c2.offset && c1.size==c2.size; }
		public static bool operator != (CoordRect c1, CoordRect c2) { return c1.offset!=c2.offset || c1.size!=c2.size; }
		public static CoordRect operator * (CoordRect c, int s) { return  new CoordRect(c.offset*s, c.size*s); }
		public static CoordRect operator * (CoordRect c, float s) { return  new CoordRect(c.offset*s, c.size*s); }
		public static CoordRect operator / (CoordRect c, int s) { return  new CoordRect(c.offset/s, c.size/s); }

		public override bool Equals(object obj) { return base.Equals(obj); }
		public override int GetHashCode() {return offset.x*100000000 + offset.z*1000000 + size.x*1000+size.z;}

		//public float SqrDistFromCenter (Coord c) { return (Center-c).SqrMagnitude; }
		//public bool IsInRadius (Coord c) { return SqrDistFromCenter(c) < radius*radius; }
		//public void CalcRadius () { radius = size.Minimal/2; }
		public void Round (int val, bool inscribed=false) { offset.Round(val, ceil:inscribed); size.Round(val, ceil:!inscribed); } //inscribed parameter will shrink rect to make it lay inside original rect
		public void Round (CoordRect r, bool inscribed=false) { offset.Round(r.offset, ceil:inscribed); size.Round(r.size, ceil:!inscribed); }
		//public void Divide (int val, bool inscribed=false) { offset.Round(val, ceil:inscribed); size.Round(val, ceil:!inscribed); } //inscribed parameter will shrink rect to make it lay inside original rect

		public void Clamp (Coord min, Coord max)
		{
			Coord oldMax = Max;
			offset = Coord.Max(min, offset);
			size = Coord.Min(max-offset, oldMax-offset);
			size.ClampPositive();
		}

		public static CoordRect Intersect (CoordRect c1, CoordRect c2) { c1.Clamp(c2.Min, c2.Max); return c1; }

		//public static bool Intersects (CoordRect c1, CoordRect c2) { c1.Clamp(c2.Min, c2.Max); return c1.size.x>0 && c1.size.z>0; }

		//public int this[int x, int z] { get { return (z-offset.z)*size.x + x - offset.x; } } //gets the pos of the coordinate
		//public int this[Coord c] { get { return (c.z-offset.z)*size.x + c.x - offset.x; } }

		public Coord CoordByNum (int num) 
		{
			int z = num / size.x;
			int x = num - z*size.x;
			return new Coord(x+offset.x, z+offset.z);
		}

		public bool CheckInRange (int x, int z)
		{
			return (x- offset.x >= 0 && x- offset.x < size.x &&
			        z- offset.z >= 0 && z- offset.z < size.z);
		}

		public bool CheckInRange (Coord coord)
		{
			return (coord.x >= offset.x && coord.x < offset.x + size.x &&
			        coord.z >= offset.z && coord.z < offset.z + size.z);
		}

		public bool CheckInRangeAndBounds (int x, int z)
		{
			return (x > offset.x && x < offset.x + size.x-1 &&
			        z > offset.z && z < offset.z + size.z-1);
		}

		public bool CheckInRangeAndBounds (Coord coord)
		{
			return (coord.x > offset.x && coord.x < offset.x + size.x-1 &&
			        coord.z > offset.z && coord.z < offset.z + size.z-1);
		}

		public bool Divisible (float factor) { return offset.x%factor==0 && offset.z%factor==0 && size.x%factor==0 && size.z%factor==0; }

		public override string ToString()
		{
			return (base.ToString() + ": offsetX:" + offset.x + " offsetZ:" + offset.z + " sizeX:" + size.x + " sizeZ:" + size.z);
		}

		public Vector2 ToWorldspace (Coord coord, Rect worldRect)
		{
			return new Vector2 ( 1f*(coord.x-offset.x)/size.x * worldRect.width + worldRect.x, 
								 1f*(coord.z-offset.z)/size.z * worldRect.height + worldRect.y);  //percentCoord*worldWidth + worldOffset
		}

		public Coord ToLocalspace (Vector2 pos, Rect worldRect)
		{
			return new Coord ( (int) ((pos.x-worldRect.x)/worldRect.width * size.x + offset.x),
							   (int) ((pos.y-worldRect.y)/worldRect.height * size.z + offset.z) ); //percentPos*size + offset
		}

		public IEnumerable<Coord> Cells (int cellSize) //coordinates of the cells inside this rect
		{
			//transforming to cell-space
			Coord min = offset/cellSize;
			Coord max = (Max-1)/cellSize + 1;

			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				yield return new Coord(x,z);
			}
		}

		public CoordRect Expand (int n) { return new CoordRect(offset-n, size+n*2); }

		public CoordRect Approximate (int val)
		{
			CoordRect approx = new CoordRect();

			approx.size.x = (size.x/val + 1) * val;
			approx.size.z = (size.z/val + 1) * val;

			approx.offset.x = offset.x - (approx.size.x-size.x)/2;
			approx.offset.z = offset.z - (approx.size.z-size.z)/2;

			approx.offset.x = (int)(approx.offset.x/val + 0.5f) * val;
			approx.offset.z = (int)(approx.offset.z/val + 0.5f) * val;

			return approx;
		}
	}


		

	public class Matrix2<T> : ICloneable
	{
		public T[] array;
		public CoordRect rect; //never assign it's size manually, use ChangeRect
		public int pos;
		public int count; //rect.size.x*rect.size.z, not a property for faster access

		#region Creation

			public Matrix2 () {}

			public Matrix2 (int x, int z, T[] array=null)
			{
				rect = new CoordRect(0,0,x,z);
				count = x*z;
				if (array != null && array.Length<count) Debug.Log("Array length: " + array.Length + " is lower then matrix capacity: " + count);
				if (array != null && array.Length>=count) this.array = array;
				else this.array = new T[count];
			}
		
			public Matrix2 (CoordRect rect, T[] array=null)
			{
				this.rect = rect;
				count = rect.size.x*rect.size.z;
				if (array != null && array.Length<count) Debug.Log("Array length: " + array.Length + " is lower then matrix capacity: " + count);
				if (array != null && array.Length>=count) this.array = array;
				else this.array = new T[count];
			}

			public Matrix2 (Coord offset, Coord size, T[] array=null)
			{
				rect = new CoordRect(offset, size);
				count = rect.size.x*rect.size.z;
				if (array != null && array.Length<count) Debug.Log("Array length: " + array.Length + " is lower then matrix capacity: " + count);
				if (array != null && array.Length>=count) this.array = array;
				else this.array = new T[count];
			}

		#endregion
		
		public T this[int x, int z] 
		{
			get { return array[(z-rect.offset.z)*rect.size.x + x - rect.offset.x]; } //rect fn duplicated to increase performance
			set { array[(z-rect.offset.z)*rect.size.x + x - rect.offset.x] = value; }
		}

		public T this[Coord c] 
		{
			get { return array[(c.z-rect.offset.z)*rect.size.x + c.x - rect.offset.x]; }
			set { array[(c.z-rect.offset.z)*rect.size.x + c.x - rect.offset.x] = value; }
		}

		public T CheckGet (int x, int z) 
		{ 
			if (x>=rect.offset.x && x<rect.offset.x+rect.size.x && z>=rect.offset.z && z<rect.offset.z+rect.size.z)
				return array[(z-rect.offset.z)*rect.size.x + x - rect.offset.x]; 
			else return default(T);
		} 

		/*public T this[Vector3 pos]
		{
			get { return array[((int)pos.z-rect.offset.z)*rect.size.x + (int)pos.x - rect.offset.x]; }
			set { array[((int)pos.z-rect.offset.z)*rect.size.x + (int)pos.x - rect.offset.x] = value; }
		}*/

		public T this[Vector2 pos]
		{
			get{ 
				int posX = (int)(pos.x + 0.5f); if (pos.x < 0) posX--;
				int posZ = (int)(pos.y + 0.5f); if (pos.y < 0) posZ--;
				return array[(posZ-rect.offset.z)*rect.size.x + posX - rect.offset.x]; 
			}
			set{
				int posX = (int)(pos.x + 0.5f); if (pos.x < 0) posX--;
				int posZ = (int)(pos.y + 0.5f); if (pos.y < 0) posZ--;
				array[(posZ-rect.offset.z)*rect.size.x + posX - rect.offset.x] = value; 
			}
		}

		public void Clear () { for (int i=0; i<array.Length; i++) array[i] = default(T); }

		public void ChangeRect (CoordRect newRect) //will re-create array only if capacity changed
		{
			rect = newRect;
			count = newRect.size.x*newRect.size.z;

			if (array.Length<count) array = new T[count];
		}

		public virtual object Clone () { return Clone(null); } //separate fn for IClonable
		public Matrix2<T> Clone (Matrix2<T> result)
		{
			if (result==null) result = new Matrix2<T>(rect);
			
			//copy params
			result.rect = rect;
			result.pos = pos;
			result.count = count;
			
			//copy array
			//result.array = (float[])array.Clone(); //no need to create it any time
			if (result.array.Length != array.Length) result.array = new T[array.Length];
			for (int i=0; i<array.Length; i++)
				result.array[i] = array[i];

			return result;
		}

		public void Fill (T v) { for (int i=0; i<count; i++) array[i] = v; }

		public void Fill (Matrix2<T> m, bool removeBorders=false)
		{
			CoordRect intersection = CoordRect.Intersect(rect, m.rect);
			Coord min = intersection.Min; Coord max = intersection.Max;
			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
					this[x,z] = m[x,z];
			if (removeBorders) RemoveBorders(intersection);
		}

		#region Quick Pos

			public void SetPos(int x, int z) { pos = (z-rect.offset.z)*rect.size.x + x - rect.offset.x; }
			public void SetPos(int x, int z, int s) { pos = (z-rect.offset.z)*rect.size.x + x - rect.offset.x  +  s*rect.size.x*rect.size.z; }

			public void MoveX() { pos++; }
			public void MoveZ() { pos += rect.size.x; }
			public void MovePrevX() { pos--; }
			public void MovePrevZ() { pos -= rect.size.x; }

			//public float current { get { return array[pos]; } set { array[pos] = value; } }
			public T nextX { get { return array[pos+1]; } set { array[pos+1] = value; } }
			public T prevX { get { return array[pos-1]; } set { array[pos-1] = value; } }
			public T nextZ { get { return array[pos+rect.size.x]; } set { array[pos+rect.size.x] = value; } }
			public T prevZ { get { return array[pos-rect.size.x]; } set { array[pos-rect.size.x] = value; } }
			public T nextXnextZ { get { return array[pos+rect.size.x+1]; } set { array[pos+rect.size.x+1] = value; } }
			public T prevXnextZ { get { return array[pos+rect.size.x-1]; } set { array[pos+rect.size.x-1] = value; } }
			public T nextXprevZ { get { return array[pos-rect.size.x+1]; } set { array[pos-rect.size.x+1] = value; } }
			public T prevXprevZ { get { return array[pos-rect.size.x-1]; } set { array[pos-rect.size.x-1] = value; } }

		#endregion

		#region OrderedFromCenter

			/*public Coord[] GetOrderedFromCenterCoords ()
			{
				Coord[] sortedByDistance = new Coord[array.Length];
				int i=0;
				Coord min = rect.Min; Coord max = rect.Max;
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
						{ sortedByDistance[i] = new Coord(x,z); i++; }

				float[] distances = new float[array.Length];
				for (int z=0; z<rect.size.z; z++)
					for (int x=0; x<rect.size.x; x++)
						distances[z*rect.size.x + x] = (x-rect.size.x/2)*(x-rect.size.x/2) + (z-rect.size.z/2)*(z-rect.size.z/2); //Mathf.Max( Mathf.Abs(x-chunks.rect.size.x/2), Mathf.Abs(z-chunks.rect.size.z/2) );

				Extensions.ArrayQSort(sortedByDistance, distances);
				return sortedByDistance;
			}

			public IEnumerable<Coord> OrderedFromCenterCoord ()
			{
				Coord[] sortedByDistance = GetOrderedFromCenterCoords();
				for (int i=0; i<sortedByDistance.Length; i++)
					yield return sortedByDistance[i];
			}

			public IEnumerable<T> OrderedFromCenter ()
			{
				Coord[] sortedByDistance = GetOrderedFromCenterCoords();
				for (int i=0; i<sortedByDistance.Length; i++)
					yield return this[sortedByDistance[i]];
			}*/

		#endregion

		#region Borders

			public void RemoveBorders ()
			{
				Coord min = rect.Min; Coord last = rect.Max - 1;
			
				for (int x=min.x; x<=last.x; x++)
					{ SetPos(x,min.z); array[pos] = nextZ; }

				for (int x=min.x; x<=last.x; x++)
					{ SetPos(x,last.z); array[pos] = prevZ; }

				for (int z=min.z; z<=last.z; z++)
					{ SetPos(min.x,z); array[pos] = nextX; }

				for (int z=min.z; z<=last.z; z++)
					{ SetPos(last.x,z); array[pos] = prevX; }
			}

			public void RemoveBorders (int borderMinX, int borderMinZ, int borderMaxX, int borderMaxZ)
			{
				Coord min = rect.Min; Coord max = rect.Max;
			
				if (borderMinZ != 0)
				for (int x=min.x; x<max.x; x++)
				{
					T val = this[x, min.z+borderMinZ];
					for (int z=min.z; z<min.z+borderMinZ; z++) this[x,z] = val;
				}

				if (borderMaxZ != 0)
				for (int x=min.x; x<max.x; x++)
				{
					T val = this[x, max.z-borderMaxZ];
					for (int z=max.z-borderMaxZ; z<max.z; z++) this[x,z] = val;
				}

				if (borderMinX != 0)
				for (int z=min.z; z<max.z; z++)
				{
					T val = this[min.x+borderMinX, z];
					for (int x=min.x; x<min.x+borderMinX; x++) this[x,z] = val;
				}
				
				if (borderMaxX != 0)
				for (int z=min.z; z<max.z; z++)
				{
					T val = this[max.x-borderMaxX, z];
					for (int x=max.x-borderMaxX; x<max.x; x++) this[x,z] = val;
				}
			}

			public void RemoveBorders (CoordRect centerRect)
			{ 
				RemoveBorders(
					Mathf.Max(0,centerRect.offset.x-rect.offset.x), 
					Mathf.Max(0,centerRect.offset.z-rect.offset.z), 
					Mathf.Max(0,rect.Max.x-centerRect.Max.x+1), 
					Mathf.Max(0,rect.Max.z-centerRect.Max.z+1) ); 
			}

		#endregion
	}

}


