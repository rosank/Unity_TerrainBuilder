﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(AshTerrainPainter))]
public class AshTerrainPainterEditor : Editor
{
	AshTerrainPainter tool;
	static bool exp_Tool = true, brushMode = true, exp_Tex = true;
	static float stroke_Opacity = 1;
	static float stroke_Size = 50;
	static int selected_Tex = 1;
	static Vector4 stroke_Channel = Vector4.zero;

	private void Select_Texure(int indx)
	{
		selected_Tex = indx;
		stroke_Channel = Vector4.zero;
		switch (selected_Tex)
		{
			case 1: stroke_Channel.x = 1; break;
			case 2: stroke_Channel.y = 1; break;
			case 3: stroke_Channel.z = 1; break;
			case 4: stroke_Channel.w = 1; break;
		}
	}
	public void Import_SpatialMap()
	{
		var path = EditorUtility.OpenFilePanel("Load SpatialMap", Application.dataPath, "png");
		if (!string.IsNullOrEmpty(path))
		{
			var pngData = File.ReadAllBytes(path);
			var tex = new Texture2D(2, 2);
			if (tex.LoadImage(pngData))
			{
				tool.Set_SpatialMap(tex);
				pngData = null;
			}
		}
	}
	public void Export_SpatialMap()
	{
		var path = EditorUtility.SaveFilePanel("Save SpatialMap", Application.dataPath, "SpatialMap", "png");
		if (!string.IsNullOrEmpty(path))
		{
			var sTex = tool.Get_SpatialMapTex();
			if (sTex != null)
			{
				var pngData = sTex.EncodeToPNG();
				if (pngData != null)
				{
					try
					{
						File.WriteAllBytes(path, pngData);
					}
					catch (System.Exception)
					{
					}
					path = null;
					pngData = null;
				}
			}
		}
	}

	void HandleEvents(SceneView view)
	{
		if (!tool) tool = (target as AshTerrainPainter);
		if (!tool && hooked)
		{
			hooked = false;
			SceneView.duringSceneGui -= HandleEvents;
			return;
		}
		if (selected_Tex > 0 && stroke_Channel == Vector4.zero)
			Select_Texure(selected_Tex);

		var e = Event.current;
		if (e.control || e.alt || e.shift)
			return;

		if (brushMode && e.isMouse && e.button == 0)
		{
			var mp = e.mousePosition;
			switch (e.type)
			{
				case EventType.MouseDown:
				case EventType.MouseDrag:
					var ray = HandleUtility.GUIPointToWorldRay(mp);
					RaycastHit rHit;
					if (Physics.Raycast(ray, out rHit))
					{
						tool.ApplyStroke(rHit.textureCoord, stroke_Channel, stroke_Opacity, stroke_Size);
						GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
						Event.current.Use();
					}
					break;
			}
		}

		if (e.type == EventType.KeyDown && e.isKey)
		{
			switch (e.keyCode)
			{
				case KeyCode.BackQuote:
					Selection.activeGameObject = tool.gameObject;
					EditorUtility.SetDirty(target);
					Event.current.Use();
					break;
				case KeyCode.B:
					brushMode = !brushMode; 
					EditorUtility.SetDirty(target);
					Event.current.Use();
					break;
				case KeyCode.Q:
					if (!brushMode) break;
					stroke_Size = Mathf.Clamp(stroke_Size - 1, 1f, 500f);
					EditorUtility.SetDirty(target);
					Event.current.Use();
					break;
				case KeyCode.E:
					if (!brushMode) break;
					stroke_Size = Mathf.Clamp(stroke_Size + 1, 1f, 500f);
					EditorUtility.SetDirty(target);
					Event.current.Use();
					break;
				case KeyCode.F:
					if (!brushMode) break;
					stroke_Opacity = Mathf.Clamp(stroke_Opacity - 0.05f, 0.1f, 1f);
					EditorUtility.SetDirty(target);
					Event.current.Use();
					break;
				case KeyCode.R:
					if (!brushMode) break;
					stroke_Opacity = Mathf.Clamp(stroke_Opacity + 0.05f, 0.1f, 1f);
					EditorUtility.SetDirty(target);
					Event.current.Use();
					break;
				case KeyCode.G:
					if (!brushMode) break;
					Select_Texure(0);
					Event.current.Use();
					break;
				case KeyCode.Z:
					if (!brushMode) break;
					Select_Texure(1);
					Event.current.Use();
					break;
				case KeyCode.X:
					if (!brushMode) break;
					Select_Texure(2);
					Event.current.Use();
					break;
				case KeyCode.C:
					if (!brushMode) break;
					Select_Texure(3);
					Event.current.Use();
					break;
				case KeyCode.V:
					if (!brushMode) break;
					Select_Texure(4);
					Event.current.Use();
					break;
			}
		}
	}

