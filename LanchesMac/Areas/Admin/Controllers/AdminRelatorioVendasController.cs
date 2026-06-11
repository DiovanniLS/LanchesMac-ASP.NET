using LanchesMac.Areas.Admin.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace LanchesMac.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminRelatorioVendasController : Controller
    {
        private readonly RelatorioVendasServico relatorioVendasServico;

        public AdminRelatorioVendasController(RelatorioVendasServico relatorioVendasServico)
        {
            this.relatorioVendasServico = relatorioVendasServico;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RelatorioVendasSimples(DateTime? minDate, DateTime? maxDate)
        {
            if (!minDate.HasValue) 
            {
                minDate = new DateTime(DateTime.Now.Year, 1, 1);
            }
            if (!maxDate.HasValue)
            {
                maxDate = DateTime.Now;
            }

            ViewData["minDate"] = minDate.Value.ToString("yyyy-mm-dd");
            ViewData["maxDate"] = maxDate.Value.ToString("yyyy-mm-dd");

            var result = await relatorioVendasServico.FindByDateAsync(minDate.Value, maxDate.Value);
            return View(result);
        }
    }
}
