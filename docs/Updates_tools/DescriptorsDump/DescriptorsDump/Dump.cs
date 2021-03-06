﻿using System;
using System.Diagnostics;
using System.Globalization;

namespace DescriptorsDump
{
    static class Dump
    {
        static Magic.BlackMagic _memory;

        internal static string Go(string prefixEnum = "", bool multiply = true, bool upper = true, bool remLocal = true)
        {
            try
            {
                var p = Process.GetProcessesByName("WoW");
                if (p.Length <= 0)
                    p = Process.GetProcessesByName("WowT");
                if (p.Length <= 0)
                    p = Process.GetProcessesByName("WowB");
                if (p.Length <= 0)
                    return "Wow process not found.";
                _memory = new Magic.BlackMagic(p[0].Id);
                if (!_memory.IsProcessOpen)
                    return "Process not open.";

                // Check function
                /* 2F8F51 */
                uint dwStartFunc = _memory.FindPattern("53 56 57 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 E8 00 00 00 00 6A 00 5B 53 6A",
                    "xxxx????x????x????x????x????x????x????x????x????x????x????x????x????x????x????x????x?xxx");
                if (dwStartFunc <= 0)
                {
                    _memory.Close();
                    return "Descriptors function not found.";
                }

                /*uint baseObjectDescriptorsPointer = _memory.FindPattern("8B 4D FC 33 C0 40 C7 81", "xxxxxxxx");
                if (baseObjectDescriptorsPointer <= 0)
                {
                    _memory.Close();
                    return "Base_CGObjectData not found.";
                }*/

                // Get base adresse of all Field.
                uint s_objectDescriptors, s_unitDescriptors, s_itemDescriptors, s_playerDescriptors, s_containerDescriptors, s_gameobjectDescriptors, s_dynamicObjectDescriptors, s_corpseDescriptors, s_areaTriggerDescriptors, s_sceneObjectDescriptors, s_itemDynamicData, s_unitDynamicData, s_playerDynamicData, s_conversationData, s_conversationDynamicData;
                s_objectDescriptors = _memory.ReadUInt(dwStartFunc + 0x53 + 1); //s_objectDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0x55);
                s_itemDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0x6B);
                s_containerDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0x81);
                s_unitDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0x99);
                s_playerDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0xB1);
                s_gameobjectDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0xCB);
                s_dynamicObjectDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0xE3);
                s_corpseDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0xF8);
                s_areaTriggerDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0x10D);
                s_sceneObjectDescriptors = _memory.ReadUInt(dwStartFunc + 0x1 + 0x122);
                s_itemDynamicData = _memory.ReadUInt(dwStartFunc + 0x1 + 0x162);
                s_unitDynamicData = _memory.ReadUInt(dwStartFunc + 0x1 + 0x17B);
                s_playerDynamicData = _memory.ReadUInt(dwStartFunc + 0x1 + 0x194);
                // WoD new:
                s_conversationData = _memory.ReadUInt(dwStartFunc + 0x1 + 0x137);
                s_conversationDynamicData = _memory.ReadUInt(dwStartFunc + 0x1 + 0x1CE);


                const int g_baseObjDescriptors = 0xEBEFF0; // CGObjectData
                s_objectDescriptors = g_baseObjDescriptors;

                const int g_baseItemDescriptors = 0xEBF080; // CGItemData
                s_itemDescriptors = g_baseItemDescriptors;

                const int g_baseContainerDescriptors = 0xEBFAD8; // CGContainerData
                s_containerDescriptors = g_baseContainerDescriptors;

                const int g_baseUnitDescriptors = 0xEBFAE8; // CGUnitData
                s_unitDescriptors = g_baseUnitDescriptors;

                const int g_basePlayerDescriptors = 0xEC0460; // CGPlayerData
                s_playerDescriptors = g_basePlayerDescriptors;

                const int g_baseGameObjectDescriptors = 0xECCF18; // CGGameObjectData
                s_gameobjectDescriptors = g_baseGameObjectDescriptors;

                const int g_baseDynamicObjectDescriptors = 0xECD020; // CGDynamicObjectData
                s_dynamicObjectDescriptors = g_baseDynamicObjectDescriptors;

                const int g_baseCorpseDescriptors = 0xECD090; // CGCorpseData
                s_corpseDescriptors = g_baseCorpseDescriptors;

                const int g_baseAreaTriggerDescriptors = 0xECD2D0; // CGAreaTriggerData
                s_areaTriggerDescriptors = g_baseAreaTriggerDescriptors;

                const int g_baseSceneObjectDescriptors = 0xECD3A0; // CGSceneObjectData
                s_sceneObjectDescriptors = g_baseSceneObjectDescriptors;

                const int g_baseConversationDescriptors = 0xEBF40C; // CGConversationData
                s_itemDynamicData = g_baseConversationDescriptors;

                const int g_baseItemDynamicDescriptors = 0xEBF3EC; // CGItemDynamicData
                s_unitDynamicData = g_baseItemDynamicDescriptors;

                const int g_baseUnitDynamicDescriptors = 0xEC0448; // CGUnitDynamicData
                s_playerDynamicData = g_baseUnitDynamicDescriptors;

                const int g_basePlayerDynamicDescriptors = 0xECCEB0; // CGPlayerDynamicData
                s_conversationData = g_basePlayerDynamicDescriptors;

                const int g_baseGameObjectDynamicDescriptors = 0xECD014; // CGGameObjectDynamicData
                s_conversationDynamicData = g_baseGameObjectDynamicDescriptors;


                const int g_baseConversationDynamicDescriptors = 0xECD3F4; // CGConversationDynamicData
                if (s_objectDescriptors <= 0 || s_itemDescriptors <= 0 || s_containerDescriptors <= 0 || s_unitDescriptors <= 0 || s_playerDescriptors <= 0 || s_gameobjectDescriptors <= 0 || s_dynamicObjectDescriptors <= 0 || s_corpseDescriptors <= 0 || s_areaTriggerDescriptors <= 0 || s_sceneObjectDescriptors <= 0 || s_itemDynamicData <= 0 || s_unitDynamicData <= 0 || s_playerDynamicData <= 0 || s_conversationData <= 0 || s_conversationDynamicData <= 0)
                {
                    _memory.Close();
                    return "Field not found.";
                }

                // Get Field
                string retVal = "";
                uint objectEndObjectFields;
                uint objectEndItemFields;
                uint objectEndContainerFields;
                uint objectEndUnitFields;
                uint objectEndPlayerFields;
                uint objectEndGameObjectFields;
                uint objectEndDynamicObjectFields;
                uint objectEndCorpseFields;
                uint objectEndAreaTriggerFields;
                uint objectEndSceneObjecFields, objectEndConversationData, objectEndItemDynamicFields, objectEndUnitDynamicFields, objectEndPlayerDynamicFields, objectEndConversationDynamicData;

                retVal = retVal + DumpField("ObjectFields", "CGObjectData::m_", s_objectDescriptors, 0, out objectEndObjectFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("ItemFields", "CGItemData::m_", s_itemDescriptors, objectEndObjectFields, out objectEndItemFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("ContainerFields", "CGContainerData::m_", s_containerDescriptors, objectEndItemFields, out objectEndContainerFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("UnitFields", "CGUnitData::", s_unitDescriptors, objectEndObjectFields, out objectEndUnitFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("PlayerFields", "CGPlayerData::", s_playerDescriptors, objectEndUnitFields, out objectEndPlayerFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("GameObjectFields", "CGGameObjectData::m_", s_gameobjectDescriptors, objectEndObjectFields, out objectEndGameObjectFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("DynamicObjectFields", "CGDynamicObjectData::m_", s_dynamicObjectDescriptors, objectEndObjectFields, out objectEndDynamicObjectFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("CorpseFields", "CGCorpseData::m_", s_corpseDescriptors, objectEndObjectFields, out objectEndCorpseFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("AreaTriggerFields", "CGAreaTriggerData::m_", s_areaTriggerDescriptors, objectEndObjectFields, out objectEndAreaTriggerFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("SceneObjectFields", "CGSceneObjectData::m_", s_sceneObjectDescriptors, objectEndObjectFields, out objectEndSceneObjecFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("ConversationData", "CGConversationData::", s_conversationData, objectEndObjectFields, out objectEndConversationData, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("ItemDynamicFields", "CGItemDynamicData::m_", s_itemDynamicData, objectEndObjectFields, out objectEndItemDynamicFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("UnitDynamicFields", "CGUnitDynamicData::m_", s_unitDynamicData, objectEndObjectFields, out objectEndUnitDynamicFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("PlayerDynamicFields", "CGPlayerDynamicData::m_", s_playerDynamicData, objectEndObjectFields, out objectEndPlayerDynamicFields, prefixEnum, multiply, upper, remLocal);
                retVal = retVal + DumpField("ConversationDynamicData", "CGConversationDynamicData::m_", s_conversationDynamicData, objectEndObjectFields, out objectEndConversationDynamicData, prefixEnum, multiply, upper, remLocal);
                
                // Dispose
                _memory.Close();
                // Return value
                return retVal;
            }
            catch (Exception e)
            {
                return e + Environment.NewLine + Environment.NewLine + " Exception caught.";
            }
        }

        static string DumpField(string szName, string sPrefix, uint dwPointer, uint lastIndex, out uint outLastIndex, string prefixEnum, bool multiply, bool upper, bool remLocal)
        {
            
            string valueReturn = "";

            valueReturn = valueReturn + "public enum " + prefixEnum + szName + Environment.NewLine + "{" + Environment.NewLine;
            string lastPszName = "";
            while (true)
            {
                DescriptorStruct descriptorStruct = (DescriptorStruct)_memory.ReadObject((uint)_memory.MainModule.BaseAddress+dwPointer, typeof(DescriptorStruct));
                // Get name:
                string pszName = _memory.ReadASCIIString(descriptorStruct.pName, 100);
                uint multiplyNum = 1;
                if (multiply)
                    multiplyNum = 4;

                if (string.IsNullOrEmpty(pszName))
                    break;

                if (pszName.Contains(sPrefix) && !pszName.Contains("["))
                {
                    pszName = pszName.Replace(sPrefix, "").Replace(".", "_");

                    if (remLocal)
                        pszName = pszName.Replace("local_", "");
                    if (upper)
                    {
                        var t = pszName.ToCharArray();
                        if (t.Length > 0)
                        {
                            t[0] = pszName[0].ToString(CultureInfo.InvariantCulture).ToUpper().ToCharArray()[0];
                            pszName = new string(t);
                        }
                    }

                    if (string.IsNullOrEmpty(lastPszName) || lastPszName != pszName)
                    {
                        lastPszName = pszName;

                        valueReturn = valueReturn + "   " + pszName + " = 0x" + ((lastIndex)*multiplyNum).ToString("X") +
                                      "," + Environment.NewLine;
                        lastIndex = lastIndex + descriptorStruct.size;
                    }
                }

                dwPointer = dwPointer + 0xC;
            }

            valueReturn = valueReturn + "};" + Environment.NewLine + Environment.NewLine;

            outLastIndex = lastIndex;

            return valueReturn;
        }

        private struct DescriptorStruct
        {
            public uint pName;
            public uint size;
            public short unknown;
        }
    }
}