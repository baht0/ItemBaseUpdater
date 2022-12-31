namespace UpdateBase
{
    public class Currency
    {
        public int Id { get; set; } = 0;
        public string Code { get; set; } = String.Empty;
        public string Country { get; set; } = String.Empty;
        public string Symbol { get; set; } = String.Empty;
    }
    public class Items
    {
        public string itemName { get; set; } = String.Empty;
        public int steamId { get; set; } = 0;
        public string type { get; set; } = String.Empty;
        public string quality { get; set; } = String.Empty;
    }
}