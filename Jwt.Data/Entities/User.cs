using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jwt.Data.Entities
{
    public  class User : BaseEntity
    {
        public User()
        {
        }

        public string Name { get; set; } = String.Empty;
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string UserName { get; set; }

    }
}
