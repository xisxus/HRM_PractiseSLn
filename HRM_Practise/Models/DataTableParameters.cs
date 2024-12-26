namespace HRM_Practise.Models
{
    public class DataTableParameters
    {
        public int? Draw { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
        public Search? Search { get; set; }
        public List<Order>? Order { get; set; }
        //public Order Order { get; set; }
        public List<Column>? Columns { get; set; }

        // Additional filter parameters
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        public string? MachineId { get; set; }
    }

    public class DataTableParameters2
    {
        public int? Draw { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
        public Search? Search { get; set; }
        public List<Order>? Order { get; set; }
        //public Order Order { get; set; }
        public List<Column>? Columns { get; set; }

        // Additional filter parameters
        // public DateTime? StartDate { get; set; }
        public string? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? MachineId { get; set; }
    }

    public class Search
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }
        public string Name { get; set; }
    }

    public class Column
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
    }

}
