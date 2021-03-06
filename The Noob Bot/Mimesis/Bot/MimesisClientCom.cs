﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using nManager.Wow.Class;
using nManager.Wow.Helpers;
using nManager.Helpful;
using nManager.Wow.ObjectManager;
using nManager.Wow.Enums;
using nManager.Wow.Bot.Tasks;

namespace Mimesis.Bot
{
    internal class MimesisClientCom
    {
        private static TcpClient client = null;
        private static IPEndPoint serviceEndPoint = null;
        public static List<MimesisHelpers.MimesisEvent> myTaskList = new List<MimesisHelpers.MimesisEvent>();
        public static List<MimesisHelpers.MimesisEvent> oldTaskList = new List<MimesisHelpers.MimesisEvent>();
        public static List<int> myQuestList = Quest.GetLogQuestId();
        private static uint RollId = 0;

        public static bool Connect()
        {
            Logging.Write("Connecting to " + MimesisSettings.CurrentSetting.MasterIPAddress + ":" + MimesisSettings.CurrentSetting.MasterIPPort + " ...");
            client = new TcpClient();

            if (serviceEndPoint == null)
                serviceEndPoint = new IPEndPoint(IPAddress.Parse(MimesisSettings.CurrentSetting.MasterIPAddress), MimesisSettings.CurrentSetting.MasterIPPort);
            try
            {
                client.Connect(serviceEndPoint);
                Logging.Write("Connected!");
                return true;
            }
            catch
            {
                Logging.Write("Could not connect to " + MimesisSettings.CurrentSetting.MasterIPAddress + ":" + MimesisSettings.CurrentSetting.MasterIPPort);
                return false;
            }
        }

        public static void Disconnect()
        {
            if (client == null)
                return;
            if (client.Connected)
            {
                try
                {
                    NetworkStream clientStream = client.GetStream();
                    byte[] opCodeAndSize = new byte[2];
                    opCodeAndSize[0] = (byte) MimesisHelpers.opCodes.Disconnect;
                    opCodeAndSize[1] = 0;
                    clientStream.Write(opCodeAndSize, 0, 2);
                    clientStream.Flush();
                }
                catch (System.IO.IOException)
                {
                }
            }
            Logging.Write("Disconnected from main bot.");
            EventsListener.UnHookEvent(WoWEventsType.START_LOOT_ROLL, callback => RollItem());
            client.Close();
        }

        public static UInt128 GetMasterGuid()
        {
            byte[] opCodeAndSize = new byte[2];
            byte[] buffer;
            opCodeAndSize[0] = (byte) MimesisHelpers.opCodes.QueryGuid;
            opCodeAndSize[1] = 0;

            NetworkStream clientStream = client.GetStream();
            clientStream.Write(opCodeAndSize, 0, 2);
            clientStream.Flush();

            // Now wait for an answer
            try
            {
                int bytesRead = clientStream.Read(opCodeAndSize, 0, 2);
                int len = opCodeAndSize[1];
                buffer = new byte[len];
                bytesRead += clientStream.Read(buffer, 0, len);
            }
            catch (Exception e)
            {
                Logging.WriteError("MimesisClientCom > GetMasterGuid(): " + e);
                return 0;
            }
            if ((MimesisHelpers.opCodes) opCodeAndSize[0] == MimesisHelpers.opCodes.ReplyGuid)
                return MimesisHelpers.BytesToStruct<UInt128>(buffer);
            return 0;
        }

