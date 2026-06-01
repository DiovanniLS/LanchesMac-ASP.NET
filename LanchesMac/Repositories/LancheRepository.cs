using LanchesMac.Context;
using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LanchesMac.Repositories
{
    public class LancheRepository : ILancheRepository
    {
        private readonly AppDbContext _context;
        public LancheRepository(AppDbContext contexto)
        {
            _context = contexto;
        }

        public IEnumerable<Lanche> Lanches => _context.Lanches.Include(c => c.Categoria);

        public IEnumerable<Lanche> LanchesPreferidos => _context.Lanches.
                                   Where(l => l.IsLanchePreferido)
                                  .Include(c => c.Categoria);

        public Lanche GetLancheById(int lancheId)
        {
            return _context.Lanches.FirstOrDefault(l => l.LancheId == lancheId);
        }

        public IEnumerable<Lanche> BuscarLanches(string pesquisa)
        {
            return _context.Lanches
                .Include(c => c.Categoria)
                .Where(l =>
                    l.Nome.Contains(pesquisa) ||
                    l.DescricaoCurta.Contains(pesquisa));
        }

        public IEnumerable<Lanche> FiltrarPorCategoria(string categoria)
        {
            return _context.Lanches
                .Include(c => c.Categoria)
                .Where(l => l.Categoria.CategoriaNome == categoria);
        }
    }
}
