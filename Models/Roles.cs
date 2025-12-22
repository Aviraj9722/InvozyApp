using System.Collections.Generic;

namespace eOrderTouchApp.Models
{
    public class Roles
    {
        public string key { get; set; }
        public string value { get; set; }

        public static List<Roles> GetRoles()
        {
            List<Roles> rl = new List<Roles>();
            rl.Add(new Roles() { value="Owner", key= "Owner" });
            rl.Add(new Roles() { value = "User", key = "User" });
            rl.Add(new Roles() { value = "HeadOfficer", key = "HeadOfficer" });
            

            return rl;
        }

    }
}
