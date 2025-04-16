using System.ComponentModel.DataAnnotations;

namespace NET_API.Models;

public class UserModel
{
  // userid
  [Key]
  public string UserId {get; set;}
  // user_name
  public string UserName {get; set;}
  // password
  public string Password {get; set;}
  // email
  public string email {get; set;}
  // create_date
  public DateTime  CreateDate {get; set;}
}

public class RoleModel
{
  // role id
  [Key]
  public string RoleId {get; set; }
  // role display name
  public string RoleName {get; set; }
}

public class UserHasRole
{
  // role id
  [Key]
  public string RoleId {get; set;}
  // user id
  public string UserId {get; set;}
  // create_date
  public DateTime CreateDate {get; set;}
}