        public static Point GetMasterPosition()
        {
            byte[] opCodeAndSize = new byte[2];
            byte[] buffer;
            opCodeAndSize[0] = (byte) MimesisHelpers.opCodes.QueryPosition;
            opCodeAndSize[1] = 0;
            NetworkStream clientStream = client.GetStream();
            clientStream.Write(opCodeAndSize, 0, 2);
            clientStream.Flush();

            // Now wait for an answer
            try
            {
                int bytesRead = clientStream.Read(opCodeAndSize, 0, 2);
                int len = opCodeAndSize[1]; // 3 float[4] + 1 byte (type)
                buffer = new byte[len];
                bytesRead += clientStream.Read(buffer, 0, len);
            }
            catch (Exception e)
            {
                Logging.WriteError("MimesisClientCom > GetMasterPosition(): " + e);
                return new Point();
            }
            if ((MimesisHelpers.opCodes) opCodeAndSize[0] == MimesisHelpers.opCodes.ReplyPosition)
            {
                if (buffer.Length != 13)
                    return new Point();
                var masterPositionXYZ = new float[3];
                Buffer.BlockCopy(buffer, 0, masterPositionXYZ, 0, 12);
                var masterPosition = new Point(masterPositionXYZ);

                switch (buffer[12])
                {
                    case 2:
                        masterPosition.Type = "Flying";
                        break;
                    case 1:
                        masterPosition.Type = "Swimming";
                        break;
                }
                return masterPosition;
            }
            return new Point();
        }

        public static void JoinGroup()
        {
            byte[] opCodeAndSize = new byte[2];
            byte[] buffer;
            string randomString = Others.GetRandomString(Others.Random(4, 10));
            Lua.LuaDoString(randomString + " = GetRealmName()");
            byte[] bufferName = MimesisHelpers.StringToBytes(ObjectManager.Me.Name + "-" + Lua.GetLocalizedText(randomString));
            opCodeAndSize[0] = (byte) MimesisHelpers.opCodes.RequestGrouping;
            opCodeAndSize[1] = (byte) bufferName.Length;
            NetworkStream clientStream = client.GetStream();
            clientStream.Write(opCodeAndSize, 0, 2);
            clientStream.Write(bufferName, 0, bufferName.Length); // It's hardcoded "PlayerName-RealmName"
            clientStream.Flush();
            // Now wait for an answer
            try
            {
                int bytesRead = clientStream.Read(opCodeAndSize, 0, 2);
                int len = opCodeAndSize[1]; // It's 4 (one uint)
                buffer = new byte[len];
                bytesRead += clientStream.Read(buffer, 0, len);
                RollId = BitConverter.ToUInt32(buffer, 0);
            }
            catch (Exception e)
            {
                Logging.WriteError("MimesisClientCom > JoinGroup(): " + e);
                return;
            }
            EventsListener.UnHookEvent(WoWEventsType.GROUP_ROSTER_UPDATE, callback => CloseGroupPopup());
            EventsListener.HookEvent(WoWEventsType.GROUP_ROSTER_UPDATE, callback => CloseGroupPopup());
            System.Threading.Thread.Sleep(250 + 2*Usefuls.Latency);
            Lua.LuaDoString("AcceptGroup()");
        }

        public static void CloseGroupPopup()
        {
            Lua.LuaDoString("StaticPopup_Hide(\"PARTY_INVITE\")");
            EventsListener.UnHookEvent(WoWEventsType.GROUP_ROSTER_UPDATE, callback => CloseGroupPopup());
            EventsListener.HookEvent(WoWEventsType.START_LOOT_ROLL, callback => RollItem());
        }

