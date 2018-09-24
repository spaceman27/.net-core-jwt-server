using Common.Standard.REST.Interfaces.API;
using System;
using System.Xml.Serialization;
using Vision.Core.REST.Auth.DTO;

namespace Vision.Core.REST.Auth.DTO
{
    /// <summary>
    /// The token to return to the client for security purposes.
    /// </summary>
    public class LoginToken : ILoginToken<UsersDTO>
    {
        /// <summary>
        /// The Bearer token to add to the header.
        /// </summary>
        public String Token { get; set; }
        
        /// <summary>
        /// User information.
        /// </summary>
        public UsersDTO User { get; set; } 
        
        /// <summary>
        /// The date and time that the bearertokenexpires.
        /// </summary>
        public DateTime Expiration { get; set; }

        public int MinutesValid { get; set; }
        
    }
}
