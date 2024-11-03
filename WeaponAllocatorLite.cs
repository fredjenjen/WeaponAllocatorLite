using System.Data;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using System.IO;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Utilities;
using System.Runtime.InteropServices;
using System.Linq.Expressions;

namespace WeaponAllocatorLite;

public class WeaponAllocatorLite : BasePlugin
{
    public static WeaponAllocatorLite Plugin = null!;

    public static FileHandler fileHandler = null!;

    public override string ModuleName => "Weapon Allocator Lite Plugin";

    public override string ModuleVersion => "0.0.1";

    public static List<Allocator> Allocators = new();

    public override void Load(bool hotReload)
    {
        Plugin = this;

        fileHandler = new FileHandler();

        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterListener<CounterStrikeSharp.API.Core.Listeners.OnMapStart>(OnMapStart);
        RegisterListener<CounterStrikeSharp.API.Core.Listeners.OnClientAuthorized>(OnClientAuthorized);
        RegisterListener<CounterStrikeSharp.API.Core.Listeners.OnClientDisconnect>(OnClientDisconnect);

        if (hotReload)
        {
            Allocators.Clear();
            GetPlayers().ForEach(AddToList);
        }
    }

    public static Allocator FindAllocator(CCSPlayerController player)
    {
        return Allocators.Find(x => x.Index == player.Index)!;
    }

    public static void AddToList(CCSPlayerController player)
    {
        if(FindAllocator(player) != null!)
        {
            return;
        }

        var allocatorObj = new Allocator(player);

        fileHandler.AddPlayerToFile(player.PlayerName);

        allocatorObj.primary = fileHandler.FindPrimaryInFile(player.PlayerName);
        allocatorObj.secondary = fileHandler.FindSecondaryInFile(player.PlayerName);
        allocatorObj.random = fileHandler.FindRandomInFile(player.PlayerName);

        Allocators.Add(allocatorObj);
    }

    public static void RemoveFromList(CCSPlayerController player)
    {
        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }

        fileHandler.InsertPrefsInFile(allocatorObj);
        Allocators.Remove(allocatorObj);
    }

    private static void OnMapStart(string map_name)
    {
        Allocators.Clear();
        GetPlayers().ForEach(AddToList);
    }

    private static void OnClientAuthorized(int playerSlot, SteamID steamID)
    {
        var player = GetPlayerFromSlot(playerSlot);

        if (player == null || !player.IsValid || player.IsBot)
        {
            return;
        }

        AddToList(player);
    }

    private static void OnClientDisconnect(int playerSlot)
    {
        var player = GetPlayerFromSlot(playerSlot);
        if (player == null || !player.IsValid || player.IsBot)
        {
            return;
        }

        RemoveFromList(player);
    }

    [ConsoleCommand("prim", "this changes primary weapon")]
    public void OnCommandPrim(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;
        
        if (command.ArgCount < 2)
            return;
      
        string primary = command.ArgByIndex(1);

        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }

        switch (primary)
        {
            case "nova":
                allocatorObj.primary = CsItem.Nova;
                break;
            case "ak":
                allocatorObj.primary = CsItem.AK47;
                break;
            case "m4":
                allocatorObj.primary = CsItem.M4A1;
                break;
            case "m4s":
                allocatorObj.primary = CsItem.M4A1S;
                break;
            case "awp":
                allocatorObj.primary = CsItem.AWP;
                break;
            case "krieg":
                allocatorObj.primary = CsItem.Krieg;
                break;
            case "famas":
                allocatorObj.primary = CsItem.Famas;
                break;
            case "galil":
                allocatorObj.primary = CsItem.Galil;
                break;
            case "bizon":
                allocatorObj.primary = CsItem.Bizon;
                break;
            case "ump":
                allocatorObj.primary = CsItem.UMP;
                break;
            case "mp5":
                allocatorObj.primary = CsItem.MP5;
                break;
            case "mp9":
                allocatorObj.primary = CsItem.MP9;
                break;
            case "negev":
                allocatorObj.primary = CsItem.Negev;
                break;
            case "m249":
                allocatorObj.primary = CsItem.M249;
                break;
            case "mag7":
                allocatorObj.primary = CsItem.MAG7;
                break;
            case "mac10":
                allocatorObj.primary = CsItem.Mac10;
                break;
            case "scout":
                allocatorObj.primary = CsItem.Scout;
                break;
            case "tauto":
                allocatorObj.primary = CsItem.AutoSniperT;
                break;
            case "ctauto":
                allocatorObj.primary = CsItem.AutoSniperCT;
                break;
            case "autoshot":
                allocatorObj.primary = CsItem.XM1014;
                break;
            case "sawed":
                allocatorObj.primary = CsItem.SawedOff;
                break;
            case "mp7":
                allocatorObj.primary = CsItem.MP7;
                break;
            case "p90":
                allocatorObj.primary = CsItem.P90;
                break;
            case "aug":
                allocatorObj.primary = CsItem.AUG;
                break;                
        }
    }

    [ConsoleCommand("seco", "this changes secondary weapon")]
    public void OnCommandSeco(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (command.ArgCount < 2)
            return;

        string secondary = command.ArgByIndex(1);

        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }

        switch (secondary)
        {
            case "deagle":
                allocatorObj.secondary = CsItem.Deagle;
                break;
            case "p2000":
                allocatorObj.secondary = CsItem.P2000;
                break;
            case "usp":
                allocatorObj.secondary = CsItem.USP;
                break;
            case "glock":
                allocatorObj.secondary = CsItem.Glock;
                break;
            case "duals":
                allocatorObj.secondary = CsItem.Dualies;
                break;
            case "fiveseven":
                allocatorObj.secondary = CsItem.FiveSeven;
                break;
            case "r8":
                allocatorObj.secondary = CsItem.R8;
                break;
            case "revolver":
                allocatorObj.secondary = CsItem.R8;
                break;
            case "57":
                allocatorObj.secondary = CsItem.FiveSeven;
                break;
            case "p250":
                allocatorObj.secondary = CsItem.P250;
                break;
            case "cz":
                allocatorObj.secondary = CsItem.CZ75;
                break;
            case "tec9":
                allocatorObj.secondary = CsItem.Tec9;
                break;
        }

    }

    [ConsoleCommand("random", "this changes random choice")]
    public void OnCommandRandom(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (command.ArgCount < 2)
            return;

        string toggle = command.ArgByIndex(1);

        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }   
        
        switch (toggle)
        {
            case "yes":
                allocatorObj.random = true;
                break;
            case "no":
                allocatorObj.random = false;
                break;
        }
    }

    private static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null! || !player.IsValid)
        {
            return HookResult.Continue;
        }

        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null! || !allocatorObj.IsValid())
        {
            return HookResult.Continue;
        }

        Plugin.AddTimer(0.1f, allocatorObj.AllocateStuff);
        
        return HookResult.Continue;
    }
}




