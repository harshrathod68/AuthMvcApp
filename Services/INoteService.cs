using MyApps.Models;

namespace MyApps.Services
{
    public interface INoteService
    {
        List<NoteModel> GetUserNotes(string userId, string? category = null, string? search = null);
        NoteModel? GetNoteById(int id, string userId);
        void AddNote(NoteModel note);
        bool UpdateNote(NoteModel note);
        bool DeleteNote(int id, string userId);
        bool TogglePin(int id, string userId);
    }
}
