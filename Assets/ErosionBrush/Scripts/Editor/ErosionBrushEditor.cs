using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

////using Plugins;

namespace ErosionBrushPlugin
{
[CustomEditor(typeof(ErosionBrush))]
public class ErosionBrushEditor : Layout
{
	public ErosionBrush script; //public: save preset window uses it
	Preset preset;

	private Vector2 oldMousePos = new Vector2(0,0); //checks dist before DrawBrush
	private Vector3 oldBrushPos = new Vector3(0,0,0); //checks in Edit to perform brush spacing

	public bool test = false;

	public int unity5terrainRefreshCounter = 0;

	GUIContent[] presetContents = new GUIContent[0];

	int presetSelectedFromKeyboard = -1;


	public void OnEnable ()
	{
		if (script==null) script = (ErosionBrush) target;
		if (script==null || !script.enabled) return;
			
		UnityEditor.EditorApplication.update -= EditorUpdate;
		UnityEditor.EditorApplication.update += EditorUpdate;

		Undo.undoRedoPerformed -= script.PerformUndo;
		Undo.undoRedoPerformed += script.PerformUndo;
	}
	
	public void OnDisable ()
	{
		if (script==null) script = (ErosionBrush)target;
		if (script==null) return;
		UnityEditor.EditorApplication.update -= EditorUpdate;

		Undo.undoRedoPerformed -= script.PerformUndo;
	}

	#region Inspector

