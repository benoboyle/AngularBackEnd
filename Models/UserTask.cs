public class UserTask
{
    public int TaskId { get; set; } // Auto-incrementing ID
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Planned"; // Default value
    public string Username { get; set; } // Assigned from latest login
}
