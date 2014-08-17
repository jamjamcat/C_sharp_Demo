//------------------------------------------------------------------
// 2014		www.JamJamCat.com
// Ivory,Tsai
// 
// This build array of textures into a single spritesheet.
// Slice each sprite and set anchor point
// Save as a single PNG file in Asset Path
// Create a demo gameobject to Preview the spritesheet
//------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class CreateSpriteSheet : ScriptableWizard 
{
	public string spriteSheetName = "soldier";//my_file_name";
	public int pixels2Units = 100; // pixels to Units
	public int padding = 4; // pixels btween each image

	// Anchor/Pivot point for each Sprite    
	public enum AnchorPoint    
	{
		Center = 0,
		TopLeft,
		Top,
		TopRight,
		Left,
		Right,
		BottomLeft,
		Bottom,
		BottomRight
	}

	//Position of Anchor    
	public AnchorPoint anchorPos = AnchorPoint.Bottom;

	// Array of input textures  
	public Texture2D[] tex2DAry;

	//-----------------------------------------------------------------------------
	[MenuItem("Assets/Convert To Sprite Sheet")]

	//-----------------------------------------------------------------------------
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard("Create Sprite Sheet (Texture)",typeof(CreateSpriteSheet));
	}
	//-----------------------------------------------------------------------------
	void OnWizardCreate()
	{
		string assetName= GenerateSpriteSheet();
		GeneratePreView(assetName);
	}
	//-----------------------------------------------------------------------------
	string GenerateSpriteSheet()
	{
		// Generate texture names
		string[] tmp_texNameAry= new string[tex2DAry.Length];

		// Generate texture rects
		Rect[] tmp_UVAry = new Rect[tex2DAry.Length];
		Rect[] tmp_rectAry= new Rect[tex2DAry.Length];

		// setup each input texture
		string inputPath;
		for(int i=0; i<tex2DAry.Length; i++)
		{
			inputPath = AssetDatabase.GetAssetPath(tex2DAry[i]);
			ConfigTexture(inputPath);
		}
		
		// Creating a texture in memory
		// Create a new 1x1 texture ARGB32 (32 bit with alpha) and no mipmaps
		Texture2D tex2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		tex2D.alphaIsTransparency = true;
		
		// The PackTextures function build array of textures into a single spritesheet.
		// and returns an array of Rect UV value.
		// which are stored in the AtlasData.UVs member.
		// Set 4096 as maximun size
		tmp_UVAry = tex2D.PackTextures(tex2DAry, padding, 4096);

		for(int i=0; i<tex2DAry.Length; i++)
		{
			// save each sprite rects for auto slice  
			tmp_rectAry[i] = new Rect(tmp_UVAry[i].x * tex2D.width,
			                          tmp_UVAry[i].y * tex2D.height,
			                             (float)tex2DAry[i].width,
			                             (float)tex2DAry[i].height);
			// setup name for each sprite
			// filename plus number
			// Eg: Soldier_7
			tmp_texNameAry[i] = spriteSheetName+"_"+i; 
		}

		if(!Directory.Exists("Assets/Resources"))
		{
			// Create Resources folder in Asset Path 
			AssetDatabase.CreateFolder("Assets", "Resources");
		}

		//Generate file name in Asset Path    
		// Eg:  Assets/Resources/spritesheet_OOXX.png
		string assetName ="spritesheet_" + spriteSheetName;
		string assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/" + assetName + ".png");
		
		//convert to PNG and save to hard-drive
		byte[] bytes = tex2D.EncodeToPNG();
		System.IO.File.WriteAllBytes(assetPath, bytes);
		bytes = null;
		
		//Delete generated texture in memory
		UnityEngine.Object.DestroyImmediate(tex2D);
		
		//Import saved png file into Asset
		AssetDatabase.ImportAsset(assetPath);
		
		//Get Imported Texture
		tex2D = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
		
		//Configure texture as atlas
		ConfigSpriteSheet(AssetDatabase.GetAssetPath(tex2D));

		DoSlicing(AssetDatabase.GetAssetPath(tex2D), tmp_texNameAry, tmp_rectAry);
		return assetName;

	}
	//-----------------------------------------------------------------------------
	void GeneratePreView(string assetName)
	{
		// Create gameobject
		GameObject tmpObject = new GameObject(assetName);
		tmpObject.AddComponent<SpriteRenderer>();
		tmpObject.AddComponent<DemoObject>();
	}
	//-----------------------------------------------------------------------------
	//Fucnction to configure input texture.
	//-----------------------------------------------------------------------------
	public void ConfigTexture(string texturePath)
	{
		TextureImporter texImpo = AssetImporter.GetAtPath(texturePath) as TextureImporter;
		TextureImporterSettings tISettings = new TextureImporterSettings();
		
		texImpo.textureType = TextureImporterType.Advanced;
		texImpo.ReadTextureSettings(tISettings);
		
		tISettings.mipmapEnabled = false;
		tISettings.readable = true;
		tISettings.maxTextureSize = 4096;
		tISettings.textureFormat = TextureImporterFormat.RGBA32;
		tISettings.filterMode = FilterMode.Point;
		tISettings.wrapMode = TextureWrapMode.Clamp;
		tISettings.npotScale = TextureImporterNPOTScale.None;
		tISettings.alphaIsTransparency = true;
		
		texImpo.SetTextureSettings(tISettings);
		
		//Re-import/update Texture
		AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
		AssetDatabase.Refresh();
	}

	//-----------------------------------------------------------------------------
	//Fucnction to configure output SpriteSheet.
	//-----------------------------------------------------------------------------
	public void ConfigSpriteSheet(string texturePath)
	{
		TextureImporter texImpo = AssetImporter.GetAtPath(texturePath) as TextureImporter;
		TextureImporterSettings tISettings = new TextureImporterSettings();
		
		texImpo.textureType = TextureImporterType.Sprite;
		texImpo.spriteImportMode = SpriteImportMode.Multiple;
		texImpo.spritePixelsToUnits= pixels2Units;
		texImpo.ReadTextureSettings(tISettings);
		texImpo.spritePackingTag = spriteSheetName;

		tISettings.filterMode = FilterMode.Point;
		tISettings.maxTextureSize = 4096;
		tISettings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		tISettings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
		tISettings.readable = true;

		texImpo.SetTextureSettings(tISettings);
		
		//Re-import/update Texture
		AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
		AssetDatabase.Refresh();
	}

	//-----------------------------------------------------------------------------
	// Fucnction to slice each sprite and set anchor point
	//-----------------------------------------------------------------------------
	public void DoSlicing(string texturePath,string[] textureNames, Rect[] rects)
	{
		TextureImporter texImpo = AssetImporter.GetAtPath(texturePath) as TextureImporter;
		AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
		AssetDatabase.Refresh();

		List <SpriteMetaData> sMDataLst = new List<SpriteMetaData>();

		for (int i=0; i<tex2DAry.Length; i++)
		{
			SpriteMetaData spriteMD = new SpriteMetaData();
			spriteMD = new SpriteMetaData();
			spriteMD.alignment = (int)anchorPos;
			spriteMD.name = textureNames[i];
			spriteMD.pivot = Vector2.one * 0.5f;
			spriteMD.rect =  rects[i];

			sMDataLst.Add(spriteMD);

			EditorUtility.DisplayProgressBar(
				"Do Slicing on each sprite",
					"Shows a progress",
				i/tex2DAry.Length);

		}
	
		texImpo.spritesheet = sMDataLst.ToArray();

		EditorUtility.SetDirty(texImpo);
		AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

		EditorUtility.DisplayProgressBar("", "Done.", 1);
		EditorUtility.ClearProgressBar();
	}
	//-----------------------------------------------------------------------------
	
}