	public override void OnInspectorGUI () 
	{
		script = (ErosionBrush) target;
		preset = script.preset;

		GetInspectorField();
		margin = 0;
		rightMargin = 0;
		fieldSize = 0.6f;

		//evaluation version
		//bool paintDisabled = false;
		//if (script.terrain.terrainData.heightmapResolution-1 > 512 ||
		//	script.terrain.terrainData.alphamapResolution > 512)
		//{
		//	Par(62); 
		//	EditorGUI.HelpBox(Inset(), "Evaluation version.\nTerrain maximum resolution is limited to 512 pixels.\n", MessageType.Warning);
		//	Par(20); cursor.y -= 24;
		//	Inset(40);
		//	if (GUI.Button(Inset(width-50), "Switch resolution to 512"))
		//		if (EditorUtility.DisplayDialog("Warning", "Changing resolution will remove all terrain data. " +
		//			"This operation is not undoable. Please make a backup copy of your terrain data (not scene, but terrain .asset file).", "Switch", "Cancel"))
		//				{ script.terrain.terrainData.alphamapResolution = 512; script.terrain.terrainData.heightmapResolution = 513; }
		//	disabled = true;
		//}

		//drawing toolbar
		if (script.guiHydraulicIcon==null) script.guiHydraulicIcon = Resources.Load("ErosionBrushHydraulic") as Texture2D;
		if (script.guiWindIcon==null) script.guiWindIcon = Resources.Load("ErosionBrushNoise") as Texture2D;

		Par(5);
		Par(22);

		//paint button
		if (disabled) script.paint = false;
		Button(ref script.paint, "Paint", toggle:true, width:0.3f, tooltip:"A checkbutton that turns erosion or noise painting on/off. When painting is on it is terrain editing with standard Unity tools is not possible, so terrain component is disabled when “Paint” is checked. To enable terrain editing turn off paint mode.");
		disabled = false;

		//mode selector
		Inset(0.1f);
		bool oldIsErosion = preset.isErosion; bool oldIsNoise = preset.isNoise;
		Rect erosionRect = Inset(0.3f); Rect noiseRect = Inset(0.3f);
		preset.isErosion = GUI.Toggle(erosionRect, preset.isErosion, new GUIContent(" Erosion", script.guiHydraulicIcon, ""), GUI.skin.button);
		preset.isNoise = GUI.Toggle(noiseRect, preset.isNoise, new GUIContent(" Noise", script.guiWindIcon, ""), GUI.skin.button);
		
		//selecting one mode
		bool controlClick = (erosionRect.Contains(Event.current.mousePosition) || noiseRect.Contains(Event.current.mousePosition)) && Event.current.control && Event.current.type == EventType.used;
		if (!controlClick)
		{
			if (oldIsErosion && oldIsNoise) //if both togles enabled - inverted case
			{
				if (!preset.isNoise && oldIsNoise) { preset.isErosion = false; preset.isNoise = true; }
				if (!preset.isErosion && oldIsErosion) { preset.isErosion = true; preset.isNoise = false; }
			}
			else //default case
			{
				if (preset.isNoise && !oldIsNoise) preset.isErosion = false;
				if (preset.isErosion && !oldIsErosion) preset.isNoise = false;
			}
		}
		else script.guiControlUsed = true;

		if (!preset.isErosion && !preset.isNoise)
		{
			if (oldIsErosion) preset.isErosion = true;
			if (oldIsNoise) preset.isNoise = true;
			if (!preset.isErosion && !preset.isNoise) preset.isErosion = true; //if still none selected - selecting erosion
		}

		//control-click hint
		
		if (!script.guiControlUsed) { Par(22); EditorGUI.HelpBox(Inset(1), "Use Control-click to select both modes", MessageType.Info); }

		/*int toolBarVal = GUI.Toolbar(Inset(0.83f), -1, new GUIContent[] { 
			new GUIContent(" Erosion", script.guiHydraulicIcon, ""),
			new GUIContent(" Noise", script.guiWindIcon, ""),
			new GUIContent(" Both", script.guiHydraulicIcon, "") });
		switch (toolBarVal)
		{
			case 0: preset.isErosion = true; preset.isNoise = false; break;
			case 1: preset.isErosion = false; preset.isNoise = true; break;
			case 2: preset.isErosion = true; preset.isNoise = true; break;
		}*/

		margin += 7;


		#region Preset
		Par(5); Par(); Foldout(ref script.guiShowPreset, "Preset");
		if (script.guiShowPreset)
		{
			//calculating a need to re-create preset array
			bool reCreate = false;
			if (presetContents.Length != script.presets.Length) reCreate = true;
			else 
				for (int i=0; i<presetContents.Length; i++) 
					if (presetContents[i].text != script.presets[i].name) { reCreate=true; break; }

			//re-creating presets contents
			if (reCreate)
			{
				presetContents = new GUIContent[script.presets.Length];
				for (int i=0; i<presetContents.Length; i++) 
				{
					string postfix = "";
					//if (i==0) postfix = " (1)";
					if (i<8) postfix = " (key " + (i+3) + ")";
					presetContents[i] = new GUIContent(script.presets[i].name + postfix);
				}
			}
			
			//selecting preset
			margin += 10;
			Par();
			int tempSelectedPreset = EditorGUI.Popup(Inset(), script.guiSelectedPreset, presetContents);
			if (presetSelectedFromKeyboard >= 0) { tempSelectedPreset = presetSelectedFromKeyboard; presetSelectedFromKeyboard = -1; }
			if (tempSelectedPreset != script.guiSelectedPreset && tempSelectedPreset < script.presets.Length)
			{
				LoadPreset(tempSelectedPreset);
				script.guiSelectedPreset = tempSelectedPreset;
			}

			//save, add, remove
			Par(); disabled = script.presets.Length==0;
			if (Button("Save", tooltip:"Save current preset changes", width:0.3333f) &&
				EditorUtility.DisplayDialog("Overwrite Preset", "Overwrite currently selected preset?", "Save", "Cancel") )
					SavePreset(script.guiSelectedPreset);
			
			disabled = false;
			if (Button("Save As...", tooltip:"Save current settings as new preset", width:0.3333f))
			{
				SavePresetWindow window = new SavePresetWindow();
				window.titleContent = new GUIContent("Save Erosion Brush Preset");
				window.position = new Rect(window.position.x, window.position.y, window.windowSize.x, window.windowSize.y);
				window.main = this;
				window.ShowUtility();
			}

			disabled =script.presets.Length==0;
			if (Button("Remove", tooltip:"Remove currently selected preset", width:0.3333f) &&
				EditorUtility.DisplayDialog("Remove Preset", "Are you sure you wish to remove currently selected preset?", "Remove", "Cancel"))
					RemovePreset(script.guiSelectedPreset);
			disabled = false;

			//DrawLabel(script.preset.name + " " + script.guiSelectedPreset.ToString() + "/" + script.presets.Length.ToString());

			margin -= 10;
		}
		#endregion

		#region brush settings
		Par(5); Par(); Foldout(ref script.guiShowBrush, "Brush Settings");
		if (script.guiShowBrush)
		{
			margin += 10;
			
			Quick<float>(ref preset.brushSize, "Brush Size", min:1, max:script.guiMaxBrushSize, tooltip:"Size of the brush in Unity units. Bigger brush size gives better terrain quality, but too big values can slow painting. Brush size is displayed as brighter circle in scene view. Brush could be resized with [ and ] keys.",  quadratic:true);
			Quick<float>(ref preset.brushFallof, "Brush Falloff", min:0.01f, max:0.99f, tooltip:"Decrease of brush opacity from center to rim. This parameter is specified in percent of the brush size. It is displayed as dark blue circle in scene view. Brush inside of the circle has the full opacity, and gradually decreases toward the bright circle.");
			Quick<float>(ref preset.brushSpacing, "Brush Spacing", min:0, max:1, tooltip:"When pressing and holding mouse button brush goes on making stamps. Script will not place brush at the same position where old brush was placed, but in a little distance. This parameter specifies how far from old brush stamp will be placed new one (while mouse is still pressed). It  is specified in percent of the brush size.");
			Quick<int>(ref preset.downscale, "Downscale", min:1, max:4, tooltip:"To perform quick operation on heightmaps of large size brush resolution could be scaled down. This will give less detail, but faster stamp.", quadratic:true);
			preset.downscale = Mathf.ClosestPowerOfTwo(preset.downscale);
			EditorGUI.BeginDisabledGroup(preset.downscale==1);
			Quick<bool>(ref preset.preserveDetail, "Preserve Detail", "All the terrain detail edited with Downscale parameter will be returned on upscale");
			EditorGUI.EndDisabledGroup();

			//Quick<float>(ref preset.blur, "Blur", min:0, max:1, tooltip:"The amount brush stamp should be blurred before apply. This parameter is very useful together with the donscale: faceted downscaled data could be blurred to give smooth result");
			
			margin -= 10;
		}
		#endregion

		#region generator settings 
		if (preset.isErosion)
		{
			Par(5); Par();
			Foldout(ref script.guiShowErosion, "Erosion Parameters");
			if (script.guiShowErosion)
			{
				margin += 10;

				//Noise Brush
				//Par(30); Label("Noise Brush is a free version \nof Erosion Brush plugin."); 
				//Par(45); Label("To generate both erosion and \nnoise with the same tool \nconsider using Erosion Brush");
				//Par(5);
				//Par(); Url("https://www.assetstore.unity3d.com/en/#!/content/27389", "Asset Store link");
				//Par(); Url("https://www.youtube.com/watch?v=bU88tkrBbb0", "Video");
				//Par(); Url("http://www.denispahunov.ru/ErosionBrush/eval.html", "Evaluation Version");
				
				//Quick<int>(ref erosion_iterations, "Iterations", "Number of algorithm iterations. Higher values will make terrain more eroded. Lowering this value and increasing amounts can speed up terrain generation, but will affect terrain quality.", min:1, max:20);
				Quick<float>(ref preset.erosion_durability, "Terrain Durability", "Baserock resistance to water erosion. Low values erode terrain more. Lowering this parameter is mainly needed to reduce the number of brush passes (iterations), but will reduce terrain quality as well.", max:1);
				Quick<int>(ref preset.erosion_fluidityIterations, "Fluidity Iterations", "This parameter sets how liquid sediment (bedrock raised by torrents) is. Low parameter value will stick sediment on sheer cliffs, high value will allow sediment to drain in hollows. As this parameter sets number of iterations, increasing it to very high values can slow down performance.", min:1, max:10);
				Quick<float>(ref preset.erosion_amount, "Erosion Amount", "Amount of bedrock that is washed away by torrents. Unlike sediment amount, this parameter sets the amount of bedrock that is subtracted from original terrain. Zero value will not erode terrain by water at all.", max:2);
				Quick<float>(ref preset.sediment_amount, "Sediment Amount", "Percent of bedrock raised by torrents that returns back to earth ) Unlike erosion amount, this parameter sets amount of land that is added to terrain. Zero value will not generate any sediment at all.", max:2);
				//Quick<float>(ref preset.wind_amount, "Wind Amount", "Wind sets the amount of bedrock that was carried away by wind, rockfall and other factors non-related with water erosion. Technically it randomly smoothes the convex surfaces of the terrain. Use low values for tropical rocks (as they are more influenced by monsoon, rains and water erosion than by wind), and high values for highland pikes (as all streams freeze at high altitudes).", max:2);
				Quick<float>(ref preset.ruffle, "Ruffle", "Adds smoe randomness on the slopes of the cliffs.", max:1);
				Quick<float>(ref preset.erosion_smooth, "Smooth", "Applies additional smoothness to terrain in order to fit brush terrain into an existing terrain made with Unity standard tools. Low, but non-zero values can remove small pikes made by wind randomness or left from water erosion. Use low values if your terrain heightmap resolution is low.", max:1);
			
				margin -= 10;
			}
		}


		if (preset.isNoise)
		{
			Par(5); Par();
			Foldout(ref script.guiShowErosion, "Noise Parameters");
			if (script.guiShowErosion)
			{
				margin += 10;

				int tempSeed = preset.noise_seed;
				Quick<int>(ref tempSeed, "Seed", "Number to initialize random generator. With the same brush size, noise size and seed the noise value will be constant for each heightmap coordinate.", slider:false);
				//if (preset.noise_seed != tempSeed) { Noise.seed = tempSeed; preset.noise_seed = tempSeed; UnityEngine.Random.seed = tempSeed; }
				
				Quick<float>(ref preset.noise_amount, "Amount", tooltip:"Magnitude. How much noise affects the surface", quadratic:true, max:100f);
				Quick<float>(ref preset.noise_size, "Size", tooltip:"Wavelength. Sets the size of the highest iteration of fractal noise. High values will create more irregular noise. This parameter represents the percentage of brush size.", max:1000, quadratic:true);
				Quick<float>(ref preset.noise_detail, "Detail", "Defines the bias of each fractal. Low values sets low influence of low-sized fractals and high influence of high fractals. Low values will give smooth terrain, high values - detailed and even too noisy.", max:1);
				Quick<float>(ref preset.noise_uplift, "Uplift", "When value is 0, noise is subtracted from terrain. When value is 1, noise is added to terrain. Value of 0.5 will mainly remain terrain on the same level, lifting or lowering individual areas.", max:1);
				//Quick<float>(ref preset.noise_ruffle, "Ruffle", "Adds additional shallow (1-unit) noise to the resulting heightmap", max:2);

				margin -= 10;
			}
		}
		#endregion

		#region texture settings
		Par(5); Par(); Foldout(ref script.guiShowTextures, "Textures");
		if (script.guiShowTextures)
		{
			margin += 10;

			//refreshing terrains
			script.terrains = script.GetTerrains();
			
			if (script.terrains.Length != 0)
			{
				SplatPrototype[] splats = script.terrains[0].terrainData.splatPrototypes;
				Texture2D[] textures = new Texture2D[splats.Length];
				for (int i=0; i<splats.Length; i++) textures[i] = splats[i].texture;

				Par();
				Field<bool>(ref preset.foreground.apply, width:20);
				Label("Crag", width:70);
				Slider<float>(ref preset.foreground.opacity, width:width-130, max:10, quadratic:true);
				Field<float>(ref preset.foreground.opacity, width:40);
				Par(42); TextureSelector(ref preset.foreground.num, textures);

				Par(5); Par();
				Field<bool>(ref preset.background.apply, width:20);
				Label("Sediment", width:70);
				Slider<float>(ref preset.background.opacity, width:width-130, max:10, quadratic:true);
				Field<float>(ref preset.background.opacity, width:40);
				Par(42); TextureSelector(ref preset.background.num, textures);
			}

			margin -= 10;
		}
		#endregion

		#region apply to whole terrain
		Par(5); Par(); Foldout(ref script.guiShowGlobal, "Global Brush", "Apply Erosion Brush to whole terrain at once");
		if (script.guiShowGlobal)
		{
			margin += 10; Par();
			if (Button("Apply to Whole Terrain"))
			{
				script.terrains = script.GetTerrains();

				//recording undo
				script.NewUndo();
				script.referenceUndoState = script.currentUndoState+1;
				Undo.RecordObject(script, "Erosion Brush Stroke");
				script.currentUndoState++;
				EditorUtility.SetDirty(script);
				script.AddGlobalUndo();

				for (int i=0; i<script.guiApplyIterations; i++) 
					for (int t=0; t<script.terrains.Length; t++)
						script.ApplyBrush(
							script.terrains[t].transform.position + script.terrains[t].terrainData.size/2, 
							Mathf.Max(script.terrains[t].terrainData.size.x, script.terrains[t].terrainData.size.y), 
							useFallof:false);
			}
			Quick<int>(ref script.guiApplyIterations, "Iterations", max:20);
			margin -= 10;
		}
		#endregion

		#region settings
		Par(5); Par(); Foldout(ref script.guiShowSettings, "Settings");
		if (script.guiShowSettings)
		{
			margin += 10;
			Quick<Color>(ref script.guiBrushColor, "Brush Color", "Visual representation of the brush.");
			Quick<float>(ref script.guiBrushThickness, "Brush Thickness", "Visual representation of the brush.", slider:false);
			Quick<int>(ref script.guiBrushNumCorners, "Brush Num Corners", "Visual representation of the brush.", slider:false);
			//Quick<bool>(ref script.unity5positioning, "Fix Unity5 Brush Positioning", "Unity5 Beta has incorrect terrain brush positioning (Both in Erosion Brush and Standard Terrain sculpting). Turn toggle on to fix it. WARNING: This fix is a crutch that bypasses known Unity's bug, turning it on causes some lag.");
			//Quick<bool>(ref script.recordUndo, "Record Undo", "Disabling can increase performance a bit, but will make undo unavailable");
			//Quick<bool>(ref script.focusOnBrush, "G Focuses on Brush", "Analog of F button, but it will focus camera not on the whole terrain, but on current brush position.");
			Quick<int>(ref script.guiMaxBrushSize, "Max Brush Size", "Brush size slider maximum. Note that increasing brush size will reduce performance in the quadratic dependence.", slider:false);
			if (script.guiMaxBrushSize > 100) { Par(40); EditorGUI.HelpBox(Inset(), "Increasing brush size will reduce performance in the quadratic dependence.", MessageType.Warning); }
			margin -= 10;
		}
		#endregion

		#region about
		Par(5); Par(); Foldout(ref script.guiShowAbout, "About");
		if (script.guiShowAbout)
		{
			
			Par(50+2);
			if (script.guiPluginIcon==null) script.guiPluginIcon = Resources.Load("ErosionBrushIcon") as Texture2D;
			EditorGUI.DrawPreviewTexture(Inset(50+2), script.guiPluginIcon); 
			cursor.y -= 50; cursor.y -= 7;
			
			margin = 70; 
			
			Par(); Label("Erosion Brush v1.52_u5");
			Par(); Label("by Denis Pahunov");
			Par(5);
			Par(); Label("Useful Links:");
			Par(); Url("http://www.denispahunov.ru/ErosionBrush/doc.html", " - Online Documentation");
			Par(); Url("http://www.youtube.com/watch?v=bU88tkrBbb0", " - Video Tutorial");
			Par(); Url("http://forum.unity3d.com/threads/erosion-brush-a-tool-to-paint-terrain-with-noise-and-erosion.290257/", " - Forum Thread", "Question and answers");
			Par(); Url("https://www.facebook.com/ErosionBrush", " - Facebook", "News, anounces, contests");

			Par(); Label("On any issues related with plugin");
			Par(); Label("functioning you can contact the");
			Par(); Label("author by mail:");
			Par(); Url("mailto:mail@denispahunov.ru", "mail@denispahunov.ru"); 

			//margin = 10;
			//Par(1); lastRect.y -= 208; lastRect.height = 50;
			//if (script.guiPluginIcon==null) script.guiPluginIcon = Resources.Load("ErosionBrushIcon") as Texture2D;
			//EditorGUI.DrawPreviewTexture(Inset(50), script.guiPluginIcon);
		}
		#endregion

		SetInspectorField();
	}


