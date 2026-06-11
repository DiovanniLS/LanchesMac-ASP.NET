using LanchesMac.Context;
using LanchesMac.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace LanchesMac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminLanchesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AdminLanchesController(
    AppDbContext context,
    IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Admin/AdminLanches
        public IActionResult Index(string filtro, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var lanches = _context.Lanches
                .Include(l => l.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                lanches = lanches.Where(l =>
                    l.Nome.Contains(filtro) ||
                    l.DescricaoCurta.Contains(filtro) ||
                    l.Categoria.CategoriaNome.Contains(filtro));
            }

            return View(
                lanches
                    .OrderBy(l => l.Nome)
                    .ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/AdminLanches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Lanches == null)
            {
                return NotFound();
            }

            var lanche = await _context.Lanches
                .Include(l => l.Categoria)
                .FirstOrDefaultAsync(m => m.LancheId == id);
            if (lanche == null)
            {
                return NotFound();
            }

            return View(lanche);
        }

        // GET: Admin/AdminLanches/Create
        // GET: Admin/AdminLanches/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias,
                "CategoriaId",
                "CategoriaNome");

            CarregarImagens();

            return View();
        }

        // POST: Admin/AdminLanches/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lanche lanche,IFormFile? novaImagem,IFormFile? novaThumbnail)
        {
            if (ModelState.IsValid)
            {
                var imagem = await SalvarImagem(novaImagem);
                var thumbnail = await SalvarImagem(novaThumbnail);

                if (!string.IsNullOrEmpty(imagem))
                    lanche.ImagemUrl = imagem;

                if (!string.IsNullOrEmpty(thumbnail))
                    lanche.ImagemThumbnailUrl = thumbnail;

                _context.Add(lanche);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias,
                "CategoriaId",
                "CategoriaNome",
                lanche.CategoriaId);

            CarregarImagens();

            return View(lanche);
        }

        // GET: Admin/AdminLanches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lanche = await _context.Lanches.FindAsync(id);

            if (lanche == null)
            {
                return NotFound();
            }

            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias,
                "CategoriaId",
                "CategoriaNome",
                lanche.CategoriaId);

            CarregarImagens();

            return View(lanche);
        }

        // POST: Admin/AdminLanches/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LancheId,Nome,DescricaoCurta,DescricaoDetalhada,Preco,ImagemUrl,ImagemThumbnailUrl,IsLanchePreferido,EmEstoque,QuantidadeEstoque,CategoriaId")] Lanche lanche)
        {
            if (id != lanche.LancheId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (lanche.QuantidadeEstoque <= 0)
                    {
                        lanche.EmEstoque = false;
                        lanche.QuantidadeEstoque = 0;
                    }
                    else
                    {
                        lanche.EmEstoque = true;
                    }

                    _context.Update(lanche);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LancheExists(lanche.LancheId))
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
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "CategoriaNome", lanche.CategoriaId);

            CarregarImagens();
            return View(lanche);
        }

        // GET: Admin/AdminLanches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Lanches == null)
            {
                return NotFound();
            }

            var lanche = await _context.Lanches
                .Include(l => l.Categoria)
                .FirstOrDefaultAsync(m => m.LancheId == id);
            if (lanche == null)
            {
                return NotFound();
            }

            return View(lanche);
        }

        // POST: Admin/AdminLanches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Lanches == null)
            {
                return Problem("Entity set 'AppDbContext.Lanches'  is null.");
            }
            var lanche = await _context.Lanches.FindAsync(id);
            if (lanche != null)
            {
                _context.Lanches.Remove(lanche);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LancheExists(int id)
        {
          return _context.Lanches.Any(e => e.LancheId == id);
        }

        private void CarregarImagens()
        {
            var pastaImagens = Path.Combine(
                _hostingEnvironment.WebRootPath,
                "images/produtos");

            ViewBag.Imagens = Directory.Exists(pastaImagens)
                ? Directory.GetFiles(pastaImagens)
                    .Select(Path.GetFileName)
                    .ToList()
                : new List<string>();
        }


        private async Task<string?> SalvarImagem(IFormFile? arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return null;

            var pastaImagens = Path.Combine(
                _hostingEnvironment.WebRootPath,
                "images/produtos");

            Directory.CreateDirectory(pastaImagens);

            var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(arquivo.FileName)}";

            var caminhoCompleto = Path.Combine(
                pastaImagens,
                nomeArquivo);

            using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return nomeArquivo;
        }
    }
}
