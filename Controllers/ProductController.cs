using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sky.Data;
using Sky.Models;

namespace Sky.Controllers
{
    public class ProductController : Controller
    {
        const int USER_PER_PAGE = 2;
        private readonly SkyAppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(SkyAppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        public string CurrentSort { get; set; }
        public async Task<IActionResult> Nam(string searchString, string sortOrder, int PageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var skyAppDbContext = _context.ProductDbSet.Where(p => p.Category.CategoryName == "Nam" && p.ProductStatus == true);

            if (!String.IsNullOrEmpty(searchString))
            {
                skyAppDbContext = skyAppDbContext.Where(s => s.ProductName.Contains(searchString) || s.ProductDescription.Contains(searchString));

            }

            if (!skyAppDbContext.Any())
            {
                ViewBag.Message = "Không tìm thấy từ khóa '" + searchString + "'";
            }

            if (PageNumber == 0)
            {
                PageNumber = 1;
            }

            ViewData["PageNumber"] = PageNumber;

            int totalProducts = await skyAppDbContext.CountAsync();

            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalProducts / USER_PER_PAGE);

            skyAppDbContext = skyAppDbContext.Include(p => p.Category).Include(p => p.Type);

            return View(await skyAppDbContext.Skip(USER_PER_PAGE * ((int)ViewData["PageNumber"] - 1)).Take(USER_PER_PAGE).AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.ProductDbSet
                .Include(p => p.Category)
                .Include(p => p.Type)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            product.ViewCount += 1;

            _context.Update(product);
            await _context.SaveChangesAsync();

            return View(product);
        }







        //-------------------------------------------------------------------------------------------------------------------------




        // GET: Product
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int PageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            CurrentSort = sortOrder;
            ViewData["ProductNameSort"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["ProductDateSort"] = sortOrder == "date" ? "date_desc" : "date";

            var skyAppDbContext = from a in _context.ProductDbSet select a;
            if (!String.IsNullOrEmpty(searchString))
            {
                skyAppDbContext = skyAppDbContext.Where(s => s.ProductName.Contains(searchString) || s.ProductDescription.Contains(searchString));

            }

            if (!skyAppDbContext.Any())
            {
                ViewBag.Message = "Không tìm thấy từ khóa '" + searchString + "'";
            }

            switch (sortOrder)
            {
                case "name_desc":
                    skyAppDbContext = skyAppDbContext.OrderByDescending(s => s.ProductName);
                    break;
                case "ate":
                    skyAppDbContext = skyAppDbContext.OrderBy(s => s.ProductDate);
                    break;
                case "date_desc":
                    skyAppDbContext = skyAppDbContext.OrderByDescending(s => s.ProductDate);
                    break;
                default:
                    skyAppDbContext = skyAppDbContext.OrderBy(s => s.ProductName);
                    break;
            }

            if (PageNumber == 0)
            {
                PageNumber = 1;
            }

            ViewData["PageNumber"] = PageNumber;

            int totalProducts = await skyAppDbContext.CountAsync();

            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalProducts / USER_PER_PAGE);

            skyAppDbContext = skyAppDbContext.Include(p => p.Category).Include(p => p.Type);

            return View(await skyAppDbContext.Skip(USER_PER_PAGE * ((int)ViewData["PageNumber"] - 1)).Take(USER_PER_PAGE).AsNoTracking().ToListAsync());
        }

        // GET: Product/Details/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.ProductDbSet
                .Include(p => p.Category)
                .Include(p => p.Type)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.CategorieDbSet, "CategoryId", "CategoryName");
            ViewData["TypeId"] = new SelectList(_context.TypeDbSet, "TypeId", "TypeName");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductImageFile,ProductDescription,ProductPrice,PreviousPrice,ViewCount,ProductDate,ProductStatus,CategoryId,TypeId")] Product product)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(product.ProductImageFile.FileName);
                string extension = Path.GetExtension(product.ProductImageFile.FileName);
                product.ProductImage = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwRootPath + "/Image/", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await product.ProductImageFile.CopyToAsync(fileStream);
                }
                product.ProductDate = DateTime.Now;
                product.ViewCount = 1;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.CategorieDbSet, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["TypeId"] = new SelectList(_context.TypeDbSet, "TypeId", "TypeName", product.TypeId);
            return View(product);
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.ProductDbSet.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.CategorieDbSet, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["TypeId"] = new SelectList(_context.TypeDbSet, "TypeId", "TypeName", product.TypeId);
            return View(product);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductImage,ProductImageFile,ProductDescription,ProductPrice,PreviousPrice,ViewCount,ProductDate,ProductStatus,CategoryId,TypeId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    try
                    {
                        

                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(product.ProductImageFile.FileName);
                        string extension = Path.GetExtension(product.ProductImageFile.FileName);

                        var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "image", product.ProductImage);
                        if (System.IO.File.Exists(imagePath))
                            System.IO.File.Delete(imagePath);

                        product.ProductImage = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "/Image/", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await product.ProductImageFile.CopyToAsync(fileStream);
                        }

                        //delete image from wwwroot/image
                        
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message = e.ToString();
                    }
                    

                    
                    
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.CategorieDbSet, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["TypeId"] = new SelectList(_context.TypeDbSet, "TypeId", "TypeName", product.TypeId);
            return View(product);
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.ProductDbSet
                .Include(p => p.Category)
                .Include(p => p.Type)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.ProductDbSet.FindAsync(id);

            //delete image from wwwroot/image
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "image", product.ProductImage);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            _context.ProductDbSet.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool ProductExists(int id)
        {
            return _context.ProductDbSet.Any(e => e.ProductId == id);
        }
    }
}
