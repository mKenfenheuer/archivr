using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Archivr.UI.Data;
using Archivr.UI.Models;
using Archivr.PrintingServer.Controllers;
using Archivr.PrintingServer.Model;
using System.Configuration;
using Wkhtmltopdf.NetCore;
using QRCoder;
using System.Text.Json;

namespace Archivr.UI.Controllers.Archive
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeneratePdf _generatePdf;
        private readonly IConfiguration _configuration;

        public ItemsController(ApplicationDbContext context, GeneratePdf generatePdf, IConfiguration configuration)
        {
            _context = context;
            _generatePdf = generatePdf;
            _configuration = configuration;
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ArchiveItems.Include(a => a.Parent);
            return View(await applicationDbContext.ToListAsync());
        }

        [HttpGet("Import")]
        public IActionResult Import()
        {
            return View();
        }
        

        [HttpPost("Import")]
        public async Task<IActionResult> Import([FromForm] IFormFileCollection files)
        {
            foreach (var file in files)
            {
                string str = new StreamReader(file.OpenReadStream()).ReadToEnd();
                ArchiveItem[]? items = JsonSerializer.Deserialize<ArchiveItem[]>(str);

                if (items == null || items.Length == 0)
                    continue;
                
                foreach (var item in items)
                {
                    var existing = await _context.ArchiveItems.FirstOrDefaultAsync(a => a.Id == item.Id);
                    if (existing != null)
                    {
                        existing.ParentId = item.ParentId;
                        existing.Name = item.Name;
                        existing.Description = item.Description;
                        existing.ItemType = item.ItemType;
                        _context.Update(existing);
                    }
                    else 
                    {
                        _context.Add(item);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ArchiveItems == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            var archiveItem = await _context.ArchiveItems
                .Include(a => a.Parent)
                .Include(a => a.Children)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (archiveItem == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            return View(archiveItem);
        }

        public IActionResult UnknownItem(string id)
        {
            return View(nameof(UnknownItem), id);
        }

        public async Task<IActionResult> Search([FromForm] string search)
        {
            var items = await _context.ArchiveItems
                .Include(a => a.Parent)
                .Include(a => a.Children)
            .ToListAsync();
            items = items.Where(i => i.Name.ToLower().Contains(search.ToLower()) ||
                                        i.Description.ToLower().Contains(search.ToLower()) ||
                                        i.Id.ToLower().Contains(search.ToLower())).ToList();
            return View(items);
        }

        // GET: Items/Create
        public IActionResult Create(string? id)
        {
            return View(new ArchiveItem()
            {
                ParentId = id,
            });
        }

        public async Task<IActionResult> PrintItemLabel(string id)
        {
            if (id == null || _context.ArchiveItems == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            var archiveItem = await _context.ArchiveItems.FindAsync(id);
            if (archiveItem == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            Label label = new Label()
            {
                Code = archiveItem.Id,
                Name = archiveItem.Name,
                Description = "Archivr Archiv-Item. Weitere Informationen siehe QR-Code.",
                QrCodeLink = $"{Request.Scheme}://{Request.Host}/Items/Details/{archiveItem.Id}"
            };

            var pdf = await LabelsController.GenerateLabel(_generatePdf, label, "Horizontal");
            var success = await LabelsController.PrintPdf(_configuration, pdf);

            return RedirectToAction(nameof(Details), new { Id = id });
        }

        public async Task<IActionResult> PreviewItemLabel(string id)
        {
            if (id == null || _context.ArchiveItems == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            var archiveItem = await _context.ArchiveItems.FindAsync(id);
            if (archiveItem == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            Label label = new Label()
            {
                Code = archiveItem.Id,
                Name = archiveItem.Name,
                Description = "Archivr Archiv-Item. Weitere Informationen siehe QR-Code.",
                QrCodeLink = $"{Request.Scheme}://{Request.Host}/Items/Details/{archiveItem.Id}"
            };

            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(label.QrCodeLink ?? label.Code, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode QrCode = new BitmapByteQRCode(QrCodeInfo);
            byte[] BitmapArray = QrCode.GetGraphic(10);
            label.QrCodeImage = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));

            return View($"Views/Labels/Horizontal.cshtml", label);
        }

        public async Task<IActionResult> DownloadItemLabel(string id)
        {
            if (id == null || _context.ArchiveItems == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            var archiveItem = await _context.ArchiveItems.FindAsync(id);
            if (archiveItem == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            Label label = new Label()
            {
                Code = archiveItem.Id,
                Name = archiveItem.Name,
                Description = "Archivr Archiv-Item. Weitere Informationen siehe QR-Code.",
                QrCodeLink = $"{Request.Scheme}://{Request.Host}/Items/Details/{archiveItem.Id}"
            };

            var pdf = await LabelsController.GenerateLabel(_generatePdf, label, "Horizontal");
            return new FileContentResult(pdf, "application/pdf");
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ItemType,ParentId")] ArchiveItem archiveItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(archiveItem);
                await _context.SaveChangesAsync();
                if (archiveItem.ParentId != null)
                    return RedirectToAction(nameof(Details), new { Id = archiveItem.ParentId });
                return RedirectToAction(nameof(Index));
            }
            return View(archiveItem);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ArchiveItems == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            var archiveItem = await _context.ArchiveItems.FindAsync(id);
            if (archiveItem == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }
            ViewData["ParentId"] = new SelectList(_context.ArchiveItems, "Id", "Id", archiveItem.ParentId);
            return View(archiveItem);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Description,ItemType,ParentId")] ArchiveItem archiveItem)
        {
            if (id != archiveItem.Id)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(archiveItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArchiveItemExists(archiveItem.Id))
                    {
                        return RedirectToAction(nameof(UnknownItem), new { Id = id });
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { Id = id });
            }
            return View(archiveItem);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ArchiveItems == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            var archiveItem = await _context.ArchiveItems
                .Include(a => a.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (archiveItem == null)
            {
                return RedirectToAction(nameof(UnknownItem), new { Id = id });
            }

            return View(archiveItem);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ArchiveItems == null)
            {
                return Problem("Entity set 'ApplicationDbContext.ArchiveItems'  is null.");
            }
            var archiveItem = await _context.ArchiveItems.FindAsync(id);
            if (archiveItem != null)
            {
                _context.ArchiveItems.Remove(archiveItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArchiveItemExists(string id)
        {
            return (_context.ArchiveItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
