/*
 * =====================================================
 * NoteService.cs - Notes CRUD Service
 * =====================================================
 * 
 * Ye service Notes ka data manage karti hai.
 * Data JSON file mein store hota hai: Data/notes.json
 * 
 * CRUD Operations:
 * - Create (Add new note)
 * - Read (Get notes list)
 * - Update (Edit note)
 * - Delete (Remove note)
 * 
 * Extra Features:
 * - Pin/Unpin notes
 * - Filter by category
 * - Search notes
 * 
 * Author: Harsh Rathod
 * =====================================================
 */

using System.Text.Json;
using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    public class NoteService : INoteService
    {
        // JSON file ka path (Data/notes.json)
        private readonly string _filePath;
        
        // Lock object - Multiple requests ek saath file access na karein
        // Thread safety ke liye zaroori hai
        private readonly object _lock = new();

        // Constructor - File path set karo
        public NoteService(IWebHostEnvironment env)
        {
            // ContentRootPath = Project folder ka path
            _filePath = Path.Combine(env.ContentRootPath, "Data", "notes.json");
            
            // Agar file nahi hai to create karo
            EnsureFileExists();
        }

        /// <summary>
        /// Agar notes.json file nahi hai to empty array ke saath create karo
        /// </summary>
        private void EnsureFileExists()
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]"); // Empty JSON array
            }
        }

        /// <summary>
        /// JSON file se sab notes read karo
        /// </summary>
        private List<NoteModel> ReadNotes()
        {
            // lock = Ek time par ek hi thread file access kare
            lock (_lock)
            {
                // File read karo
                var json = File.ReadAllText(_filePath);
                
                // JSON ko List<NoteModel> mein convert karo
                return JsonSerializer.Deserialize<List<NoteModel>>(json) ?? new List<NoteModel>();
            }
        }

        /// <summary>
        /// Notes list ko JSON file mein save karo
        /// </summary>
        private void SaveNotes(List<NoteModel> notes)
        {
            lock (_lock)
            {
                // List ko JSON string mein convert karo
                // WriteIndented = true means formatted JSON (readable)
                var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                // File mein write karo
                File.WriteAllText(_filePath, json);
            }
        }

        // ========================================
        // READ - Notes List Get Karo
        // ========================================
        
        /// <summary>
        /// User ki sab notes lo, optional filters ke saath
        /// </summary>
        /// <param name="userId">User ka ID</param>
        /// <param name="category">Category filter (optional)</param>
        /// <param name="search">Search text (optional)</param>
        public List<NoteModel> GetUserNotes(string userId, string? category = null, string? search = null)
        {
            // Step 1: Sab notes read karo
            var notes = ReadNotes()
                // Step 2: Sirf is user ki notes filter karo
                .Where(n => n.UserId == userId)
                // Step 3: Pehle pinned notes, phir latest notes
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();

            // Step 4: Category filter (agar diya hai)
            if (!string.IsNullOrEmpty(category))
            {
                notes = notes.Where(n => n.Category == category).ToList();
            }

            // Step 5: Search filter (agar diya hai)
            if (!string.IsNullOrEmpty(search))
            {
                notes = notes.Where(n => 
                    // Title ya Content mein search karo (case insensitive)
                    n.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    n.Content.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return notes;
        }

        /// <summary>
        /// Single note ID se get karo
        /// </summary>
        public NoteModel? GetNoteById(int id, string userId)
        {
            // FirstOrDefault = Pehla match return karo, ya null
            return ReadNotes().FirstOrDefault(n => n.Id == id && n.UserId == userId);
        }

        // ========================================
        // CREATE - New Note Add Karo
        // ========================================
        
        /// <summary>
        /// Naya note add karo
        /// </summary>
        public void AddNote(NoteModel note)
        {
            // Step 1: Existing notes read karo
            var notes = ReadNotes();
            
            // Step 2: New ID generate karo
            // Agar notes hain to max ID + 1, warna 1
            note.Id = notes.Any() ? notes.Max(n => n.Id) + 1 : 1;
            
            // Step 3: Created time set karo
            note.CreatedAt = DateTime.Now;
            
            // Step 4: List mein add karo
            notes.Add(note);
            
            // Step 5: File mein save karo
            SaveNotes(notes);
        }

        // ========================================
        // UPDATE - Note Edit Karo
        // ========================================
        
        /// <summary>
        /// Existing note update karo
        /// </summary>
        /// <returns>true if updated, false if not found</returns>
        public bool UpdateNote(NoteModel note)
        {
            var notes = ReadNotes();
            
            // Note dhundho
            var existing = notes.FirstOrDefault(n => n.Id == note.Id && n.UserId == note.UserId);
            
            // Agar nahi mila
            if (existing == null) 
                return false;

            // Sab fields update karo
            existing.Title = note.Title;
            existing.Content = note.Content;
            existing.Category = note.Category;
            existing.Color = note.Color;
            existing.IsPinned = note.IsPinned;
            existing.ImageUrl = note.ImageUrl;
            existing.LinkUrl = note.LinkUrl;
            existing.TodoItems = note.TodoItems;
            existing.UpdatedAt = DateTime.Now; // Update time set karo

            // Save karo
            SaveNotes(notes);
            return true;
        }

        // ========================================
        // DELETE - Note Remove Karo
        // ========================================
        
        /// <summary>
        /// Note delete karo
        /// </summary>
        /// <returns>true if deleted, false if not found</returns>
        public bool DeleteNote(int id, string userId)
        {
            var notes = ReadNotes();
            
            // Note dhundho
            var note = notes.FirstOrDefault(n => n.Id == id && n.UserId == userId);
            
            if (note == null) 
                return false;

            // List se remove karo
            notes.Remove(note);
            
            // Save karo
            SaveNotes(notes);
            return true;
        }

        // ========================================
        // PIN/UNPIN - Note Pin Toggle Karo
        // ========================================
        
        /// <summary>
        /// Note ka pin status toggle karo (pinned ↔ unpinned)
        /// </summary>
        public bool TogglePin(int id, string userId)
        {
            var notes = ReadNotes();
            
            var note = notes.FirstOrDefault(n => n.Id == id && n.UserId == userId);
            
            if (note == null) 
                return false;

            // Toggle: true → false, false → true
            note.IsPinned = !note.IsPinned;
            
            SaveNotes(notes);
            return true;
        }
    }
}
