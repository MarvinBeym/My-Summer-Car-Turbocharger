using MSCLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrewablePartAPI;
using HutongGames.PlayMaker;

namespace SatsumaTurboCharger
{
    public static class Helper
    {
        public static AssetBundle LoadAssetBundle(Mod mod, string fileName, Logger logger)
        {
            try
            {
                return LoadAssets.LoadBundle(mod, fileName);
            }
            catch (Exception ex)
            {
                string message = String.Format("AssetBundle file '{0}' could not be loaded", fileName);
                logger.New(
                    message,
                    String.Format("Check: {0}", Path.Combine(ModLoader.GetModAssetsFolder(mod), fileName)),
                    ex);
                ModConsole.Error(message);
                ModUI.ShowYesNoMessage(message + "\n\nClose Game? - RECOMMENDED", delegate ()
                {
                    Application.Quit();
                });
            }
            return null;
        }
        public static bool DetectRaycastHitObject(GameObject gameObjectToDetect, string layermask = "Parts", float distance = 0.8f)
        {
            RaycastHit hit;
            if (Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, distance, 1 << LayerMask.NameToLayer(layermask)) != false)
            {
                GameObject gameObjectHit;
                gameObjectHit = hit.collider?.gameObject;
                return (gameObjectHit != null && hit.collider && gameObjectHit == gameObjectToDetect);
            }
            return false;
        }
        public static bool PlayerInCar()
        {
            return FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle").Value == "Satsuma";
        }
        public static ScrewablePart[] GetScrewablePartsArrayFromPartsList(List<SimplePart> partsList)
        {
            List<ScrewablePart> screwableParts = new List<ScrewablePart>();

            partsList.ForEach(delegate (SimplePart part)
            {
                if (part.screwablePart != null)
                {
                    screwableParts.Add(part.screwablePart);
                }
            });
            return screwableParts.ToArray();
        }
        public static GameObject LoadPartAndSetName(AssetBundle assetsBundle, string prefabName, string name)
        {
            GameObject gameObject = assetsBundle.LoadAsset(prefabName) as GameObject;
            return SetObjectNameTagLayer(gameObject, name);
        }
        public static bool ThrottleButtonDown
        {
            get { return cInput.GetKey("Throttle"); }
        }
        public static bool LeftMouseDown
        {
            get { return Input.GetMouseButtonDown(0); }
        }
        public static bool RightMouseDown
        {
            get { return Input.GetMouseButtonDown(1); }
        }
        public static bool UseButtonDown
        {
            get { return cInput.GetKeyDown("Use"); }
        }
        public static T LoadSaveOrReturnNew<T>(Mod mod, string savefileName) where T : new()
        {
            string path = Path.Combine(ModLoader.GetModConfigFolder(mod), savefileName);
            if (File.Exists(path))
            {
                string serializedData = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(serializedData);
            }
            return new T();
        }
        public static bool CheckCloseToPosition(Vector3 positionOfPartTocheck, Vector3 position, float minimumDistance)
        {
            try
            {
                if (Vector3.Distance(positionOfPartTocheck, position) <= minimumDistance)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }

        }
        public static void WorkAroundAction() { }
        public static GameObject SetObjectNameTagLayer(GameObject gameObject, string name)
        {
            gameObject.name = name;
            gameObject.tag = "PART";

            gameObject.layer = LayerMask.NameToLayer("Parts");
            return gameObject;
        }
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }
        public static string CreatePathIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
               Directory.CreateDirectory(path);
            }
            return path;
        }

        public static Sprite LoadNewSprite(Sprite sprite, string FilePath, float pivotX = 0.5f, float pivotY = 0.5f, float PixelsPerUnit = 100.0f)
        {
            if (File.Exists(FilePath) && Path.GetExtension(FilePath) == ".png")
            {
                Sprite NewSprite = new Sprite();
                Texture2D SpriteTexture = LoadTexture(FilePath);
                NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(pivotX, pivotY), PixelsPerUnit);

                return NewSprite;
            }
            return sprite;
        }
        public static Texture2D LoadTexture(string FilePath)
        {
            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(FileData))
                    return Tex2D;
            }
            return null;
        }

        public static bool ApproximatelyVector(Vector3 me, Vector3 other, float allowedDifference = 0)
        {
            var dx = me.x - other.x;
            if (Mathf.Abs(dx) > allowedDifference)
                return false;

            var dy = me.y - other.y;
            if (Mathf.Abs(dy) > allowedDifference)
                return false;

            var dz = me.z - other.z;

            return Mathf.Abs(dz) >= allowedDifference;
        }

        public static bool ApproximatelyQuaternion(this Quaternion quatA, Quaternion value, float acceptableRange = 0)
        {
            return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange;
        }
    }
}