        public static void ProcessEvents()
        {
            byte[] opCodeAndSize = new byte[2];
            byte[] buffer = new byte[1];
            opCodeAndSize[0] = (byte) MimesisHelpers.opCodes.QueryEvent;
            opCodeAndSize[1] = 0;

            NetworkStream clientStream = client.GetStream();
            clientStream.Write(opCodeAndSize, 0, 2);
            clientStream.Flush();

            // Now wait for an answer
            try
            {
                int bytesRead = clientStream.Read(opCodeAndSize, 0, 2);
                int len = opCodeAndSize[1];
                if (len > 0)
                {
                    buffer = new byte[len];
                    bytesRead += clientStream.Read(buffer, 0, len);
                }
            }
            catch (Exception e)
            {
                Logging.WriteError("MimesisClientCom > ProcessEvents(): " + e);
                return;
            }
            if ((MimesisHelpers.opCodes) opCodeAndSize[0] == MimesisHelpers.opCodes.ReplyEvent && opCodeAndSize[1] > 0)
            {
                MimesisHelpers.MimesisEvent evt = MimesisHelpers.BytesToStruct<MimesisHelpers.MimesisEvent>(buffer);
                if (oldTaskList.Contains(evt))
                    return;
                switch (evt.eType)
                {
                    case MimesisHelpers.eventType.pickupQuest:
                        oldTaskList.Add(evt);
                        break;
                        Logging.WriteDebug("Received pickupquest " + evt.EventValue2);
                        myTaskList.Add(evt);
                        break;
                    case MimesisHelpers.eventType.turninQuest:
                        oldTaskList.Add(evt);
                        break;
                        Logging.WriteDebug("Received turninquest " + evt.EventValue2);
                        myTaskList.Add(evt);
                        break;
                    case MimesisHelpers.eventType.mount:
                        Logging.WriteDebug("Received mount type " + (MountCapacity) evt.EventValue1);
                        myTaskList.Add(evt);
                        oldTaskList.Add(evt);
                        break;
                }
            }
        }

        public static MimesisHelpers.MimesisEvent GetBestTask
        {
            get
            {
                if (myTaskList.Count == 0 || myTaskList[0].eType == MimesisHelpers.eventType.none)
                {
                    if (myTaskList.Count > 0)
                        myTaskList.Remove(myTaskList[0]); // Stop blocking the mimesis client if received a wrong/corrupted event from the master.
                    return new MimesisHelpers.MimesisEvent();
                }
                foreach (var mimesisEvent in myTaskList)
                {
                    if (mimesisEvent.eType == MimesisHelpers.eventType.turninQuest && Quest.GetLogQuestId().Contains(mimesisEvent.EventValue2) && Quest.GetLogQuestIsComplete(mimesisEvent.EventValue2))
                        return mimesisEvent; // If a quest is finishable right away, just do it as a priority. (the Quester bot sends QuestPickUp faster than QuestTurnIn for some reasons)

                    // We need to do something about the following issue: Master tells you to TurnIn a quest, but you did not complete it ?
                    // Then what to do ? Create a task to finish that quest ? Or just ignore the order ?
                    // Also, what happend if the Master tells you to PickUp a quest that you cannot PickUp ?
                    // Do we stay at the NPC for ever like actually ? Or do we ignore the order ? (we need to receive the PickUp requirement from the master)
                }

                return myTaskList[0];
            }
        }

        public static void DoTasks()
        {
            MimesisHelpers.MimesisEvent evt = GetBestTask;
            if (evt.eType == MimesisHelpers.eventType.none)
                return; // "new instance of MimesisEvent" => nothing to do
            switch (evt.eType)
            {
                case MimesisHelpers.eventType.pickupQuest:
                case MimesisHelpers.eventType.turninQuest:
                    List<WoWUnit> listU = ObjectManager.GetWoWUnitByEntry(evt.EventValue1);
                    if (listU.Count > 0)
                    {
                        WoWUnit u = listU[0];
                        Npc quester = new Npc();
                        quester.Entry = evt.EventValue1;
                        quester.Position = u.Position;
                        quester.Name = u.Name;
                        bool cancelPickUp = false;
                        if (evt.eType == MimesisHelpers.eventType.pickupQuest && !Quest.GetQuestCompleted(evt.EventValue2) && !Quest.GetLogQuestId().Contains(evt.EventValue2))
                            Quest.QuestPickUp(ref quester, evt.EventString1, evt.EventValue2, out cancelPickUp);
                        else if (evt.eType == MimesisHelpers.eventType.turninQuest && Quest.GetLogQuestId().Contains(evt.EventValue2) && Quest.GetLogQuestIsComplete(evt.EventValue2))
                            Quest.QuestTurnIn(ref quester, Quest.GetLogQuestTitle(evt.EventValue2), evt.EventValue2);
                        CleanQuestEvents(cancelPickUp);
                    }
                    break;
                case MimesisHelpers.eventType.mount:
                    switch ((MountCapacity) evt.EventValue1)
                    {
                        case MountCapacity.Ground:
                            MountTask.MountingGroundMount(true);
                            break;
                        case MountCapacity.Fly:
                            MountTask.MountingFlyingMount(true);
                            break;
                        case MountCapacity.Swimm:
                            MountTask.MountingAquaticMount(true);
                            break;
                        default:
                            MountTask.DismountMount(true);
                            break;
                    }
                    myTaskList.Remove(evt);
                    break;
            }
        }

