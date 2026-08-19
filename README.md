# BugFix Pack for Jalopy
[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://github.com/Dima6543/FixPackJalopy/releases)
[![JaLoader](https://img.shields.io/badge/JaLoader-5.1.4+-brightgreen.svg)](https://github.com/theLeaxx/JaLoader)

## Description

BugFix Pack is a collection of fixes for critical and annoying bugs in Jalopy that were never addressed by the developers. The mod works through JaLoader and does not modify game files directly.

### What it fixes

1. **Wine in the trunk (1×2×1 item bug)** – When trying to place a 1×2×1 item (e.g., wine) into the trunk (regular or roof), the game could break the trunk, making it unusable for new items. The fix redirects the `Place1x2x1ObjectIntoInventory` call via `SendMessage` to the working method `PlaceNext1x2x1ObjectIntoInventory`.

2. **Baskets at petrol stations** – After removing items from a basket, it remained marked as "occupied" (`isOccupied = true`). The patch resets this flag for empty slots.

3. **Empty boxes** – A rare bug where an empty box caused a cyclic `NullReferenceException` in `ObjectPickupC.Update`, dropping FPS to 3–5. The protection disables `Update` for invalid objects.

4. **Log spam** – Removed messages about `Non-convex MeshCollider` that do not affect gameplay but clutter the console and logs.

> **Important:** For stable operation of the wine fix, technical messages (`JaLoader.Console.Log`) may appear in the logs. This creates a micro‑delay necessary for proper slot initialisation. If you disable console output, the bug may return. We recommend keeping the console enabled
---

## Installation

1. Ensure you have **JaLoader** version 5.1.4 or higher installed.
2. Download the latest release `BugFixPackJalopy.dll` from the [Releases](https://github.com/Dima6543/FixPackJalopy/releases) page.
3. Copy the file to the `Mods` folder (usually `C:\Users\[YourName]\Documents\Jalopy\Mods`).
4. Launch the game via JaLoader.

---

## Notes

- The mod is currently in testing mode. If you encounter new bugs or unexpected behaviour, please create an Issue in the repository.
- Further support will be provided manually, as all main dependencies have already been identified and fixed.
- I’d be glad if you could support the project financially — this will help me create more mods and maintain the existing ones: https://www.donationalerts.com/r/pkashnikq

---

## Acknowledgements

- **theLeaxx** – for creating **[JaLoader](https://github.com/theLeaxx/JaLoader)**, without which this mod would not be possible.
- Everyone who tested and reported bugs.

---

## License

This project is distributed under the MIT License. See the `LICENSE` file for details.

---

## Русская версия

### Описание

BugFix Pack — набор исправлений для критических и раздражающих багов в Jalopy, которые разработчики не починили. Мод работает через JaLoader и не изменяет файлы игры напрямую.

### Что исправляет

1. **Баг с вином в багажнике (1×2×1)** – при попытке положить предмет 1×2×1 (например, вино) в багажник (обычный или на крыше) игра могла сломать багажник, делая его недоступным для новых предметов. Фикс перенаправляет вызов `Place1x2x1ObjectIntoInventory` через `SendMessage` на рабочий метод `PlaceNext1x2x1ObjectIntoInventory`.

2. **Корзины на заправках** – после извлечения предметов корзины оставались помеченными как «занятые» (`isOccupied = true`). Патч сбрасывает этот флаг для пустых слотов.

3. **Пустые коробки** – редкий баг, когда пустая коробка вызывала циклическую ошибку `NullReferenceException` в `ObjectPickupC.Update`, что приводило к падению FPS до 3–5. Защита отключает `Update` для невалидных объектов.

4. **Спам логов** – убраны сообщения о `Non-convex MeshCollider`, которые не влияют на геймплей, но засоряют консоль и логи.

> **Важно:** для стабильной работы фикса бага с вином в логах могут появляться технические сообщения (`JaLoader.Console.Log`). Это создаёт микро-задержку, необходимую для корректной инициализации слотов. Если выключить вывод в консоль, баг может вернуться. Мы рекомендуем оставить консоль включённой

### Установка

1. Убедитесь, что у вас установлен **JaLoader** версии 5.1.4 или выше.
2. Скачайте последний релиз `BugFixPackJalopy.dll` из раздела [Releases](https://github.com/Dima6543/FixPackJalopy/releases).
3. Скопируйте файл в папку `Mods` (обычно `C:\Users\[Имя]\Documents\Jalopy\Mods`).
4. Запустите игру через JaLoader.


### Примечания

- Мод находится в режиме тестирования. Если вы столкнётесь с новыми багами или нестандартным поведением, создайте Issue в репозитории.
- Дальнейшая поддержка будет осуществляться вручную, так как все основные зависимости уже выявлены и зафиксированы.
- Буду рад, если поддержите проект финансово, так вы поможете мне создавать больше модов и поддерживать существующие https://www.donationalerts.com/r/pkashnikq

### Благодарности

- **theLeaxx** – за создание **[JaLoader](https://github.com/theLeaxx/JaLoader)**, без которого этот мод был бы невозможен.
- Всем, кто тестировал и сообщал о багах. В основном моим друзьям :)

### Лицензия

Проект распространяется под лицензией MIT. Подробнее в файле `LICENSE`.
