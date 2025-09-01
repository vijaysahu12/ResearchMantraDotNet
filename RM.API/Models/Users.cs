using System;

namespace RM.API.Models
{
    public class Users
    {
        public int Id { get; set; }
        public Guid PublicKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserType { get; set; }
    }
}
