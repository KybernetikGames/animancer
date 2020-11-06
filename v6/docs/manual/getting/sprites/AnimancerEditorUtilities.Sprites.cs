// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerEditorUtilities
    /// 
    partial class AnimancerEditorUtilities
    {
        /************************************************************************************************************************/

        private const string GenerateAnimationsBySpriteNameFunctionName = "/Generate Animations By Sprite Name";

        /************************************************************************************************************************/

        [MenuItem("Assets/Create/Animancer" + GenerateAnimationsBySpriteNameFunctionName, validate = true)]
        private static bool ValidateGenerateAnimationsBySpriteName()
        {
            var selection = Selection.objects;
            for (int i = 0; i < selection.Length; i++)
            {
                var selected = selection[i];
                if (selected is Sprite || selected is Texture)
                    return true;
            }

            return false;
        }

        [MenuItem("Assets/Create/Animancer" + GenerateAnimationsBySpriteNameFunctionName, priority = Strings.AssetMenuOrder + 13)]
        private static void GenerateAnimationsBySpriteName()
        {
            var sprites = new List<Sprite>();

            var selection = Selection.objects;
            for (int i = 0; i < selection.Length; i++)
            {
                var selected = selection[i];
                if (selected is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
                else if (selected is Texture)
                {
                    var path = AssetDatabase.GetAssetPath(selected);
                    var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    for (int j = 0; j < assets.Length; j++)
                    {
                        sprite = assets[j] as Sprite;
                        if (sprite != null)
                            sprites.Add(sprite);
                    }
                }
            }

            GenerateAnimationsBySpriteName(sprites);
        }

        /************************************************************************************************************************/

        private static List<Sprite> _CachedSprites;

        /************************************************************************************************************************/

        [MenuItem("CONTEXT/" + nameof(Sprite) + GenerateAnimationsBySpriteNameFunctionName)]
        private static void GenerateAnimationsFromSpriteByName(MenuCommand command)
        {
            // Delay the call in case multiple objects are selected.
            if (_CachedSprites == null)
            {
                _CachedSprites = new List<Sprite>();
                EditorApplication.delayCall += () =>
                {
                    GenerateAnimationsBySpriteName(_CachedSprites);
                    _CachedSprites = null;
                };
            }

            _CachedSprites.Add((Sprite)command.context);
        }

        /************************************************************************************************************************/

        [MenuItem("CONTEXT/" + nameof(TextureImporter) + GenerateAnimationsBySpriteNameFunctionName, validate = true)]
        private static bool ValidateGenerateAnimationsFromTextureBySpriteName(MenuCommand command)
        {
            var importer = (TextureImporter)command.context;
            var assets = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite)
                    return true;
            }

            return false;
        }

        [MenuItem("CONTEXT/" + nameof(TextureImporter) + GenerateAnimationsBySpriteNameFunctionName)]
        private static void GenerateAnimationsFromTextureBySpriteName(MenuCommand command)
        {
            // Delay the call in case multiple objects are selected.
            if (_CachedSprites == null)
            {
                _CachedSprites = new List<Sprite>();
                EditorApplication.delayCall += () =>
                {
                    GenerateAnimationsBySpriteName(_CachedSprites);
                    _CachedSprites = null;
                };
            }

            var importer = (TextureImporter)command.context;
            var assets = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                    _CachedSprites.Add(sprite);
            }
        }

        /************************************************************************************************************************/

        private static void GenerateAnimationsBySpriteName(List<Sprite> sprites)
        {
            if (sprites.Count == 0)
                return;

            sprites.Sort((a, b) => EditorUtility.NaturalCompare(a.name, b.name));

            var nameToSprites = new Dictionary<string, List<Sprite>>();

            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];

                // Remove numbers from the end.
                var baseName = sprite.name.TrimEnd(' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                //Regex.Replace(sprite.name, @"\d+$", "");

                if (!nameToSprites.TryGetValue(baseName, out var spriteGroup))
                {
                    spriteGroup = new List<Sprite>();
                    nameToSprites.Add(baseName, spriteGroup);
                }

                // Add the sprite to the group if it's not a duplicate.
                if (spriteGroup.Count == 0 || spriteGroup[spriteGroup.Count - 1] != sprite)
                    spriteGroup.Add(sprite);
            }

            var pathToSprites = new Dictionary<string, List<Sprite>>();

            var message = new StringBuilder()
                .Append("Do you wish to generate the following animations?");
            var line = 0;
            foreach (var nameToSpriteGroup in nameToSprites)
            {
                var path = AssetDatabase.GetAssetPath(nameToSpriteGroup.Value[0]);
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, nameToSpriteGroup.Key + ".anim");
                pathToSprites.Add(path, nameToSpriteGroup.Value);

                if (++line < 30)
                    message.AppendLine()
                        .Append("- ")
                        .Append(path)
                        .Append(" (")
                        .Append(nameToSpriteGroup.Value.Count)
                        .Append(" frames)");
            }

            if (!EditorUtility.DisplayDialog("Generate Sprite Animations?", message.ToString(), "Generate", "Cancel"))
                return;

            foreach (var pathToSpriteGroup in pathToSprites)
                CreateAnimation(pathToSpriteGroup.Key, pathToSpriteGroup.Value.Count, pathToSpriteGroup.Value.ToArray());

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