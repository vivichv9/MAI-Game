# MAI-Game

Unity-прототип 3D-головоломки с механикой эхо-позиций.

Игрок должен проходить уровень, создавая ограниченное число статичных эхо и выполняя swap: игрок и выбранное эхо меняются позициями, а эхо остается в мире и может использоваться повторно.

## Текущий статус

Этап 1 базовой архитектуры и конфигов завершен.

Готово:

- добавлена базовая структура Unity-проекта;
- подключен Unity Input System через `Packages/manifest.json`;
- включена текстовая сериализация Unity-ассетов;
- созданы стартовые сцены:
  - `Assets/Scenes/Bootstrap/Bootstrap.unity`;
  - `Assets/Scenes/Levels/Test_Playground.unity`;
  - `Assets/Scenes/Levels/Level_01.unity`;
- добавлен editor-only build runner для Windows-сборки;
- создан базовый каркас `GameBootstrap`, `GameManager`, `LevelManager`;
- добавлен интерфейс `IResettableLevelObject`;
- добавлены ScriptableObject-конфиги `PlayerConfig`, `EchoConfig`, `LevelGoalConfig`;
- выполнена успешная Windows-сборка этапа 1.

## Требования

- Unity 2022.3 LTS или совместимая LTS-версия.
- Windows PC для целевой сборки прототипа.

Локально используемый редактор:

```text
D:\Unity\2022.3.53f1\Editor\Unity.exe
```

Также установлен Unity Hub:

```text
C:\Program Files\Unity Hub\Unity Hub.exe
```

## Открытие проекта

Откройте корень репозитория как Unity-проект:

```text
D:\Users\Кирилл\Documents\MAI-Game
```

При первом открытии Unity создаст служебные папки `Library/`, `Temp/` и сгенерирует недостающие `.meta` файлы.

## Сборка

Когда проект открыт в Unity, Windows-сборку можно запустить через меню:

```text
MAI Game > Build > Windows
```

Batchmode-команда для автоматической проверки:

```powershell
& "D:\Unity\2022.3.53f1\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Users\Кирилл\Documents\MAI-Game" -executeMethod MAIGame.EditorTools.BuildPipelineRunner.BuildWindows -logFile "D:\Users\Кирилл\Documents\MAI-Game\Logs\unity-build.log"
```

Ожидаемый результат:

```text
Builds/Windows/EchoPuzzleGame.exe
```

Если batchmode сообщает `No valid Unity Editor license found`, откройте Unity Hub, войдите в Unity ID и активируйте Personal/Free license.

Важно: `2022.3.75f1` относится к Extended LTS и требует Unity Industry/Enterprise. Для разработки этого проекта используется `2022.3.53f1`, совместимая с обычной 2022.3 LTS-веткой.

## Документация разработки

- `TZ_Unity_Echo_Puzzle_Game.md` - полное техническое задание.
- `AGENTS.md` - правила разработки и архитектурные ограничения.
- `PLANS.md` - пошаговый план работ.
