namespace InertGas.Common.Model
{
    public class User : PersistableModel
    {
        public override string Id { get; }

        public UserRole Role { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public DateTime CreatedDate { get; set; }

        public byte[] Salt { get; set; }
    }
}
