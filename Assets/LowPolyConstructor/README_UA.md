# Low Poly Constructor

## Почати

Імпортувати актуальний `.unitypackage` через **Assets → Import Package → Custom Package**, або скопіювати всю папку `LowPolyConstructor` у `Assets`. Зберігати всі `.meta`: від них залежать посилання між prefab, моделями й матеріалами.

Перевірено у Unity 6000.3.15f1 з URP 17.3.0. Матеріали — URP/Lit; для Built-in/HDRP потрібне їх налаштування під відповідний pipeline. Blender для використання FBX не потрібний.

## Склад

| Категорія | FBX |
|---|---:|
| 01_Primitives — Примітиви | 50 |
| 02_Architecture — Архітектура | 24 |
| 03_Terrain — Рельєф | 7 |
| 04_Nature — Природа | 34 |
| 05_Props — Предмети й механізми | 42 |
| 06_Textured — Текстурні варіанти | 12 |
| 07_Optimized — Meshes трьох готових дерев із LOD | 9 |
| 08_Water — Модулі води | 19 |

Разом **176 базових деталей, 12 текстурних варіантів та 9 meshes LOD**. У папці 197 FBX, 241 prefab, 11 сцен; 43 модульних складань.

Наочний каталог: `Documentation/Catalog/index.html`. Машинний каталог: `CATALOG.csv`.

## Демонстраційні сцени

- `Constructor_Catalog_All`
- `Constructor_Complete_Presentation`
- `Constructor_Presentation_All`
- `Constructor_Stage1_Demo`
- `Constructor_Stage2_Demo`
- `Constructor_Stage3_Demo`
- `Constructor_Stage4_Demo`
- `Constructor_Stage4_Mechanisms`
- `Constructor_Stage5_Demo`
- `Constructor_Water_Nature_Demo`
- `Constructor_Waterfront_Demo`

## Складання

1 одиниця = 1 метр. Prefab деталей має масштаб `(1,1,1)`. У більшості форм pivot знизу; деталі з обертанням мають pivot на своїй осі. Точний опис є в каталозі та `STAGE*_MODULES.json`.

Дерева `Tree_Broad`, `Tree_Slim`, `Tree_Pine` у `Prefabs/Assemblies` складаються з 11, 9 та 31 незалежної частини. Розгорни дерево в Hierarchy: стовбур, гілки, розвилки, групи листя й хвоя редагуються окремо. `Socket_tip`, `Socket_left`, `Socket_right` показують місця приєднання. Координати та радіуси кінців — у `SOCKETS.json`. Для нового складання звіряй товщину дочірньої гілки з батьківським кінцем; невелике заглиблення гілки в листя навмисне.

Готові `GR_Tree_*` у `07_Optimized` мають об’єднану геометрію та LODGroup для розстановки дерев. Правила й фактичні бюджети геометрії — у `README_STAGE5_UA.md`.

Будівництво: сітка 0,5 м, типовий прогін стіни 2 м. Рельєф: плитка 2 × 2 м. Огорожа: крок 2 м, спільний стовп на стику. Двері, ворота, кришки, колеса, ручки та дужки залишаються окремими деталями. Інструкції для цих частин — у `README_STAGE2_UA.md`, `README_STAGE3_UA.md`, `README_STAGE4_UA.md`.

## Матеріали та межі набору

Основні матеріали: Wood, Bark, Leaves, Stone, Ground, Metal. Вони використовують PNG-палітри для кольору граней. `TX_*` мають додаткові карти поверхні. Усі стилізовані low-poly деталі з пласкими гранями; підвищувати subdivision для використання не потрібно.

Це бібліотека статичних моделей та прикладів складання. Ігровий контролер, фізика, готові колайдери, анімації й скрипти відкривання не входять до набору. LOD є тільки у трьох `GR_Tree_*`.

Геометрію перевірено на відкриті та вивернуті оболонки, вироджені грані й коректність UV; після імпорту звірено розміри, трикутники та матеріали. Демо оглянуті в живому Unity Editor. FPS і збірку окремої гри не вимірювали.

Міст, пристань та драбина: `README_WATERFRONT_UA.md` і `WATERFRONT_MODULES.json`.

Вода з рухом, озерні кола, нова природа та комахи: `README_WATER_NATURE_UA.md`.