public class FileHandler
{
    private int PRIM_OFFSET = 1;
    private int SECO_OFFSET = 2;
    private int RANDOM_OFFSET = 3;

    private string PrefFile = "player_db.txt";

    public CsItem FindPrimaryInFile(string name)
    {
        CsItem primary = CsItem.AK47;
        string[] arrLine = File.ReadAllLines(PrefFile);

        int i;
        for(i = 0; i < arrLine.Length; i++)
        {
            if (arrLine[i].Equals(name))
            {
                return (CsItem)System.Convert.ToInt16(arrLine[i+PRIM_OFFSET]);
            }
        }

        return primary;
    }

    public CsItem FindSecondaryInFile(string name)
    {
        CsItem secondary = CsItem.Deagle;
        string[] arrLine = File.ReadAllLines(PrefFile);

        int i;
        for(i = 0; i < arrLine.Length; i++)
        {
            if (arrLine[i].Equals(name))
            {
                return (CsItem)System.Convert.ToInt16(arrLine[i+SECO_OFFSET]);
            }
        }

        return secondary;
    }

    public bool FindRandomInFile(string name)
    {
        bool random = true;
        string[] arrLine = File.ReadAllLines(PrefFile);

        int i;
        for(i = 0; i < arrLine.Length; i++)
        {
            if (arrLine[i].Equals(name))
            {
                return System.Convert.ToBoolean(arrLine[i+RANDOM_OFFSET]);
            }
        }

        return random;
    }

    public void AddPlayerToFile(string name)
    {
        string[] arrLine;

        if (File.Exists(PrefFile))
        {
            arrLine = File.ReadAllLines(PrefFile);

            for(int i = 0; i < arrLine.Length; i++)
            {
                if (arrLine[i].Equals(name))
                {
                    return;
                }
            }
        }
        else
        {
            File.WriteAllLines(PrefFile, [""]);
            arrLine = File.ReadAllLines(PrefFile);
        }

        arrLine = arrLine.Append(name).ToArray();
        arrLine = arrLine.Append(((int)CsItem.AK47).ToString()).ToArray();
        arrLine = arrLine.Append(((int)CsItem.Deagle).ToString()).ToArray();
        arrLine = arrLine.Append(true.ToString()).ToArray();

        File.WriteAllLines(PrefFile, arrLine);
    }