	public void SavePreset (int num, string name="", bool saveBrushSize=true, bool saveBrushParams=true, bool saveErosionNoiseParams=true, bool saveSplatParams=true)
	{
		Preset presetCopy = script.preset.Copy();

		if (num<0 || num>=script.presets.Length)
		{
			//setting save params for a new preset
			presetCopy = preset.Copy();
			presetCopy.name = name;
			presetCopy.saveBrushSize = saveBrushSize;
			presetCopy.saveBrushParams = saveBrushParams;
			presetCopy.saveErosionNoiseParams = saveErosionNoiseParams;
			presetCopy.saveSplatParams = saveSplatParams;

			//extending array if num is negative
			Array.Resize(ref script.presets, script.presets.Length+1);
			num = script.presets.Length - 1;
		}

		script.presets[num] = presetCopy;

		LoadPreset(num); //loading name, save params. And just to make sure preset was saved.
	}

	public void LoadPreset (int num)
	{
		if (num < 0 || num > script.presets.Length-1) return;
		
		Preset preset = script.presets[num];
		script.guiSelectedPreset = num;

		script.preset.name = preset.name;
		script.preset.saveBrushSize = preset.saveBrushSize;
		script.preset.saveBrushParams = preset.saveBrushParams;
		script.preset.saveErosionNoiseParams = preset.saveErosionNoiseParams;
		script.preset.saveSplatParams = preset.saveSplatParams;

		if (preset.saveBrushSize) script.preset.brushSize = preset.brushSize;

		if (preset.saveBrushParams)
		{
			script.preset.brushFallof = preset.brushFallof;
			script.preset.brushSpacing = preset.brushSpacing;
			script.preset.downscale = preset.downscale;
			script.preset.blur = preset.blur;
			script.preset.preserveDetail = preset.preserveDetail;
		}

		if (preset.saveErosionNoiseParams)
		{
			script.preset.isErosion = preset.isErosion;
			
			script.preset.noise_seed = preset.noise_seed;
			script.preset.noise_amount = preset.noise_amount;
			script.preset.noise_size = preset.noise_size;
			script.preset.noise_detail = preset.noise_detail;
			script.preset.noise_uplift = preset.noise_uplift;
			script.preset.noise_ruffle = preset.noise_ruffle;

			script.preset.erosion_iterations = preset.erosion_iterations;
			script.preset.erosion_durability = preset.erosion_durability;
			script.preset.erosion_fluidityIterations = preset.erosion_fluidityIterations;
			script.preset.erosion_amount = preset.erosion_amount;
			script.preset.sediment_amount = preset.sediment_amount;
			script.preset.wind_amount = preset.wind_amount;
			script.preset.erosion_smooth = preset.erosion_smooth;
			script.preset.ruffle = preset.ruffle;
		}

		if (preset.saveSplatParams)
		{
			script.preset.foreground = preset.foreground;
			script.preset.background = preset.background;
		}

		this.Repaint();
	}

