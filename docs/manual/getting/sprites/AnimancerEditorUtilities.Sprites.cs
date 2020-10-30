// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerEditorUtilities
    /// 
    partial class AnimancerEditorUtilities
    {
        /************************************************************************************************************************/

        [MenuItem("CONTEXT/TextureImporter/Generate Animations By Sprite Name")]
        private static void GenerateAnimationsBySpriteName(MenuCommand command)
        {
            var importer = (TextureImporter)command.context;
            var assets = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);
            var name = Path.GetFileNameWithoutExtension(importer.assetPath).Replace(" ", "");

            var pathPrefix = Path.GetDirectoryName(importer.assetPath) + "/";
            pathPrefix += name + "-";

            var nameToSprites = new Dictionary<string, List<Sprite>>();

            Array.Sort(assets, (a, b) => a.name.CompareTo(b.name));
            for (int i = 0; i < assets.Length; i++)
            {
                var sprite = assets[i] as Sprite;
                if (sprite == null)
                    continue;

                // Remove spaces and numbers.
                var baseName = Regex.Replace(sprite.name, @"[ \d-]", "");

                if (!nameToSprites.TryGetValue(baseName, out var sprites))
                {
                    sprites = new List<Sprite>();
                    nameToSprites.Add(baseName, sprites);
                }

                sprites.Add(sprite);
            }

            foreach (var sprites in nameToSprites)
            {
                var animationName = sprites.Key;
                if (animationName.StartsWith(name))
                    animationName = animationName.Substring(name.Length, animationName.Length - name.Length);
                animationName = pathPrefix + animationName + ".anim";

                CreateAnimation(animationName, sprites.Value.Count, sprites.Value.ToArray());
            }

            AssetDatabase.SaveAssets();
        }

        /************************************************************************************************************************/

        private static void CreateAnimation(string path, int frameRate, params Sprite[] sprites)
        {
            var clip = new AnimationClip
            {
                frameRate = frameRate,
            };

            var spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite",
            };

            var spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];

            for (int i = 0; i < sprites.Length; i++)
            {
                spriteKeyFrames[i] = new ObjectReferenceKeyframe
                {
                    time = i / (float)frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);

            AssetDatabase.CreateAsset(clip, path);
        }

        /************************************************************************************************************************/
    }
}

#endif