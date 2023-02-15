using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Web;
using System.Diagnostics;
using ItemBaseUpdater;

namespace UpdateBase
{
    class Program
    {
        static List<Items> Items { get; set; } = new();
        static int Count { get; set; }
        static int RowId { get; set; } = 5;

        static void Main(string[] args)
        {
            try
            {
                PreventSleep.Enable();
                Console.WriteLine($"Program started: {DateTime.Now}");
                Console.Write($"Site content row id (usually - {RowId}): ");
                RowId = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();

                //var json = JObject.Parse(File.ReadAllText("SteamItemsBase.json"));
                var json = JObject.Parse(DropboxRequest.Get.Read("SteamItemsBase.json"));
                #pragma warning disable CS8601 // Possible null reference assignment.
                Items = JArray.Parse(json["Items"].ToString()).ToObject<List<Items>>();
                #pragma warning restore CS8601 // Possible null reference assignment.

                Console.WriteLine($"Types:");
                var count = 13;
                for (int i = 0; i < count; i++)
                    Console.WriteLine($"{i}. {(Type)i}");
                Console.WriteLine($"99. Set steam item_nameid");

                Console.Write("Select type (empty, check everything): ");
                var id = Console.ReadLine();
                Console.WriteLine();
                var isInt = int.TryParse(id, out int type);
                if (!string.IsNullOrEmpty(id) && isInt && type != 99)
                {
                    Console.WriteLine($"Checking '{(Type)type}':");
                    Console.WriteLine("==========================");
                    Check((Type)type);
                }
                else if (string.IsNullOrEmpty(id))
                {
                    Console.WriteLine("Checking everything:");
                    Console.WriteLine("==========================");
                    for (int i = 0; i < count; i++)
                        Check((Type)i);
                }
                else if (type == 99)
                {
                    Console.WriteLine("Set steam item_nameid for items:");
                    Console.WriteLine("==========================");
                    SetSteamId();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\n***********************");
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine($"\r\n========================\r\n[{Count}] Program ended: {DateTime.Now}");
                if (Count > 0)
                    SaveJsonFile();
                PreventSleep.Disable();
                Console.ReadKey();
            }
        }
        static void SaveJsonFile()
        {
            var newItemsBase = JArray.FromObject(Items);
            var sorted = new JArray(newItemsBase.OrderBy(obj => (string)obj["Type"]));
            var json = new JObject(
                    new JProperty("Updated", DateTime.Now),
                    new JProperty("Items", sorted));
            File.WriteAllText($"SteamItemsBase.json", json.ToString());

            var psi = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(psi);
        }

        static void Check(Type type)
        {
            List<string> items;
            switch (type)
            {
                case Type.Weapon:
                    items = new()
                    {
                        "CZ75-Auto", "Desert Eagle", "Glock-18", "USP-S", "P250", "Five-SeveN", "P2000", "Tec-9", "R8 Revolver", "Dual Berettas",
                        "P90", "UMP-45", "MAC-10", "MP7", "MP9", "MP5-SD", "PP-Bizon",
                        "Sawed-Off", "MAG-7", "Nova", "XM1014", "Negev", "M249",
                        "AK-47", "AWP", "M4A4", "M4A1-S", "AUG", "SG 553", "Galil AR", "FAMAS", "SSG 08", "SCAR-20", "G3SG1"
                    };
                    break;
                case Type.Knife:
                    items = new()
                    {
                        "Nomad Knife", "Skeleton Knife", "Survival Knife", "Paracord Knife", "Classic Knife", "Bayonet",
                        "Bowie Knife", "Butterfly Knife", "Falchion Knife", "Flip Knife", "Gut Knife", "Huntsman Knife",
                        "Karambit", "M9 Bayonet", "Navaja Knife", "Shadow Daggers", "Stiletto Knife", "Talon Knife", "Ursus Knife"
                    };
                    break;
                case Type.Sticker:
                    items = new()
                    {
                        "Regular", "Tournament"
                    };
                    break;
                case Type.Container:
                    items = new()
                    {
                        "Skin Cases", "Souvenir Packages", "Sticker Capsules", "Autograph Capsules"
                    };
                    break;
                default:
                    items = new()
                    {
                        type.ToString()
                    };
                    break;
            }

            int count = 0;
            foreach (var item in items)
            {
                var url = string.Empty;
                switch (type)
                {
                    case Type.Weapon or Type.Knife:
                        url = "https://csgostash.com/weapon/" + item.Replace(" ", "+") + "?name=&rarity_contraband=1&rarity_ancient=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&rarity_uncommon=1&rarity_common=1&has_st=1&no_st=1&has_souv=1&no_souv=1&sort=name&order=desc&page=";
                        break;
                    case Type.Gloves:
                        url = "https://csgostash.com/gloves?name=&gloves_hydra=1&gloves_bloodhound=1&gloves_driver=1&gloves_handwraps=1&gloves_moto=1&gloves_specialist=1&gloves_sport=1&sort=name&order=desc&page=";
                        break;
                    case Type.Agent:
                        url = "https://csgostash.com/agents?name=&team_t=1&team_ct=1&rarity_ancient=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&sort=name&order=desc&page=";
                        break;
                    case Type.Sticker:
                        url = "https://csgostash.com/stickers/" + item.ToLower() + "?name=&sticker_type=any&rarity_contraband=1&rarity_covert=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&container=any&sort_agg=avg&sort=name&order=desc&page=";
                        break;
                    case Type.Patch:
                        url = "https://csgostash.com/patches?name=&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&sort=name&order=desc&page=";
                        break;
                    case Type.Collectable:
                        url = "https://csgostash.com/pins?name=&rarity_ancient=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&sort=name&order=desc&page=";
                        break;
                    case Type.Key or Type.Pass:
                        url = "https://csgostash.com/items?page=";
                        break;
                    case Type.MusicKit:
                        url = "https://csgostash.com/music?name=&container=any&sort=name&order=desc&page=";
                        break;
                    case Type.Graffiti:
                        url = "https://csgostash.com/graffiti?name=&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&rarity_common=1&graffiti_type=any&container=any&sort=name&order=desc&page=";
                        break;
                    case Type.Container:
                        url = "https://csgostash.com/containers/" + item.Replace(" ", "-").ToLower() + "?name=&sort=name&order=desc&page=";
                        break;
                    case Type.Gift:
                        url = "https://csgostash.com/containers/gift-packages?name=&sort=name&order=desc&page=";
                        break;
                }

                int pages = 1;
                for (int i = 1; i <= pages; i++)
                {
                    int start = Count;

                    var page = url + i;
                    HtmlDocument htmlDoc = new();
                    var html = HttpRequest.RequestGetAsync(page).Result;
                    htmlDoc.LoadHtml(html);
                    RowId = item == "Souvenir Packages" ? RowId : RowId - 1;
                    HtmlNodeCollection skins = htmlDoc.DocumentNode.SelectNodes($"//div[@class='container main-content']/div[@class='row'][{RowId}]/div[@class='col-lg-4 col-md-6 col-widen text-center']");

                    pages = CountPages(htmlDoc, item == "Souvenir Packages");
                    foreach (HtmlNode skin in skins)
                    {
                        switch (type)
                        {
                            case Type.Weapon or Type.Knife or Type.Gloves or Type.Sticker or Type.Patch or Type.Collectable or Type.Graffiti:
                                html = HttpRequest.RequestGetAsync(skin.SelectSingleNode(".//div[2]/p/a").Attributes["href"].Value).Result;
                                htmlDoc = new();
                                htmlDoc.LoadHtml(html);
                                break;
                        }
                        switch (type)
                        {
                            case Type.Weapon:
                                CheckWeapon(htmlDoc, skin, item);
                                break;
                            case Type.Knife:
                                CheckKnife(htmlDoc, skin, item);
                                break;
                            case Type.Gloves:
                                CheckGloves(htmlDoc, skin);
                                break;
                            case Type.Agent:
                                CheckAgent(skin);
                                break;
                            case Type.Sticker or Type.Patch or Type.Collectable:
                                CheckStickerPatchCollectable(htmlDoc, skin, type);
                                break;
                            case Type.Key or Type.Pass:
                                CheckOther(htmlDoc, skin, type);
                                break;
                            case Type.MusicKit:
                                CheckMusic(skin);
                                break;
                            case Type.Graffiti:
                                CheckGraffiti(htmlDoc, skin);
                                break;
                            case Type.Container or Type.Gift:
                                CheckContainer(skin, type);
                                break;
                        }
                    }

                    count += Count - start;
                    Console.Write($"\r{new string(' ', Console.WindowWidth)}\r");
                    Console.Write($"[{Count - start}] | '{type}' | {items.IndexOf(item) + 1}/{items.Count} kinds | '{item}' | {i}/{pages} page");
                }
            }
            Console.Write($"\r{new string(' ', Console.WindowWidth)}\r");
            Console.Write($"[{count}] | '{type}'");
        }
        static void CheckWeapon(HtmlDocument htmlDoc, HtmlNode skin, string item)
        {
            HtmlNodeCollection exteriors = htmlDoc.DocumentNode.SelectNodes("//table[@class='table table-hover table-bordered table-condensed price-details-table']/tbody/tr");

            foreach (HtmlNode exterior in exteriors)
            {
                var exter = exterior.SelectSingleNode(".//td").InnerText.Trim();
                var option = SetOption(exter);
                exter = exter.Replace("StatTrak ", string.Empty);
                exter = exter.Replace("Souvenir ", string.Empty);
                var skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();

                var itemName = option + $"{item} | {skinName} ({exter})";
                var quality = SetQuality(skin.SelectSingleNode(".//a/div/p").InnerText.Trim());
                AddBase(itemName, quality, Type.Weapon);
            }
        }
        static void CheckKnife(HtmlDocument htmlDoc, HtmlNode skin, string item)
        {
            HtmlNodeCollection exteriors = htmlDoc.DocumentNode.SelectNodes("//table[@class='table table-hover table-bordered table-condensed price-details-table']/tbody/tr");

            foreach (HtmlNode exterior in exteriors)
            {
                var exter = exterior.SelectSingleNode(".//td").InnerText.Trim();
                var option = "★ " + SetOption(exter);
                exter = exter.Replace("StatTrak ", string.Empty);
                var skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();

                var itemName = option + $"{item} | {skinName} ({exter})";
                if (skinName == "★ (Vanilla)")
                    itemName = option + item;
                var quality = SetQuality(skin.SelectSingleNode(".//a/div/p").InnerText.Trim());
                AddBase(itemName, quality, Type.Knife);
            }
        }
        static void CheckGloves(HtmlDocument htmlDoc, HtmlNode skin)
        {
            HtmlNodeCollection exteriors = htmlDoc.DocumentNode.SelectNodes("//table[@class='table table-hover table-bordered table-condensed price-details-table']/tbody/tr");

            foreach (HtmlNode exterior in exteriors)
            {
                var exter = exterior.SelectSingleNode(".//td").InnerText.Trim();
                var option = "★ " + SetOption(exter);
                var skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();

                var itemName = option + skinName + $" ({exter})";
                var quality = SetQuality(skin.SelectSingleNode(".//div/div/p").InnerText.Trim());
                AddBase(itemName, quality, Type.Gloves);
            }
        }
        static void CheckAgent(HtmlNode skin)
        {
            var itemName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();
            var quality = SetQuality(skin.SelectSingleNode(".//div/p").InnerText.Trim());

            AddBase(itemName, quality, Type.Agent);
        }
        static void CheckStickerPatchCollectable(HtmlDocument htmlDoc, HtmlNode skin, Type type)
        {
            var itemName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='well result-box nomargin']/div/div/h2").InnerText.Trim();
            var quality = SetQuality(skin.SelectSingleNode(".//div/p").InnerText.Trim());
            AddBase(itemName, quality, type);
        }
        static void CheckOther(HtmlDocument htmlDoc, HtmlNode skin, Type type)
        {
            var itemName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='well result-box nomargin']/a/h4").InnerText.Trim();
            var quality = SetQuality(skin.SelectSingleNode(".//div/p").InnerText.Trim());
            if (itemName.Contains(type.ToString()))
                AddBase(itemName, quality, type);
        }
        static void CheckMusic(HtmlNode skin)
        {
            var title = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();
            var artist = skin.SelectSingleNode(".//div/h4").InnerText.Trim().Replace("By ", string.Empty);
            var strk = skin.SelectSingleNode(".//div/div[2]").InnerText.Trim();
            var itemName = !title.Contains(',') ? $"Music Kit | {artist}, {title}" : $"Music Kit | {artist}: {title}";

            var quality = SetQuality(skin.SelectSingleNode(".//div/div").InnerText.Trim());
            switch (strk)
            {
                case "StatTrak Available":
                    AddBase(itemName, quality, Type.MusicKit);
                    AddBase("StatTrak™ " + itemName, quality, Type.MusicKit);
                    break;
                case "StatTrak Only":
                    AddBase("StatTrak™ " + itemName, quality, Type.MusicKit);
                    break;
                default:
                    AddBase(itemName, quality, Type.MusicKit);
                    break;
            }
        }
        static void CheckGraffiti(HtmlDocument htmlDoc, HtmlNode skin)
        {
            var skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();
            var quality = SetQuality(skin.SelectSingleNode(".//div/p").InnerText.Trim());
            if (quality == Quality.IndustrialGrade)
            {
                HtmlNodeCollection colors = htmlDoc.DocumentNode.SelectNodes("//div[@class='container main-content']/div[@class='row text-center'][3]/div[@class='col-lg-3 col-md-4 col-sm-6 col-widen']");
                foreach (HtmlNode color in colors)
                {
                    var nameColor = color.SelectSingleNode(".//div/h4").InnerText;
                    AddBase($"Sealed Graffiti | {skinName} ({nameColor})", quality, Type.Graffiti);
                }
            }
            else
                AddBase($"Sealed Graffiti | {skinName}", quality, Type.Graffiti);
        }
        static void CheckContainer(HtmlNode skin, Type type)
        {
            var itemName = skin.SelectSingleNode(".//div/a/h4").InnerText.Trim();

            AddBase(itemName, null, type);
        }