	public void RemovePreset (int num) 
	{ 
		ArrayRemoveAt<Preset>(ref script.presets, num); 

		script.guiSelectedPreset = Mathf.Clamp(script.guiSelectedPreset, 0, script.presets.Length-1);
		LoadPreset(script.guiSelectedPreset);
	}
	
	#endregion //Inspector region


	#region Scene

	public void OnSceneGUI ()
	{
		script = (ErosionBrush) target;
		preset = script.preset;
		
		if (!script.paint || (Event.current.mousePosition-oldMousePos).sqrMagnitude<1f) return;

		//reading keyboard
		if (Event.current.type == EventType.keyDown)
		{
			//selecting presets with keycode
			if (script.guiSelectPresetsUsingNumkeys) 
			{
				int key = -1;
				switch (Event.current.keyCode)
				{
					//case KeyCode.Alpha1: key = 0; break;
					//case KeyCode.Alpha2: key = 2; break;
					case KeyCode.Alpha3: key = 0; break;
					case KeyCode.Alpha4: key = 1; break;
					case KeyCode.Alpha5: key = 2; break;
					case KeyCode.Alpha6: key = 3; break;
					case KeyCode.Alpha7: key = 4; break;
					case KeyCode.Alpha8: key = 5; break;
					case KeyCode.Alpha9: key = 6; break;
				}
				if (key >= 0 && key < script.presets.Length) { LoadPreset(key); script.guiSelectedPreset=key; } 
			}

			//extending brush size with keykode
			if (Event.current.keyCode == KeyCode.LeftBracket || Event.current.keyCode == KeyCode.RightBracket)
			{
				float step = (script.preset.brushSize / 10);
				step = Mathf.RoundToInt(step);
				step = Mathf.Max(1,step);

				if (Event.current.keyCode == KeyCode.LeftBracket) script.preset.brushSize -= step;
				else script.preset.brushSize += step;

				script.preset.brushSize = Mathf.Min(script.guiMaxBrushSize, script.preset.brushSize);
			}
		}

		//evaluation limitation
		//if (data.heightmapResolution-1 > 512 ||
		//	data.alphamapResolution > 512) return;

		//refreshing terrains
		script.terrains = script.GetTerrains();

		//disabling selection
		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

		//finding aiming ray
		Vector2 mousePos = Event.current.mousePosition;
		mousePos.y = Screen.height - mousePos.y - 40;
		Camera cam = UnityEditor.SceneView.lastActiveSceneView.camera;
		if (cam==null) return;
		Ray aimRay = cam.ScreenPointToRay(mousePos);

		//aiming terrains
		Vector3 brushPos = Vector3.zero; bool terrainsAimed = false;
		RaycastHit hit;
		for (int t=0; t<script.terrains.Length; t++)
		{
			Collider terrainCollider = script.terrains[t].GetComponent<Collider>();
			if (terrainCollider==null) continue;
			if (terrainCollider.Raycast(aimRay, out hit, Mathf.Infinity)) {brushPos=hit.point; terrainsAimed=true; }
		}
		if (!terrainsAimed) return;

		//drawing brush
		DrawBrush(brushPos, preset.brushSize, script.terrains, color:script.guiBrushColor, thickness:script.guiBrushThickness, numCorners:script.guiBrushNumCorners);
		DrawBrush(brushPos, preset.brushSize/2, script.terrains, color:script.guiBrushColor/2, thickness:script.guiBrushThickness, numCorners:script.guiBrushNumCorners);

		//repainting brush
		HandleUtility.Repaint();

		//focusing on brush
		if(Event.current.commandName == "FrameSelected")
		{ 
			Event.current.Use();
			UnityEditor.SceneView.lastActiveSceneView.LookAt( 
				brushPos, 
				UnityEditor.SceneView.lastActiveSceneView.rotation,
				preset.brushSize*6, 
				UnityEditor.SceneView.lastActiveSceneView.orthographic, 
				false);
		}

		//apply brush and undo
		if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
			Event.current.button == 0 &&
			!Event.current.alt)
			{
				//starting undo record
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
				{
					script.NewUndo();

					script.referenceUndoState = script.currentUndoState+1;
					Undo.RecordObject(script, "TS Terrain Brush");
					script.currentUndoState++;
					EditorUtility.SetDirty(script);
				}

				//recording undo
				script.AddUndo(brushPos, preset.brushSize);
					
				//apply brush
				script.ApplyBrush(brushPos, preset.brushSize);
			}

