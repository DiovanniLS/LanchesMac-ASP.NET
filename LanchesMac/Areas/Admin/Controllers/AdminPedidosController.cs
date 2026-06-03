using LanchesMac.Context;
using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanchesMac.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminPedidosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPedidoRepository _pedidoRepository;

        public AdminPedidosController(AppDbContext context, IPedidoRepository pedidoRepository)
        {
            _context = context;
            _pedidoRepository = pedidoRepository;
        }

        // GET: Admin/AdminPedidoes
        public async Task<IActionResult> Index()
        {
              return View(await _context.Pedidos.ToListAsync());
        }

        // GET: Admin/AdminPedidoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(m => m.PedidoId == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // GET: Admin/AdminPedidoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminPedidoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PedidoId,Nome,Sobrenome,Endereco1,Endereco2,Cep,Cidade,Estado,Telefone,Email,PedidoEnviado,PedidoEntregueEm")] Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pedido);
        }

        // GET: Admin/AdminPedidoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }
            return View(pedido);
        }

        // POST: Admin/AdminPedidoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PedidoId,Nome,Sobrenome,Endereco1,Endereco2,Cep,Cidade,Estado,Telefone,Email,PedidoEnviado,PedidoEntregueEm")] Pedido pedido)
        {
            if (id != pedido.PedidoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.PedidoId))
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
            return View(pedido);
        }

        // GET: Admin/AdminPedidoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(m => m.PedidoId == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Admin/AdminPedidoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pedidos == null)
            {
                return Problem("Entity set 'AppDbContext.Pedidos'  is null.");
            }
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
          return _context.Pedidos.Any(e => e.PedidoId == id);
        }

        public IActionResult GerarPdf(int pedidoId)
        {
            var pedido = _pedidoRepository.GetPedidoById(pedidoId);

            if (pedido == null)
            {
                return NotFound();
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("🍔 LanchesMac")
                            .FontSize(26)
                            .Bold();

                        col.Item().Text($"Pedido #{pedido.PedidoId}");
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Produto").Bold();
                                header.Cell().Text("Qtd").Bold();
                                header.Cell().Text("Preço").Bold();
                            });

                            foreach (var item in pedido.PedidoItens)
                            {
                                table.Cell().Text(item.Lanche.Nome);
                                table.Cell().Text(item.Quantidade.ToString());
                                table.Cell().Text(item.Lanche.Preco.ToString("C"));
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Pedido-{pedido.PedidoId}.pdf"
            );
        }
    }
}