    public void InsertPrefsInFile(Allocator allocator)
    {
        string[] arrLine = File.ReadAllLines(PrefFile);
        int i;
        for(i = 0; i < arrLine.Length; i++)
        {
            if (arrLine[i].Equals(allocator.Name))
            {
                arrLine[i+PRIM_OFFSET] = ((int)allocator.primary).ToString();
                arrLine[i+SECO_OFFSET] = ((int)allocator.secondary).ToString();
                arrLine[i+RANDOM_OFFSET] = allocator.random.ToString();
                File.WriteAllLines(PrefFile, arrLine);
            }
        }
    }
}


public class Allocator
{
    public uint Index;
    public string Name;
    public CsItem primary;
    public CsItem secondary;
    public bool random;

    private CCSPlayerController playerController;

    public Allocator(CCSPlayerController player)
    {
        playerController = player;
        Index = player.Index;
        Name = player.PlayerName;
        primary = CsItem.AK47;
        secondary = CsItem.Deagle;
        random = true;
    }

    public bool IsValid()
    {
        return !(playerController == null! || !playerController.IsValid);
    }

    private CsItem SelectGrenade(bool outlaw_blocker)
    {
        Random rnd = new Random();
        int random;
        CsItem grenade;
        if (outlaw_blocker)
        {
            random = rnd.Next(0, 3);
            if (random == 0)
            {
                grenade = CsItem.Flashbang;
            }
            else if (random == 1)
            {
                grenade = CsItem.Decoy;
            }
            else
            {
                grenade = CsItem.HE;
            }
        }
        else
        {
            random = rnd.Next(0, 5);

            if (random == 0)
            {
                grenade = CsItem.Flashbang;
            }
            else if (random == 1)
            {
                grenade = CsItem.Decoy;
            }
            else if (random == 2)
            {
                grenade = CsItem.HE;
            }
            else if (random == 3)
            {
                grenade = CsItem.Smoke;
            }
            else
            {
                if (playerController.Team == CsTeam.CounterTerrorist)
                {
                    grenade = CsItem.Firebomb;
                }
                else
                {
                    grenade = CsItem.Molotov;
                }
            }
        }

        return grenade;
    }

    public void AllocateStuff()
    {
        playerController.RemoveWeapons();

        CsItem primary;
        CsItem secondary;
        Random rnd = new Random();

        if (random)
        {
            if (rnd.Next(0, 2) == 1)
            {
                primary = (CsItem)rnd.Next(300, 313);
            }
            else
            {
                primary = (CsItem)rnd.Next(400, 411);
            }
            secondary = (CsItem)rnd.Next(200, 210);
        }
        else
        {
            primary = this.primary;
            secondary = this.secondary;
        }

        playerController.GiveNamedItem(primary);
        playerController.GiveNamedItem(secondary);
        playerController.GiveNamedItem(CsItem.Knife);

        // Get grenades!!!
        if (rnd.Next(0,2) == 1)
        {
            bool outlaw_blocker = false;
            CsItem grenade = SelectGrenade(outlaw_blocker);
            playerController.GiveNamedItem(grenade);

            if (rnd.Next(0,4) == 1)
            {
                outlaw_blocker = grenade == CsItem.Molotov || grenade == CsItem.Firebomb;
                grenade = SelectGrenade(outlaw_blocker);
                playerController.GiveNamedItem(grenade);

                if (rnd.Next(0,10) == 1)
                {
                    outlaw_blocker = grenade == CsItem.Molotov || grenade == CsItem.Firebomb;
                    grenade = SelectGrenade(outlaw_blocker);
                    playerController.GiveNamedItem(grenade);

                    if (rnd.Next(0,51) == 1)
                    {
                        outlaw_blocker = grenade == CsItem.Molotov || grenade == CsItem.Firebomb;
                        grenade = SelectGrenade(outlaw_blocker);
                        playerController.GiveNamedItem(grenade);
                    }
                }
            }
        }

        if (playerController.Team == CsTeam.CounterTerrorist && playerController.PlayerPawn.Value != null && playerController.PlayerPawn.Value.IsValid && playerController.PlayerPawn.Value?.ItemServices?.Handle != null)
        {
            var itemServices = new CCSPlayer_ItemServices(playerController.PlayerPawn.Value.ItemServices.Handle);
            itemServices.HasDefuser = true;
        }

        playerController.GiveNamedItem(CsItem.KevlarHelmet);
    }
}