		//pirate version limitation
		if (false)//(script.applyCount >= 1000)
		{
			EditorUtility.DisplayDialog("Erosion Brush", "Arrrr! You are using the special Erosion Brush: Pirate Edition. " +
			"This means that you have downloaded this plugin not from the official Unity Asset Store page but from an unknown (and probably very suspicious) location. Good job!\n\n" + 
			"Now that you have proved yourself a dashing pirate, as cool as Jack Sparrow (oops, Captain Jack Sparrow) or Jethro Flint, think about buying the product. You'll get: \n" +
			" - regular updates without any headaches;\n" +
			" - full source code;\n" +
			" - removal of this annoying message;\n" +
			" - and no bitcoin mining in the background! (ps: it wasn't me who inserted the miner in the dll, it was probably the guy you downloaded this version from).",
			"To Store", "Close"); 
			script.applyCount = 0;
		}
	}

	public void DrawBrush (Vector3 pos, float radius, Terrain[] terrains, Color color, float thickness=3f, int numCorners=32)
	{
		//incline is the height delta in one unit distance
		Handles.color = color;
		
		Vector3[] corners = new Vector3[numCorners+1];
		float step = 360f/numCorners;
		for (int i=0; i<=corners.Length-1; i++)
		{
			//corner initial position
			Vector3 corner = new Vector3( Mathf.Sin(step*i*Mathf.Deg2Rad), 0, Mathf.Cos(step*i*Mathf.Deg2Rad) ) * radius + pos;

			//finding proper terrain
			Terrain terrain = null;
			for (int t=0; t<terrains.Length; t++)
			{
				Terrain tc = terrains[t];
				if (tc.transform.position.x < corner.x &&
					tc.transform.position.z < corner.z &&
					tc.transform.position.x+tc.terrainData.size.x > corner.x &&
					tc.transform.position.z+tc.terrainData.size.z > corner.z) { terrain = tc; break; }
			}

			//sampling height
			corners[i] = corner;
			if (terrain != null) corners[i].y = terrain.SampleHeight(corner);
		}
		Handles.DrawAAPolyLine(thickness, corners);
	}

