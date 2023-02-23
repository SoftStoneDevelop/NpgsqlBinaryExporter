namespace NpgsqlBenchmark.Model
{
    public class Person
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public Identification Identification { get; set; }
    }
}