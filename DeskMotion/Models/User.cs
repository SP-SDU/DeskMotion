using Microsoft.AspNetCore.Identity;
using DeskMotion;
using System.Reflection.Metadata.Ecma335;

namespace DeskMotion.Models
{
    public class User
    { 
        public int UserId { get; set;}
        public string Name { get; set;}
        public string Email { get; set;}
        public string HashedPassword { get; set;}
        public Role Role { get; set;} 
    }
}