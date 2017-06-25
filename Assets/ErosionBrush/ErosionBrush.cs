using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Plugins;

namespace ErosionBrushPlugin
{
	[System.Serializable]
	public class Preset
	{
		//splat preset
		[System.Serializable]
		public struct SplatPreset
		{
			public bool apply;
			public float opacity;
			public int num;
		}

		//main brush params
		public float brushSize = 50;
		public float brushFallof = 0.6f;
		public float brushSpacing = 0.15f;
		public int downscale = 1;
		public float blur = 0.1f;
		public bool preserveDetail = false;

		public bool isErosion = true;
		public bool isNoise; // { get{return !isErosion;} set{isErosion=!value;} }

		//noise brush
		public int noise_seed = 12345;
		public float noise_amount = 20f;
		public float noise_size = 200f;
		public float noise_detail = 0.55f;
		public float noise_uplift = 0.8f;
		public float noise_ruffle = 1f;

		//erosion brush
		public int erosion_iterations = 3;
		public float erosion_durability = 0.9f;
		public int erosion_fluidityIterations = 3;
		public float erosion_amount = 1f; //quantity of erosion made by iteration. Lower values require more iterations, but will give better results
		public float sediment_amount = 0.8f; //quantity of sediment that was raised by erosion will drop back to land. Lower values will give eroded canyons with washed-out land, but can produce artefacts
		public float wind_amount = 0.75f;
		public float erosion_smooth = 0.15f;
		public float ruffle = 0.5f;

		//painting
		public SplatPreset foreground = new SplatPreset() { opacity=1 };
		public SplatPreset background = new SplatPreset() { opacity=1 };
		public bool paintSplat
		{get{
			return  (foreground.apply && foreground.opacity>0.01f) ||
					(background.apply && background.opacity>0.01f);
		}}
		
		//save-load
		public string name;
		public bool saveBrushSize;
		public bool saveBrushParams;
		public bool saveErosionNoiseParams;
		public bool saveSplatParams;
		public Preset Copy() { return (Preset) this.MemberwiseClone(); }
	}
	
	[ExecuteInEditMode]
	public class ErosionBrush : MonoBehaviour 
	{
		public Terrain[] terrains;
		private Terrain _terrain;
		public Terrain terrain { get{ if (_terrain==null) _terrain=GetComponent<Terrain>(); return _terrain; } set {_terrain=value;} }

		public Preset preset = new Preset(); 
		public Preset[] presets = new Preset[0];
		public int guiSelectedPreset = 0;

		public bool paint = false;
		public bool wasPaint = false;
		public bool moveDown = false;

		public Transform moveTfm;
		public bool gen;

		public bool undo; 

		[System.NonSerialized] public Texture2D guiHydraulicIcon;
		[System.NonSerialized] public Texture2D guiWindIcon;
		[System.NonSerialized] public Texture2D guiPluginIcon;
		public int guiApplyIterations = 1;
		public int[] guiChannels;
		public string[] guiChannelNames;
		public Color guiBrushColor = new Color(1f,0.7f,0.3f);
		public float guiBrushThickness = 4;
		public int guiBrushNumCorners = 32;
		public bool recordUndo = true;
		public bool unity5positioning = false;
		public bool focusOnBrush = true;

		public bool guiShowPreset = true;
		public bool guiShowBrush = true;
		//public bool guiShowGenerator = true;
		public bool guiShowErosion = true;
		public bool guiShowNoise = true;
		public bool guiShowTextures = true;
		public bool guiShowGlobal = false;
		public bool guiShowSettings = false;
		public bool guiShowAbout = false;
		public int guiMaxBrushSize = 100;
		public bool guiSelectPresetsUsingNumkeys = true;
		public bool guiControlUsed = false;

		public int applyCount = 0;

		[System.NonSerialized] Matrix srcHeight = new Matrix( new CoordRect(0,0,0,0) );
		//[System.NonSerialized] Matrix srcCliff = new Matrix( new CoordRect(0,0,0,0) );
		//[System.NonSerialized] Matrix srcSediment = new Matrix( new CoordRect(0,0,0,0) );

		[System.NonSerialized] Matrix wrkHeight = new Matrix( new CoordRect(0,0,0,0) );
		[System.NonSerialized] Matrix wrkCliff = new Matrix( new CoordRect(0,0,0,0) );
		[System.NonSerialized] Matrix wrkSediment = new Matrix( new CoordRect(0,0,0,0) );

