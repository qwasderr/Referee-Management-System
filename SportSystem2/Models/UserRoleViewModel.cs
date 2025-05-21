using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace SportSystem2.Models
{
    public class UserRoleViewModel
    {
        //[Required(ErrorMessage = "User is required")]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Role is required")]
        public string SelectedRole { get; set; }

        [ValidateNever]
        public List<string> Users { get; set; }
        [ValidateNever]
        public List<string> Roles { get; set; }
    }
}