	public override void OnInspectorGUI()
	{
		if(!tool) tool = (target as AshTerrainPainter);
		DrawDefaultInspector();

		GUILayout.Space(15);
		GUI.color = new Color(0.5f, 0.6f, 1.0f, 1);
		if (GUILayout.Button("Init SpatialMap")) tool.Init_SpatialMap();
		GUILayout.Space(15);
		GUI.color = new Color(0.5f, 1.0f, 0.5f, 1);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Import SpatialMap")) Import_SpatialMap();
		GUILayout.Space(10);
		if (GUILayout.Button("Export SpatialMap")) Export_SpatialMap();
		GUILayout.EndHorizontal();

		PainterWindow(0);
	}

	static bool hooked = false;
	void OnSceneGUI()
	{
		if (!tool) tool = (target as AshTerrainPainter);
		if (!hooked)
		{
			hooked = true;
			SceneView.duringSceneGui -= HandleEvents;
			SceneView.duringSceneGui += HandleEvents;
		}
	}
	
	int t_Box = 100;
	private void PainterWindow(int id)
	{
		GUI.color = Color.white;
		exp_Tool = GUILayout.Toggle(exp_Tool, "Ash Mesh Painter ~", "Button");

		if (exp_Tool)
		{
			GUILayout.BeginVertical();

			GUILayout.Space(15);
			GUILayout.BeginHorizontal();
			brushMode = GUILayout.Toggle(brushMode, "Brush Mode B", "Button");
			GUILayout.Space(10);
			GUI.color = Color.white * stroke_Opacity;
			GUILayout.Box(tool.brush, GUILayout.Width(32), GUILayout.Height(32));
			GUILayout.EndHorizontal();
			GUI.color = Color.white;
			if (brushMode)
			{
				GUILayout.Space(5);
				stroke_Opacity = EditorGUILayout.Slider("Opacity +,-", stroke_Opacity, 0.1f, 1f);
				GUILayout.Space(5);
				stroke_Size = EditorGUILayout.Slider("Size [,]", stroke_Size, 1f, 500f);
			}


			GUILayout.Space(15);
			exp_Tex = GUILayout.Toggle(exp_Tex, "Show Textures T", "Button");
			if (exp_Tex)
			{
				GUILayout.BeginHorizontal();
				GUI.color = Color.white / 2;
				GUILayout.Button(tool.spatialMap, GUILayout.Width(t_Box), GUILayout.Height(t_Box));

				if (tool.spatialChannels.Length > 0)
					GUILayout.Button(tool.spatialChannels[0].tex, GUILayout.Width(t_Box), GUILayout.Height(t_Box));
				else
					GUILayout.Button("Base Tex", GUILayout.Width(t_Box), GUILayout.Height(t_Box));
				GUILayout.EndHorizontal();

				GUILayout.Space(5);
				for (int i = 1; i < 5; i++)
				{
					if (selected_Tex == i)
						GUI.color = new Color(0.5f, 1.0f, 0.5f, 0.5f);
					else
						GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
					if (i % 2 == 1)
					{
						GUILayout.BeginHorizontal();
					}
					if (tool.spatialChannels.Length > i)
					{
						var sel = GUILayout.Button(tool.spatialChannels[i].tex, GUILayout.Width(t_Box), GUILayout.Height(t_Box));
						if (sel)
							Select_Texure(i);
					}
					else
					{
						GUILayout.Button("NONE", GUILayout.Width(t_Box), GUILayout.Height(t_Box));
					}

					if ((i + 1) % 2 == 1)
					{
						GUILayout.EndHorizontal();
						GUILayout.Space(5);
					}
				}
				GUILayout.Space(5);
			}
			GUILayout.EndVertical();
		}
	}

}