		[System.NonSerialized] Matrix dstHeight = new Matrix( new CoordRect(0,0,0,0) );
		[System.NonSerialized] Matrix dstCliff = new Matrix( new CoordRect(0,0,0,0) );
		[System.NonSerialized] Matrix dstSediment = new Matrix( new CoordRect(0,0,0,0) );


		public Terrain[] GetTerrains ()
		{
			Terrain terrain = transform.GetComponent<Terrain>();
			if (terrain != null) return new Terrain[] {terrain};
			else return transform.GetComponentsInChildren<Terrain>();
		}

		#region Main

			public void ApplyNoise (Matrix height, Matrix bedrock, Matrix sediment)
			{
				Noise noise = new Noise(preset.noise_size, 512, preset.noise_seed*7, preset.noise_seed*3);
				
				Coord min = height.rect.Min; Coord max = height.rect.Max;
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					float result = noise.Fractal(x,z,preset.noise_detail);
									
					//apply contrast and bias
					result -= 1-preset.noise_uplift;
					result = result*preset.noise_amount / terrains[0].terrainData.size.y;

					//if (result < 0) result = 0; 
					//if (result > 1) result = 1;

					height[x,z] += result;

					//writing cliff
					if (bedrock != null)
						bedrock[x,z] = result>0? result*10 * (preset.foreground.apply? preset.foreground.opacity : 1) : 0; //Mathf.Sqrt(splatNoise)*0.3f;

					//writing sediment
					if (sediment != null)
						sediment[x,z] = result<0? -result*10 * (preset.background.apply? preset.background.opacity : 1) : 0; //Mathf.Sqrt(sedimentNoise)*0.3f;
				}
			}

			public void ApplyErosion (Matrix height, Matrix bedrock, Matrix sediment)
			{
				//filling empty spots on matrix to prevent bounds errors
				height.FillEmpty();

				//clearing paint arrays if noise is on
				if (preset.isNoise) { if (bedrock!=null) bedrock.Clear(); if (sediment!=null) sediment.Clear(); }

				Erosion.ErosionIteration(height, bedrock, sediment,
					erosionDurability:preset.erosion_durability, erosionAmount:preset.erosion_amount, sedimentAmount:preset.sediment_amount, erosionFluidityIterations:preset.erosion_fluidityIterations, ruffle:preset.ruffle);

				//multiply splats to make them look as close as the previous version
				if (bedrock != null) bedrock.Multiply(100f * (preset.foreground.apply? preset.foreground.opacity : 1));
				if (sediment != null) sediment.Multiply(4f * (preset.background.apply? preset.background.opacity : 1));

				//removing the bedrock on the tiles with sediment
				if (bedrock != null && sediment != null)
					for (int i=0; i<bedrock.count; i++)
						if (sediment.array[i] > 0.01f) bedrock.array[i] = 0;
			}

		#endregion

