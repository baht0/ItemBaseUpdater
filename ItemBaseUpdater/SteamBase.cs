namespace UpdateBase
{
    public class Items
    {
        public string ItemName { get; set; } = string.Empty;
        public Type Type { get; set; }
        public Quality? Quality { get; set; }
        public SteamItem Steam { get; set; } = new();
    }
    public class SteamItem
    {
        public int Id { get; set; }
    }
    public enum Type
    {
        Weapon,
        Knife,
        Gloves,
        Agent,
        Sticker,
        Patch,
        Collectable,
        Key,
        Pass,
        MusicKit,
        Graffiti,
        Container,
        Gift,
        Tool
    }
    public enum Quality
    {
        ConsumerGrade,
        IndustrialGrade,
        MilSpec,
        Restricted,
        Classified,
        Covert,
        Contraband
    }
}