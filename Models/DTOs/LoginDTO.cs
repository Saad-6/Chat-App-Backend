namespace Chat_App.Models.DTOs
{
    public class LoginRequestDTO
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class LoginResponseDTO
    {
        public string? Email { get; set; }
        public string JWT { get; set; }
    }
}
