using System.Xml.Schema;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.DependencyInjection;
using static CounterStrikeSharp.API.Utilities;

namespace WeaponAllocatorLite;

public class WeaponAllocatorLite : BasePlugin
{
    public static WeaponAllocatorLite Plugin = null!;

    public override string ModuleName => "Weapon Allocator Lite Plugin";

    public override string ModuleVersion => "0.0.3";

    public static List<Allocator> Allocators = new();

    public override void Load(bool hotReload)
    {
        Plugin = this;

        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        
        
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnClientAuthorized>(OnClientAuthorized);
        RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);

        if (hotReload)
        {
            Allocators.Clear();
            GetPlayers().ForEach(AddToAllocators);
        }
    }

    public static Allocator FindAllocator(CCSPlayerController player)
    {
        return Allocators.Find(x => x.Index == player.Index)!;
    }

    public static void AddToAllocators(CCSPlayerController player)
    {
        if(FindAllocator(player) != null!)
        {
            return;
        }

        var allocatorObj = new Allocator(player);

        Allocators.Add(allocatorObj);
    }

    public static void RemoveFromList(CCSPlayerController player)
    {
        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }

        allocatorObj.SavePreferences();

        Allocators.Remove(allocatorObj);
    }

    private static void OnMapStart(string map_name)
    {
        Allocators.Clear();
        GetPlayers().ForEach(AddToAllocators);
    }

    private static void OnClientAuthorized(int playerSlot, SteamID steamID)
    {
        var player = GetPlayerFromSlot(playerSlot);

        if (player == null || !player.IsValid || player.IsBot)
        {
            return;
        }

        AddToAllocators(player);

        int ct = 0;
        int t = 0;

        foreach (CCSPlayerController x in GetPlayers())
        {
            if (x.Team == CsTeam.Terrorist)
            {
                t++;
            }
            else if (x.Team == CsTeam.CounterTerrorist)
            {
                ct++;
            }
        }
        
        CsTeam teamToPick = ct <= t ? CsTeam.CounterTerrorist : CsTeam.Terrorist;

        player.ChangeTeam(teamToPick);

        player.PrintToChat("Select primary with !prim <weapon>");
        player.PrintToChat("Select secondary with !seco <weapon>");
        player.PrintToChat("Toggle random weapons with !random <yes|no>");
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

    [ConsoleCommand("prim", "This changes primary weapon!")]
    public void OnCommandPrim(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;
        
        if (command.ArgCount < 2)
        {
            player.PrintToChat("Command syntax: !prim <weapon>");
            return;
        }
      
        string primary = command.ArgByIndex(1);

        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }

        var isValidEnum = Enum.TryParse(primary, true, out CsItem parsedPrimary) &&
                                            Enum.IsDefined(typeof(CsItem), parsedPrimary);

        if (isValidEnum && ((int)CsItem.Mac10 <= (int)parsedPrimary) && ((int)parsedPrimary <= (int)CsItem.AutoSniperT))
        {
            allocatorObj.Primary = parsedPrimary;
            string strPrimary = parsedPrimary.ToString();
            player.PrintToChat($"Succesfully changed primary to {strPrimary}");
        }
        else
        {
            player.PrintToChat($"Invalid input; Couldn't find {primary} in weapon list.");
        }
    }

    [ConsoleCommand("seco", "This changes secondary weapon!")]
    public void OnCommandSeco(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (command.ArgCount < 2)
        {
            player.PrintToChat("Command syntax: !seco <weapon>");
            return;
        }

        string secondary = command.ArgByIndex(1);

        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }

        var isValidEnum = Enum.TryParse(secondary, true, out CsItem parsedSecondary) &&
                                                Enum.IsDefined(typeof(CsItem), parsedSecondary);

        if (isValidEnum && ((int)CsItem.Deagle <= (int)parsedSecondary) && ((int)parsedSecondary <= (int)CsItem.Revolver))
        {
            allocatorObj.Secondary = parsedSecondary;
            string strSecondary = parsedSecondary.ToString();
            player.PrintToChat($"Succesfully changed secondary to {strSecondary}");
        }
        else
        {
            player.PrintToChat($"Invalid input; Couldn't find {secondary} in weapon list.");
        }
    }

    [ConsoleCommand("random", "This changes random choice!")]
    public void OnCommandRandom(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (command.ArgCount < 2)
        {
            player.PrintToChat("Command syntax: !random <yes|no>");
            return;
        }

        string toggle = command.ArgByIndex(1);

        var allocatorObj = FindAllocator(player);

        if (allocatorObj == null!)
        {
            return;
        }   
        
        switch (toggle)
        {
            case "true":
            case "yes":
                player.PrintToChat("Random weapons enabled!");
                allocatorObj.Random = true;
                break;
            case "false":
            case "no":
                player.PrintToChat("Random weapons disabled!");
                allocatorObj.Random = false;
                break;
            default:
                player.PrintToChat("Toggle random weapons with !random <yes|no>");
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
    private static int PRIM_OFFSET = 1;
    private static int SECO_OFFSET = 2;
    private static int RANDOM_OFFSET = 3;

    private static string PrefFile = "player_db.txt";

    public static (CsItem primary, CsItem secondary, bool random) FindAllInFile(string name)
    {
        CsItem primary = CsItem.AK47;
        CsItem secondary = CsItem.Deagle;
        bool random = true;
        string[] arrLine = File.ReadAllLines(PrefFile);

        int i;
        for(i = 0; i < arrLine.Length; i++)
        {
            if (arrLine[i].Equals(name))
            {
                primary = (CsItem)System.Convert.ToInt16(arrLine[i+PRIM_OFFSET]);
                secondary = (CsItem)System.Convert.ToInt16(arrLine[i+SECO_OFFSET]);
                random = System.Convert.ToBoolean(arrLine[i+RANDOM_OFFSET]);
                return (primary, secondary, random);
            }
        }

        return (primary, secondary, random);
    }

    public static void AddPlayerToFile(string name)
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

    public static void InsertPrefsInFile(string name, CsItem primary, CsItem secondary, bool random)
    {
        string[] arrLine = File.ReadAllLines(PrefFile);
        int i;
        for(i = 0; i < arrLine.Length; i++)
        {
            if (arrLine[i].Equals(name))
            {
                arrLine[i+PRIM_OFFSET] = ((int)primary).ToString();
                arrLine[i+SECO_OFFSET] = ((int)secondary).ToString();
                arrLine[i+RANDOM_OFFSET] = random.ToString();
                File.WriteAllLines(PrefFile, arrLine);
            }
        }
    }
}

public class Randomizer
{
    private static Random rnd = new Random();

    public static bool RandomPercentage(int percentage)
    {
        return rnd.Next(0, 100) <= percentage;
    }

    public static int RandomBetween(int start, int end)
    {
        return rnd.Next(start, end+1);
    }

    public static (CsItem primary, CsItem secondary) GetRandomWeapons()
    {
        CsItem primary, secondary;

        if (RandomPercentage(50))
        {
            primary = (CsItem)RandomBetween((int)CsItem.Mac10, (int)CsItem.Negev);
        }
        else
        {
            primary = (CsItem)RandomBetween((int)CsItem.AK47, (int)CsItem.AutoSniperT);
        }

        secondary = (CsItem)RandomBetween((int)CsItem.Deagle, (int)CsItem.Revolver);

        return (primary, secondary);
    }

    public static CsItem[] SelectGrenades()
    {
        int numGrenades = 0;
        CsItem[] possibleGrenades = [CsItem.Flashbang,  CsItem.Decoy, CsItem.HE, CsItem.Smoke, CsItem.Flashbang, CsItem.Molotov];
        CsItem[] grenadesToGive = [0];
        int[] percentages = [95, 50, 15, 2];
        CsItem pickedGrenade;
        int index;

        foreach (int x in percentages)
        {
            if (RandomPercentage(x))
            {
                pickedGrenade = possibleGrenades[RandomBetween(0, possibleGrenades.Length-1)];
                grenadesToGive = grenadesToGive.Append(pickedGrenade).ToArray();
                numGrenades++;
                
                switch (pickedGrenade)
                {
                    case CsItem.Smoke:
                        index = Array.IndexOf(possibleGrenades, CsItem.Molotov);
                        possibleGrenades = possibleGrenades.Where((val, idx) => idx != index).ToArray();
                        goto default;

                    case CsItem.Molotov:
                        index = Array.IndexOf(possibleGrenades, CsItem.Smoke);
                        possibleGrenades = possibleGrenades.Where((val, idx) => idx != index).ToArray();
                        goto default;
                    
                    default:
                        index = Array.IndexOf(possibleGrenades, pickedGrenade);
                        possibleGrenades = possibleGrenades.Where((val, idx) => idx != index).ToArray();
                        break;
                }   
            }
        }

        return grenadesToGive; 
    }
}


public class Allocator
{
    public uint Index;
    public string Name;

    private CsItem primary;
    private CsItem secondary;
    private bool random;
    public CsItem Primary
    {
        get {return primary;}
        set {primary = value;}
    }
    public CsItem Secondary
    {
        get {return secondary;}
        set {secondary = value;}
    }
    public bool Random
    {
        get {return random;}
        set {random = value;}
    }

    private CCSPlayerController playerController;
    
    public Allocator(CCSPlayerController player)
    {
        playerController = player;
        Index = player.Index;
        Name = player.PlayerName;
        FileHandler.AddPlayerToFile(player.PlayerName);
        (primary, secondary, random) = FileHandler.FindAllInFile(player.PlayerName);
    }

    public void SavePreferences()
    {
        FileHandler.InsertPrefsInFile(playerController.PlayerName, primary, secondary, random);
    }

    public bool IsValid()
    {
        return !(playerController == null! || !playerController.IsValid);
    }

    private void DoGrenades()
    {
        CsItem[] grenades = Randomizer.SelectGrenades();

        foreach (CsItem x in grenades)
        {
            if (x == CsItem.Molotov && playerController.Team == CsTeam.CounterTerrorist)
            {
                playerController.GiveNamedItem(CsItem.Incendiary);
            }
            else
            {
                playerController.GiveNamedItem(x);
            }
        }
    }

    public void AllocateStuff()
    {
        playerController.RemoveWeapons();

        CsItem primary, secondary;

        if (random)
        {
            (primary, secondary) = Randomizer.GetRandomWeapons();
        }
        else
        {
            primary = this.primary;
            secondary = this.secondary;
        }

        DoGrenades();
        playerController.GiveNamedItem(primary);
        playerController.GiveNamedItem(secondary);
        playerController.GiveNamedItem(CsItem.Knife);


        if (playerController.Team == CsTeam.CounterTerrorist && playerController.PlayerPawn.Value != null && playerController.PlayerPawn.Value.IsValid && playerController.PlayerPawn.Value?.ItemServices?.Handle != null)
        {
            var itemServices = new CCSPlayer_ItemServices(playerController.PlayerPawn.Value.ItemServices.Handle);
            itemServices.HasDefuser = true;
        }

        playerController.GiveNamedItem(CsItem.KevlarHelmet);
    }
}