        public static void CleanQuestEvents(bool cancelPickUp)
        {
            List<MimesisHelpers.MimesisEvent> evtsToRemove = new List<MimesisHelpers.MimesisEvent>();
            foreach (MimesisHelpers.MimesisEvent evt in myTaskList)
            {
                if (evt.eType == MimesisHelpers.eventType.pickupQuest)
                {
                    if (evt.EventValue2 == 0 || Quest.GetQuestCompleted(evt.EventValue2) || Quest.GetLogQuestId().Contains(evt.EventValue2))
                    {
                        if (evt.EventValue2 == 0) // happend if you abandon a quest from the master and restart Quester to take it
                            Logging.Write("Received an invalid QuestPickUp from the master, if you abandonned a quest from it, please restart the Master bot");
                        else if (!myQuestList.Contains(evt.EventValue2))
                            myQuestList.Add(evt.EventValue2);
                        evtsToRemove.Add(evt);
                        break;
                    }
                    if (cancelPickUp)
                        evtsToRemove.Add(evt); // The NPC Found did not have any quest take-able for us.
                }
                else if (evt.eType == MimesisHelpers.eventType.turninQuest)
                {
                    if (evt.EventValue2 == 0 || !Quest.GetLogQuestIsComplete(evt.EventValue2) || !Quest.GetLogQuestId().Contains(evt.EventValue2))
                    {
                        if (evt.EventValue2 == 0) // should not be a case
                            Logging.Write("Received an invalid QuestTurnIn from the master, cannot TurnIn the right quest.");
                        else if (myQuestList.Contains(evt.EventValue2))
                            myQuestList.Remove(evt.EventValue2);
                        evtsToRemove.Add(evt);
                        break;
                    }
                }
            }
            if (evtsToRemove.Count <= 0)
                return;
            foreach (MimesisHelpers.MimesisEvent mimesisEvent in evtsToRemove)
            {
                myTaskList.Remove(mimesisEvent);
            }
        }

        // I will need to expand this for new Tasks
        public static bool HasTaskToDo()
        {
            return myTaskList.Count > 0;
        }

        // We need the roll Id, but since the events have no data, how ?
        public static void RollItem()
        {
            RollId++;
            string randomString = Others.GetRandomString(Others.Random(4, 10));
            Lua.LuaDoString("_, " + randomString + " = GetLootRollItemLink(" + RollId + ")");
            string itemLink = Lua.GetLocalizedText(randomString);

            // Then we need to use new code located in nManager/Wow/Helpers/ItemSelection.cs

            Lua.LuaDoString("RollOnLoot(" + RollId + ", 2)"); // Roll "Greed"
            Logging.Write("Doing Loot Roll Greed on RollId=" + RollId);
            // 0 - Pass (declines the loot)
            // 1 - Roll "need" (wins if highest roll)
            // 2 - Roll "greed" (wins if highest roll and no other member rolls "need")
            // 3 - Disenchant

            // register event CONFIRM_LOOT_ROLL
            EventsListener.HookEvent(WoWEventsType.CONFIRM_LOOT_ROLL, callback => ConfirmLootRoll(RollId));
        }

        public static void ConfirmLootRoll(uint id)
        {
            Logging.Write("Confirm Roll on RollId=" + id);
            Lua.LuaDoString("ConfirmLootRoll(" + id + ")");
            EventsListener.UnHookEvent(WoWEventsType.CONFIRM_LOOT_ROLL, callback => ConfirmLootRoll(RollId));
        }
    }
}