	public void EditorUpdate ()
	{
		if (script==null) return; //in case of re-assigning missing script
		
		//finding terrain
		if (script.terrain==null) 
			try { script.terrain = script.GetComponent<Terrain>(); }
			catch (Exception e) { UnityEditor.EditorApplication.update -= EditorUpdate; e.GetType(); } //get type to disable warinng 'never used'
		if (script.terrain==null) return;
		
		RefreshTerrainGui();
	}

	public void RefreshTerrainGui ()
	{
		//returning components order to finish refresh
		if (script.moveDown) 
		{ 
			script.moveDown=false;
			UnityEditorInternal.ComponentUtility.MoveComponentDown(script); 
		}

		//disabling terrain tool if pain is turned on
		if (script.paint && !script.wasPaint)
		{
			script.wasPaint = true;

			//finding terrain reflections
			System.Type terrainType = null;
			System.Type[] tmp = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetTypes();
				for (int i=tmp.Length-1; i>=0; i--) 
			{
				//if (tmp[i].Name.ToLower().Contains("terrain"))
				//	Debug.Log(tmp[i]);
				if (tmp[i].Name=="TerrainInspector") 
						{ terrainType=tmp[i]; break; } //GetType just by name do not work
			}

			object[] editors = Resources.FindObjectsOfTypeAll(terrainType);
			for (int i=0; i<editors.Length; i++)
			{
				PropertyInfo toolProp = terrainType.GetProperty("selectedTool", BindingFlags.Instance | BindingFlags.NonPublic);	

				toolProp.SetValue(editors[i], -1, null);

				//moving component up to refresh terrain tool state
				UnityEditorInternal.ComponentUtility.MoveComponentUp(script); 
				script.moveDown=true;
			}

			script.terrain.hideFlags = HideFlags.NotEditable;
		}

		//enabling terrain if pain was turned off
		if (!script.paint && script.wasPaint)
		{
			script.wasPaint = false;
			script.terrain.hideFlags = HideFlags.None; 
		}
	}
	#endregion //Scene region

}//EB editor

