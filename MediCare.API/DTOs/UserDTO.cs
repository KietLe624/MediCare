using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class UserDTO
    {
        // Admin và Patient
        public class UserQueryParams
        {
            public string? Search { get; set; }
            public string? Role { get; set; }
            public string? IsActive { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string? SortBy { get; set; }
            public string? SortOrder { get; set; } = "desc";
        }

        // Request
        public class UpdateProfileRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
            [Required, Phone]
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public class UpdateUserRoleRequest
        {
            [Required]
            public string Role { get; set; } = string.Empty;

        }
        
        public class UpdateUserStatusRequest
        {
            [Required]
            public bool IsActive { get; set; }
        }

        // Response
        public class UserResponse
        {
            public long Id { get; set; }
            public string UserName { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public string? PhoneNumber { get; set; }
            public bool IsActive { get; set; }  
            public DateTime CreatedAt { get; set; }
            public string Role { get; set; } = default!;
        }
        public class UserSummaryResponse
        {
            public long Id { get; set; }
            public string UserName { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public bool IsActive { get; set; }
            public string Role { get; set; } = default!;
        }

        public class PagedResponse<T>
        {
            public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        }
    }
}
