namespace vaulterpAPI.Models.Inventory
{
    public class StockDto
    {
        public int stock_id { get; set; }

        public int item_id { get; set; }

        public int office_id { get; set; }

        public int current_qty { get; set; }

        public int min_qty { get; set; }

        public string name { get; set; }            
        public string description { get; set; }
        public int category_id { get; set; }
    }
}
