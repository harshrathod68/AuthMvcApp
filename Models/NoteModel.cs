using System.ComponentModel.DataAnnotations;

namespace MyApps.Models
{
    public class NoteModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = "";
        
        public string Content { get; set; } = "";
        
        [Required]
        public string Category { get; set; } = "text";
        
        public string Color { get; set; } = "#ffffff";
        public bool IsPinned { get; set; } = false;
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public List<TodoItem>? TodoItems { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public class TodoItem
    {
        public string Text { get; set; } = "";
        public bool IsCompleted { get; set; } = false;
    }

    public class NoteCreateModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = "";
        
        public string Content { get; set; } = "";
        
        [Required]
        public string Category { get; set; } = "text";
        
        public string Color { get; set; } = "#667eea";
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? TodoItemsJson { get; set; }
    }

    public class NoteEditModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = "";
        
        public string Content { get; set; } = "";
        
        [Required]
        public string Category { get; set; } = "text";
        
        public string Color { get; set; } = "#667eea";
        public bool IsPinned { get; set; }
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? TodoItemsJson { get; set; }
    }

    public class NoteFilterModel
    {
        public string? Category { get; set; }
        public string? Search { get; set; }
    }
}
