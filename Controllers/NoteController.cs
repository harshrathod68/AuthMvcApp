using Microsoft.AspNetCore.Mvc;
using MyApps.Services;
using MyApps.Models;
using System.Text.Json;

namespace MyApps.Controllers
{
    public class NoteController : Controller
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        public IActionResult Index(string? category, string? search)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            var notes = _noteService.GetUserNotes(userId, category, search);
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.CurrentCategory = category;
            ViewBag.Search = search;
            
            return View(notes);
        }

        [HttpGet]
        public IActionResult Create(string category = "text")
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(new NoteCreateModel { Category = category });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NoteCreateModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Remove validation for fields that are not required based on category
            if (model.Category == "image")
            {
                ModelState.Remove("Content");
            }
            else if (model.Category == "link")
            {
                ModelState.Remove("Content");
            }
            else if (model.Category == "todo")
            {
                ModelState.Remove("Content");
            }

            if (!ModelState.IsValid) 
            {
                // Log validation errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                return View(model);
            }

            var note = new NoteModel
            {
                UserId = HttpContext.Session.GetString("UserId")!,
                Title = model.Title,
                Content = model.Content ?? "",
                Category = model.Category,
                Color = model.Color,
                ImageUrl = model.ImageUrl,
                LinkUrl = model.LinkUrl
            };

            if (!string.IsNullOrEmpty(model.TodoItemsJson))
            {
                note.TodoItems = JsonSerializer.Deserialize<List<TodoItem>>(model.TodoItemsJson);
            }

            _noteService.AddNote(note);
            TempData["Success"] = "Note created successfully!";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            var note = _noteService.GetNoteById(id, userId);
            
            if (note == null)
            {
                TempData["Error"] = "Note not found";
                return RedirectToAction(nameof(Index));
            }

            var model = new NoteEditModel
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Category = note.Category,
                Color = note.Color,
                IsPinned = note.IsPinned,
                ImageUrl = note.ImageUrl,
                LinkUrl = note.LinkUrl,
                TodoItemsJson = note.TodoItems != null ? JsonSerializer.Serialize(note.TodoItems) : null
            };

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NoteEditModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Remove validation for fields that are not required based on category
            if (model.Category == "image")
            {
                ModelState.Remove("Content");
            }
            else if (model.Category == "link")
            {
                ModelState.Remove("Content");
            }
            else if (model.Category == "todo")
            {
                ModelState.Remove("Content");
            }

            if (!ModelState.IsValid) 
            {
                // Log validation errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                return View(model);
            }

            var note = new NoteModel
            {
                Id = model.Id,
                UserId = HttpContext.Session.GetString("UserId")!,
                Title = model.Title,
                Content = model.Content ?? "",
                Category = model.Category,
                Color = model.Color,
                IsPinned = model.IsPinned,
                ImageUrl = model.ImageUrl,
                LinkUrl = model.LinkUrl
            };

            if (!string.IsNullOrEmpty(model.TodoItemsJson))
            {
                note.TodoItems = JsonSerializer.Deserialize<List<TodoItem>>(model.TodoItemsJson);
            }

            if (_noteService.UpdateNote(note))
            {
                TempData["Success"] = "Note updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update note";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            
            if (_noteService.DeleteNote(id, userId))
            {
                TempData["Success"] = "Note deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete note";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TogglePin(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            _noteService.TogglePin(id, userId);

            return RedirectToAction(nameof(Index));
        }

        private bool IsAuthenticated() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
    }
}
