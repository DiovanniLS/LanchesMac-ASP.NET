using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using LanchesMac.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LanchesMac.Controllers
{
    public class LancheController : Controller
    {
        private readonly ILancheRepository _lancheRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        public LancheController(ILancheRepository lancheRepository,ICategoriaRepository categoriaRepository)
        {
            _lancheRepository = lancheRepository;
            _categoriaRepository = categoriaRepository;
        }

        public IActionResult List(string categoria, string searchString)
        {
            IEnumerable<Lanche> lanches;
            string categoriaAtual = string.Empty;

            if (!string.IsNullOrEmpty(searchString))
            {
                lanches = _lancheRepository.BuscarLanches(searchString);

                categoriaAtual = "Resultado da Pesquisa";
            }
            else if (!string.IsNullOrEmpty(categoria))
            {
                lanches = _lancheRepository.FiltrarPorCategoria(categoria);

                categoriaAtual = categoria;
            }
            else
            {
                lanches = _lancheRepository.Lanches;

                categoriaAtual = "Todos os Lanches";
            }

            var lancheListViewModel = new LancheListViewModel
            {
                Lanches = lanches,
                CategoriaAtual = categoriaAtual,
                Categorias = _categoriaRepository.Categorias
            };

            return View(lancheListViewModel);
        }

        public IActionResult Details(int lancheId)
        {
            var lanche = _lancheRepository.GetLancheById(lancheId);

            return View(lanche);
        }

    }
}
