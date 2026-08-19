using HarmonyLib;
using JaLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BugFixPack
{
    public class BugFixPack : Mod
    {
        public override string ModID => "com.deepteam.bugfixpack";
        public override string ModName => "BugFix Pack";
        public override string ModAuthor => "Deep Team";
        public override string ModDescription => "Исправляет спам логов. Дебаггер для поиска бага с корзинами. Патчи корзин, 1х2х1,";
        public override string ModVersion => "1.0.0";

        private static Harmony harmony = new Harmony("com.deepteam.bugfixpack");

         public override void OnEnable()
        {
            base.OnEnable();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            JaLoader.Console.Log("BugFixPack", "Loaded and patched (debug mode).");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            harmony.UnpatchSelf();
        }
    }

    // ============================================================
    // 1. ФИЛЬТР ЛОГОВ (убираем MeshCollider спам)
    // ============================================================

    [HarmonyPatch(typeof(Debug))]
    public static class Patch_DebugLogFilter
    {
        [HarmonyPrefix]
        [HarmonyPatch("Log", new Type[] { typeof(object) })]
        public static bool Prefix_Log(object message)
        {
            string msg = message?.ToString() ?? "";
            if (msg.Contains("Non-convex MeshCollider")) return false;
            return true;
        }

        [HarmonyPrefix]
         [HarmonyPatch("LogWarning", new Type[] { typeof(object) })]
        public static bool Prefix_LogWarning(object message)
        {
            string msg = message?.ToString() ?? "";
            if (msg.Contains("Non-convex MeshCollider")) return false;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("LogError", new Type[] { typeof(object) })]
        public static bool Prefix_LogError(object message)
        {
            string msg = message?.ToString() ?? "";
            if (msg.Contains("Non-convex MeshCollider")) return false;
            return true;
        }
    }

    // ============================================================
    // 2. ДЕБАГГЕР: логирование всех методов InventoryLogicC и DragRigidbodyC
    // ============================================================

    [HarmonyPatch]
    public static class Debug_InventoryCalls
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var types = new[] { typeof(InventoryLogicC), typeof(DragRigidbodyC) };
            foreach (var type in types)
             {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var m in methods)
                {
                    if (m.Name.StartsWith("Place") || m.Name.StartsWith("Remove") || m.Name.StartsWith("Take") || m.Name.StartsWith("Clear") || m.Name.StartsWith("Move"))
                        yield return m;
                }
            }
        }

        [HarmonyPrefix]
        public static void Prefix(MethodBase __originalMethod, object[] __args)
        {
            string argsStr = "null";
            if (__args != null && __args.Length > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < __args.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(__args[i]?.ToString() ?? "null");
                }
                argsStr = sb.ToString();
             }
            JaLoader.Console.Log("DebugInv", $"CALL: {__originalMethod.Name}({argsStr})");
        }
    }

  

    // ============================================================
    // ПАТЧ: синхронная инициализация findNextSlot и вызов PlaceNext1x2x1ObjectIntoInventory

    [HarmonyPatch(typeof(InventoryLogicC), "Place1x2x1ObjectIntoInventory")]
    public static class Patch_Place1x2x1ObjectIntoInventory_Fix
    {
        [HarmonyPrefix]
        public static bool Prefix(InventoryLogicC __instance, object[] __args)
        {
            // Если метод вызван с параметром (не null) — пропускаем, не мешаем игре
            if (__args != null && __args.Length > 0 && __args[0] != null)
            {
                return true; // выполняем оригинальный метод
            }
            {
                try
                {
                    // Проверяем, вызван ли метод без параметра (или с null)
                     // Мы не можем проверить параметры напрямую
                    // Поэтому мы всегда будем перехватывать и перенаправлять

                    // 1. Найти свободный слот с spaceAbove
                    Transform foundSlot = null;
                    Transform foundSpaceAbove = null;

                    for (int i = 0; i < __instance.inventorySlots.Length; i++)
                    {
                        var slot = __instance.inventorySlots[i];
                        var relay = slot.GetComponent<InventoryRelayC>();
                        if (!relay.isOccupied && relay.spaceAbove != null && !relay.spaceAbove.GetComponent<InventoryRelayC>().isOccupied)
                        {
                            foundSlot = slot;
                            foundSpaceAbove = relay.spaceAbove.transform;
                            break;
                        }
                    }

                    if (foundSlot == null)
                    {
                        // Если слот не найден, показываем ошибку и выходим
                        var drag = UnityEngine.Object.FindObjectOfType<DragRigidbodyC>();
                        if (drag != null) drag.PickUpError();
                        return false;
                    }

                    // 2. Устанавливаем nextInventorySlot и findNextSlot
                    var nextSlotField = typeof(InventoryLogicC).GetField("nextInventorySlot", BindingFlags.NonPublic | BindingFlags.Instance);
                    var findSlotField = typeof(InventoryLogicC).GetField("findNextSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                     if (nextSlotField != null) nextSlotField.SetValue(__instance, foundSlot);
                    if (findSlotField != null) findSlotField.SetValue(__instance, foundSlot);

                    // 3. Вызываем PlaceNext1x2x1ObjectIntoInventory через рефлексию
                    var method = typeof(InventoryLogicC).GetMethod("PlaceNext1x2x1ObjectIntoInventory", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(__instance, null);
                        JaLoader.Console.Log("BugFixPack", "[Fix] PlaceNext1x2x1ObjectIntoInventory called successfully.");
                        return false; // отменяем оригинальный вызов
                    }
                    else
                    {
                        JaLoader.Console.LogWarning("BugFixPack", "[Fix] PlaceNext1x2x1ObjectIntoInventory not found.");
                    }
                }
                catch (Exception e)
                {
                    JaLoader.Console.LogError("BugFixPack", $"[Fix] Error: {e}");
                }
                return false; // если что-то пошло не так, вызываем оригинал
            }
        }
        // ============================================================
        // ПАТЧ: блокировка вызова Place1x2x1ObjectIntoInventory через SendMessage

        [HarmonyPatch(typeof(GameObject), "SendMessage", new Type[] { typeof(string), typeof(object), typeof(SendMessageOptions) })]
        public static class Patch_SendMessage_Place1x2x1
        {
            [HarmonyPrefix]
            public static bool Prefix(GameObject __instance, string methodName, object value)
            {
                if (methodName == "Place1x2x1ObjectIntoInventory" && value == null)
                {
                    JaLoader.Console.Log("BugFixPack", "[Redirect] SendMessage Place1x2x1ObjectIntoInventory with null parameter. Redirecting to PlaceNext...");

                    // Находим InventoryLogicC на объекте
                    var inventory = __instance.GetComponent<InventoryLogicC>();
                    if (inventory != null)
                    {
                        // Ищем и вызываем PlaceNext1x2x1ObjectIntoInventory
                        var method = typeof(InventoryLogicC).GetMethod("PlaceNext1x2x1ObjectIntoInventory", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (method != null)
                        {
                            method.Invoke(inventory, null);
                            JaLoader.Console.Log("BugFixPack", "[Redirect] PlaceNext1x2x1ObjectIntoInventory called.");
                            return false; // отменяем оригинальный SendMessage
                        }
                    }
                }
                 return true;
            }
        }
        // ============================================================
        // ПАТЧ: сброс состояния корзин через InventoryRelayC.UnOccupy()

        [HarmonyPatch(typeof(InventoryLogicC), "UpdateInventory")]
        public static class Patch_UpdateInventory_Basket
        {
            [HarmonyPostfix]
            public static void Postfix(InventoryLogicC __instance)
            {
                // Находим все InventoryRelayC, которые принадлежат корзинам
                var relays = UnityEngine.Object.FindObjectsOfType<InventoryRelayC>();
                foreach (var relay in relays)
                {
                    // Проверяем, является ли родительский объект корзиной
                    if (relay.transform.parent != null && relay.transform.parent.name.Contains("Basket"))
                    {
                        // Если слот пуст, сбрасываем isOccupied
                        if (relay.transform.childCount == 0)
                        {
                             relay.isOccupied = false;
                        }
                    }
                }
            }
        }


        // ============================================================
        // ПАТЧ: защита ObjectPickupC.Update от NullReferenceException
        // ============================================================

        [HarmonyPatch(typeof(ObjectPickupC), "Update")]
        public static class Patch_ObjectPickupC_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(ObjectPickupC __instance)
            {
                // Проверяем, что объект существует
                if (__instance == null || __instance.gameObject == null || __instance.transform == null)
                    return false;

                // Проверяем, что объект активен
                if (!__instance.gameObject.activeInHierarchy)
                    return false;

                // Проверяем, что renderTargets существует и все его элементы не null
                if (__instance.renderTargets != null)
                {
                    foreach (var rt in __instance.renderTargets)
                        if (rt == null) return false;
                }

                // Проверяем, что transform.parent существует (используется в методе)
                if (__instance.transform.parent == null)
                    return false;

                // Если все проверки пройдены, выполняем оригинальный Update
                return true;
             }
        }

    }
}