		#region Brush
			public void ApplyBrush (Vector3 pos, float radius, bool useFallof=true)
			{
				//preparing height matrix
				float heightPixelSize = terrains[0].terrainData.size.x / terrains[0].terrainData.heightmapResolution;
				CoordRect heightRect = pos.ToCoordRect(radius, heightPixelSize);
				heightRect = heightRect.Approximate(preset.downscale);
				Matrix height = new Matrix(heightRect); //TODO: I've changed ToCoordRect fn in extensions, this should be re-checked

				//filling height matrix
				for (int t=0; t<terrains.Length; t++) GetHeight(height,terrains[t]);
				Matrix sourceHeight = height.Copy();

				//downscaling
				if (preset.downscale > 1) height = height.Downscale(preset.downscale);

				//preparing splat matrices
				Matrix bedrockMatrix = null; Matrix sedimentMatrix = null;
				if ((preset.foreground.apply && terrains[0].terrainData.alphamapLayers>preset.foreground.num) || 
					(preset.background.apply && terrains[0].terrainData.alphamapLayers>preset.background.num))
						{ bedrockMatrix = new Matrix(height.rect); sedimentMatrix = new Matrix(height.rect); }

				//brush
				if (preset.isNoise) ApplyNoise(height, bedrockMatrix, sedimentMatrix);
				if (preset.isErosion) ApplyErosion(height, bedrockMatrix, sedimentMatrix);

				//upscaling
				if (preset.downscale > 1)
				{
					height = height.Upscale(preset.downscale); 
					if (bedrockMatrix != null) bedrockMatrix = bedrockMatrix.Upscale(preset.downscale);
					if (sedimentMatrix != null) sedimentMatrix = sedimentMatrix.Upscale(preset.downscale);
				}

				//making height additive
				if (!preset.preserveDetail || preset.downscale == 1) height.Subtract(sourceHeight);
				else 
				{
					Matrix smoothHeight = sourceHeight.Downscale(preset.downscale);
					smoothHeight = smoothHeight.Upscale(preset.downscale);
					height.Subtract(smoothHeight);

				}

				//applying fallof
				if (useFallof)
				{
					Coord center = height.rect.Center; float rad = height.rect.size.x/2f;
					Coord min = height.rect.Min; Coord max = height.rect.Max;
					for (int x=min.x; x<max.x; x++)
						for (int z=min.z; z<max.z; z++)
					{
						float percent = (rad - Coord.Distance(new Coord(x,z), center)) / (rad-rad*preset.brushFallof);
						if (percent < 0) percent = 0; if (percent > 1) percent = 1;
						percent = 3*percent*percent - 2*percent*percent*percent;

						height[x,z] *= percent;
						if (bedrockMatrix != null) bedrockMatrix[x,z] *= percent;
						if (sedimentMatrix != null) sedimentMatrix[x,z] *= percent;
					}
				}

				//scaling splat matrices to match terrain splat resolution
				float splatPixelSize = terrains[0].terrainData.size.x / terrains[0].terrainData.alphamapResolution;
				CoordRect splatRect = pos.ToCoordRect(radius, splatPixelSize);
				if (bedrockMatrix != null) bedrockMatrix = bedrockMatrix.Resize(splatRect);
				if (sedimentMatrix != null) sedimentMatrix = sedimentMatrix.Resize(splatRect);

				//backing to terrain
				for (int t=0; t<terrains.Length; t++)
				{
					AddHeight(height, terrains[t]);

					if (bedrockMatrix != null && preset.foreground.apply) AddSplat(bedrockMatrix, terrains[t], preset.foreground.num);
					if (sedimentMatrix != null && preset.background.apply) AddSplat(sedimentMatrix, terrains[t], preset.background.num);
				}
			}