public class SavePresetWindow : EditorWindow
{
	public ErosionBrushEditor main;
	public readonly Vector2 windowSize = new Vector2(300, 120);
	
	public new string name;
	public bool saveBrushSize = false;
	public bool saveBrushParams = true;
	public bool saveErosionNoiseParams = true;
	public bool saveSplatParams = true;
	
	public void OnGUI ()
	{
		EditorGUIUtility.labelWidth = 50;
		
		name = EditorGUILayout.TextField("Name:", name);

		EditorGUILayout.Space();
		saveBrushSize = EditorGUILayout.ToggleLeft(new GUIContent("Save Brush Size", "Each time the preset will be selected Brush Size will be set to current one."), saveBrushSize);
		saveBrushParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Brush Parameters", "Brush fallof, spacing, downscale and blur"), saveBrushParams);
		if (main.script.preset.isErosion) saveErosionNoiseParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Erosion Parameters", "Durability, fluidity and amounts"), saveErosionNoiseParams);
		else saveErosionNoiseParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Noise Parameters", "Amount, size, detail, uplift and riffle"), saveErosionNoiseParams);
		saveSplatParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Splat Parameters", "Splats num and opacity"), saveSplatParams);

		EditorGUILayout.Space();
		if (GUILayout.Button(new GUIContent("Save", "Save current splat to list"))) 
		{
			main.SavePreset(-1, name, saveBrushSize, saveBrushParams, saveErosionNoiseParams, saveSplatParams);
			main.script.guiSelectedPreset = main.script.presets.Length-1;
			this.Close();
		}
	}
}


}//namespace