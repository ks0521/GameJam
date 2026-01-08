using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.U2D.Sprites;

public static class BatchSpriteSlicer2022
{
    // 메뉴: Tools > Sprite > Batch Slice Selected (Grid)
    
    [MenuItem("Tools/Sprite/Batch Slice Selected (Grid)")]
    private static void SliceSelected()
    {
        // === 설정값: 너 타일에 맞게 여기만 바꾸면 됨 ===
        const int cellSize = 16;    // 16 또는 32
        const int offsetX = 0;
        const int offsetY = 0;
        const int paddingX = 0;
        const int paddingY = 0;
        const float pixelsPerUnit = 16f; // 타일 16px면 16, 32px면 32

        Object[] selected = Selection.objects;
        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("선택된 에셋이 없습니다. PNG 텍스처들을 선택하세요.");
            return;
        }

        int processed = 0;

        foreach (var obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) continue; // 텍스처가 아니면 스킵

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            // 1) 임포트 기본 설정
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            // 먼저 Reimport 해서 설정 반영(중요)
            importer.SaveAndReimport();

            // 2) Sprite Editor Data Provider로 그리드 슬라이스
            var factory = new SpriteDataProviderFactories();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            int texW = tex.width;
            int texH = tex.height;

            var spriteRects = new List<SpriteRect>();

            int xStart = offsetX;
            int yStart = offsetY;

            // Unity Sprite Rect는 좌하단 기준 좌표계를 씀
            // 아래는 (왼쪽 아래)부터 위로 쌓는 방식
            for (int y = yStart; y + cellSize <= texH; y += (cellSize + paddingY))
            {
                for (int x = xStart; x + cellSize <= texW; x += (cellSize + paddingX))
                {
                    var rect = new Rect(x, y, cellSize, cellSize);

                    var sr = new SpriteRect()
                    {
                        rect = rect,
                        name = $"{tex.name}_{x}_{y}",
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };

                    spriteRects.Add(sr);
                }
            }

            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply(); // 적용!

            // 마지막으로 저장/리임포트
            importer.SaveAndReimport();

            processed++;
            Debug.Log($"[SLICED] {path} ({spriteRects.Count} sprites)");
        }

        Debug.Log($"Batch Slice Done. Processed: {processed}");
    }
}