			public void GetHeight (Matrix matrix, Terrain terrain)
			{
				CoordRect terrainRect = terrain.GetHeightRect();
				CoordRect intersection = CoordRect.Intersect(terrainRect, matrix.rect);
				if (intersection.size.x<=0 || intersection.size.z<=0) return;

				float[,] heights = terrain.terrainData.GetHeights(intersection.offset.x-terrainRect.offset.x, intersection.offset.z-terrainRect.offset.z, intersection.size.x, intersection.size.z);

				Coord min = intersection.Min; Coord max = intersection.Max;
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					matrix[x,z] = heights[z-min.z, x-min.x]; //x and z switched
				}
			}

			public void AddHeight (Matrix matrix, Terrain terrain)
			{
				CoordRect terrainRect = terrain.GetHeightRect();
				CoordRect intersection = CoordRect.Intersect(terrainRect, matrix.rect);
				if (intersection.size.x<=0 || intersection.size.z<=0) return;

				float[,] heights = terrain.terrainData.GetHeights(intersection.offset.x-terrainRect.offset.x, intersection.offset.z-terrainRect.offset.z, intersection.size.x, intersection.size.z);
				//float[,] heights = new float[intersection.size.z, intersection.size.x]; //x and z switched

				Coord min = intersection.Min; Coord max = intersection.Max;
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					heights[z-min.z, x-min.x] += matrix[x,z]; //x and z switched
				}

				terrain.terrainData.SetHeights(intersection.offset.x-terrainRect.offset.x, intersection.offset.z-terrainRect.offset.z, heights);
			}

			public void AddSplat (Matrix matrix, Terrain terrain, int channel)
			{
				CoordRect terrainRect = terrain.GetSplatRect();
				CoordRect intersection = CoordRect.Intersect(terrainRect, matrix.rect);
				if (intersection.size.x<=0 || intersection.size.z<=0) return;

				float[,,] splats = terrain.terrainData.GetAlphamaps(intersection.offset.x-terrainRect.offset.x, intersection.offset.z-terrainRect.offset.z, intersection.size.x, intersection.size.z);
				int numSplats = splats.GetLength(2);

				Coord min = intersection.Min; Coord max = intersection.Max;
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					int sx = z-min.z;  int sz = x-min.x; //x and z switched
					float val = matrix[x,z];
					float invVal = 1-matrix[x,z];
				
					//multiplying all splats on inverse value
					for (int s=0; s<numSplats; s++) splats[sx,sz,s] *= invVal;

					//adding val
					splats[sx,sz,channel] += val;
				}

				terrain.terrainData.SetAlphamaps(intersection.offset.x-terrainRect.offset.x, intersection.offset.z-terrainRect.offset.z, splats);
			}
		#endregion

		#region Undo
			public class TerrainUndoStep
			{
				public static readonly int cellSize = 64;
				
				public Terrain terrain;
				Matrix2<float[,]> heights = null;
				Matrix2<float[,,]> splats = null;

				public void Add (Vector3 pos, float radius)
				{
					//adding heights
					float pixelSize = terrain.terrainData.size.x / terrain.terrainData.heightmapResolution;
					CoordRect rect = (pos-terrain.transform.localPosition).ToCoordRect(radius, pixelSize);

					CoordRect intersection = CoordRect.Intersect(rect, new CoordRect(0,0,terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution));
					if (intersection.size.x > 0 && intersection.size.z > 0)
					{
						int numHeightCells = Mathf.CeilToInt(1f*terrain.terrainData.heightmapResolution / cellSize);
						if (heights == null) heights = new Matrix2<float[,]>(numHeightCells, numHeightCells);

						foreach (Coord cellCoord in intersection.Cells(cellSize))
						{
							if (heights[cellCoord] != null) continue;
							heights[cellCoord] = terrain.terrainData.SafeGetHeights(cellCoord.x*cellSize, cellCoord.z*cellSize, cellSize, cellSize);
						}
					}

					//adding splats
					if (terrain.terrainData.alphamapLayers == 0) return;
					pixelSize = terrain.terrainData.size.x / terrain.terrainData.alphamapResolution;
					rect = (pos-terrain.transform.localPosition).ToCoordRect(radius, pixelSize);

					intersection = CoordRect.Intersect(rect, new CoordRect(0,0,terrain.terrainData.alphamapResolution,terrain.terrainData.alphamapResolution));
					if (intersection.size.x > 0 && intersection.size.z > 0)
					{
						int numSplatCells = Mathf.CeilToInt(1f*terrain.terrainData.alphamapResolution / cellSize);
						if (splats == null) splats = new Matrix2<float[,,]>(numSplatCells, numSplatCells);

						foreach (Coord cellCoord in intersection.Cells(cellSize))
						{
							if (splats[cellCoord] != null) continue;
							splats[cellCoord] = terrain.terrainData.SafeGetAlphamaps(cellCoord.x*cellSize, cellCoord.z*cellSize, cellSize, cellSize);
						}
					}
				}

				public void Perform ()
				{
					if (terrain == null) return; //in case it was removed
					
					//heights
					if (heights != null)
					{
						for (int x=0; x<heights.rect.size.x; x++)
							for (int z=0; z<heights.rect.size.z; z++)
								if (heights[x,z] != null) terrain.terrainData.SetHeights(x*cellSize, z*cellSize, heights[x,z]);
					}

					//splats
					if (splats != null)
					{
						for (int x=0; x<splats.rect.size.x; x++)
							for (int z=0; z<splats.rect.size.z; z++)
								if (splats[x,z] != null) terrain.terrainData.SetAlphamaps(x*cellSize, z*cellSize, splats[x,z]);
					}
				}

				public void GetCurrentState (TerrainUndoStep reference)
				{
					terrain = reference.terrain;
					
					//heights
					if (reference.heights != null)
					{
						heights = new Matrix2<float[,]>(reference.heights.rect);
						for (int x=0; x<heights.rect.size.x; x++)
							for (int z=0; z<heights.rect.size.z; z++)
								if (reference.heights[x,z] != null) heights[x,z] = terrain.terrainData.SafeGetHeights(x*cellSize, z*cellSize, cellSize, cellSize);
					}

					//splats
					if (reference.splats != null)
					{
						splats = new Matrix2<float[,,]>(reference.splats.rect);
						for (int x=0; x<splats.rect.size.x; x++)
							for (int z=0; z<splats.rect.size.z; z++)
								if (reference.splats[x,z] != null) splats[x,z] = terrain.terrainData.SafeGetAlphamaps(x*cellSize, z*cellSize, cellSize, cellSize);
					}
				}
			}

			public void TestUndo (Vector3 pos, float radius)
			{
				TerrainUndoStep[] terrainUndoSteps = new TerrainUndoStep[terrains.Length];
				for (int t=0; t<terrains.Length; t++)
				{
					terrainUndoSteps[t] = new TerrainUndoStep();
					terrainUndoSteps[t].terrain = terrains[t];
					terrainUndoSteps[t].Add(pos, radius);
				}
				for (int t=0; t<terrains.Length; t++)
				{
					terrainUndoSteps[t].Perform();
				}
			}


			[System.NonSerialized] public Stack<TerrainUndoStep[]> undoSteps = new Stack<TerrainUndoStep[]>();
			[System.NonSerialized] public Stack<TerrainUndoStep[]> redoSteps = new Stack<TerrainUndoStep[]>();
			
			public int currentUndoState = 0;
			public int referenceUndoState = 0;


			public void NewUndo () {undoSteps.Push(new TerrainUndoStep[terrains.Length]);}

			public void AddUndo (Vector3 pos, float radius)
			{
				TerrainUndoStep[] steps = undoSteps.Peek();
				if (steps.Length != terrains.Length) { Debug.LogWarning("Undo terrains mismatch"); return; }

				for (int t=0; t<terrains.Length; t++)
				{
					if (steps[t] == null) steps[t] = new TerrainUndoStep() { terrain=terrains[t] }; 
					if (steps[t].terrain != terrains[t]) { Debug.LogWarning("Undo terrains mismatch"); return; }
					steps[t].Add(pos,radius);
				}
			}

			public void AddGlobalUndo ()
			{
				TerrainUndoStep[] steps = undoSteps.Peek();
				if (steps.Length != terrains.Length) { Debug.LogWarning("Undo terrains mismatch"); return; }

				for (int t=0; t<terrains.Length; t++)
				{
					if (steps[t] == null) steps[t] = new TerrainUndoStep() { terrain=terrains[t] }; 
					if (steps[t].terrain != terrains[t]) { Debug.LogWarning("Undo terrains mismatch"); return; }
					steps[t].Add(terrain.transform.position + terrain.terrainData.size/2f, terrain.terrainData.size.x+1);
				}
			}

			public void PerformUndo ()
			{
				if (currentUndoState < referenceUndoState) //performing undo
				{
					if (undoSteps.Count == 0) return;

					TerrainUndoStep[] steps = undoSteps.Pop();

					//saving current state for redo
					TerrainUndoStep[] rSteps = new TerrainUndoStep[steps.Length];
					for (int t=0; t<steps.Length; t++)
					{
						rSteps[t] = new TerrainUndoStep();
						rSteps[t].GetCurrentState(steps[t]);
					}
					redoSteps.Push(rSteps);

					//performing undo
					for (int t=0; t<steps.Length; t++) steps[t].Perform();
				}
				else if (currentUndoState > referenceUndoState) //performing redo
				{
					if (redoSteps.Count == 0) return;

					TerrainUndoStep[] steps = redoSteps.Pop();
					for (int t=0; t<steps.Length; t++) steps[t].Perform();
				}

				//if (currentUndoState == referenceUndoState) //non-brush update, doing nothing

				referenceUndoState = currentUndoState;
			}

		#endregion


		public struct UndoStep
		{
			float[,] heights;
			int heightsOffsetX; int heightsOffsetZ;
			float[,,] splats;
			int splatsOffsetX; int splatsOffsetZ;

			public UndoStep (float[,] heights, float[,,] splats, int heightsOffsetX, int heightsOffsetZ, int splatsOffsetX, int splatsOffsetZ)
			{
				//clamping offset low (no need to clamp high as float[,] already has proper size)
				if (heightsOffsetX<0) heightsOffsetX=0; if (heightsOffsetZ<0) heightsOffsetZ=0; 
				if (splatsOffsetX<0) splatsOffsetX=0; if (splatsOffsetZ<0) splatsOffsetZ=0; 
				
				this.heightsOffsetX = heightsOffsetX; this.heightsOffsetZ = heightsOffsetZ;
				this.splatsOffsetX = splatsOffsetX; this.splatsOffsetZ = splatsOffsetZ;
				this.heights = heights.Clone() as float[,]; 
				if (splats!=null) this.splats = splats.Clone() as float[,,];
				else this.splats = null;
			}

			public void Perform (TerrainData data)
			{
				data.SetHeights(heightsOffsetX,heightsOffsetZ,heights);
				if (splats!=null) data.SetAlphamaps(splatsOffsetX,splatsOffsetZ,splats);
			}
		}
		public List< List<UndoStep> > undoList = new List< List<UndoStep> >();	
		public bool allowUndo;

	}
}//namespace