        static void SetSteamId()
        {
            int i = 1;
            var items = Items.Where(x => x.Steam.Id == 0);
            int count = items.Count();
            foreach (var item in items)
            {
                var html = string.Empty;
                while (string.IsNullOrEmpty(html))
                {
                    html = SteamRequest(item.ItemName);
                    Thread.Sleep(100);
                }

                item.Steam.Id = ItemNameId(html);

                int current = Items.Where(x => x.Steam.Id == 0).Count();
                Console.Write($"\r{new string(' ', Console.WindowWidth)}\r");
                decimal timeLeft = Math.Round(Math.Ceiling((count - i) / 25m) * 5m, 1);
                Console.Write($"[{count - current}] | {i}/{count} | ~{timeLeft} minutes left");
                i++;
            }
            SaveJsonFile();
        }
        static void AddBase(string itemName, Quality? quality, Type type)
        {
            List<string> exceptions = new()
            {
                "MP5-SD | Lab Rats (Factory New)",
                "MP5-SD | Lab Rats (Minimal Wear)",
                "MP5-SD | Lab Rats (Field-Tested)"
            };

            itemName = HttpUtility.HtmlDecode(itemName);
            itemName = itemName.Trim();
            itemName = itemName.Replace("\n", " ");

            if (Items.FirstOrDefault(x => x.ItemName == itemName) == null && !exceptions.Any(x => x == itemName))
            {
                Items.Add(new()
                {
                    ItemName = itemName,
                    Quality = quality,
                    Type = type
                });
                Count++;
            }
        }
        static Quality? SetQuality(string line)
        {
            if (line.Contains("Contraband"))
                return Quality.Contraband;
            if (line.Contains("Covert") || line.Contains("Extraordinary") || line.Contains("Master Agent"))
                return Quality.Covert;
            if (line.Contains("Classified") || line.Contains("Exotic") || line.Contains("Superior Agent"))
                return Quality.Classified;
            if (line.Contains("Restricted") || line.Contains("Remarkable") || line.Contains("Exceptional Agent"))
                return Quality.Restricted;
            if (line.Contains("Mil-Spec") || line.Contains("High Grade") || line.Contains("Distinguished Agent"))
                return Quality.MilSpec;
            if (line.Contains("Industrial Grade") || line.Contains("Base Grade Graffiti"))
                return Quality.IndustrialGrade;
            if (line.Contains("Consumer Grade"))
                return Quality.ConsumerGrade;

            return null;
        }
        static string SetOption(string line)
        {
            if (line.Contains("StatTrak"))
                return "StatTrak™ ";
            if (line.Contains("Souvenir"))
                return "Souvenir ";

            return string.Empty;
        }
        static Int32 CountPages(HtmlDocument htmlDoc, bool isSouvenir)
        {
            try
            {
                int rowId = isSouvenir ? 4 : 3;
                var page = htmlDoc.DocumentNode.SelectNodes("//div[@class='container main-content']/div[@class='row'][" + rowId + "]/div/ul/li");
                page.Remove(page.LastOrDefault());

                return Int32.Parse(page.LastOrDefault().InnerText);
            }
            catch
            {
                return 1;
            }
        }
        static string SteamRequest(string itemName)
        {
            try
            {
                return HttpRequest.RequestGetAsync("https://steamcommunity.com/market/listings/730/" + HttpUtility.UrlPathEncode(itemName)).Result;
            }
            catch
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
                return string.Empty;
            }
        }
        static Int32 ItemNameId(string html)
        {
            try
            {
                html = html.Substring(html.IndexOf("Market_LoadOrderSpread"));
                var a = html.IndexOf("(");
                var b = html.IndexOf(")");
                var str = html.Substring(a, b);

                int id = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, @"[^\d]+", ""));

                return id;
            }
            catch
            {
                return 0;
            }
        }
    }
}
