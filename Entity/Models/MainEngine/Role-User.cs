namespace Entities.Models.MainEngine
{
    public class Role_User
    {
        public int Id { get; set; }
        #region relations
        public int UserId { get; set; } // the relation of user
        public int RoleId { get; set; } //the relation of role
        public Role? Role { get; set; }
        #endregion
    }
}