using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LanchesMac.Controllers
{
    public class PedidoController : Controller
    {
       private readonly IPedidoRepository _pedidoRepository;
        private readonly CarrinhoCompra _carrinhoCompra;

        public PedidoController(IPedidoRepository pedidoRepository, CarrinhoCompra carrinhoCompra)
        {
            _pedidoRepository = pedidoRepository;
            _carrinhoCompra = carrinhoCompra;
        }


       

        [HttpGet]
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(Pedido pedido)
        {
            int totalItensPedido = 0;
            decimal precoTotalPedido = 0.0m;

            //obtém os itens do carrinho de compra do cliente

            List<CarrinhoCompraItem> items = _carrinhoCompra.GetCarrinhoCompraItens();
            _carrinhoCompra.CarrinhoCompraItems = items;

            //veriica se tem itens no carrinho
            if (_carrinhoCompra.CarrinhoCompraItems.Count == 0)
            {
                ModelState.AddModelError("", "Seu carrinho está vazio, que tal adicionar algum lanche?");
            }

            //calcula o total de itens e o total do pedido
            foreach (var item in items)
            {
                totalItensPedido += item.Quantidade;
                precoTotalPedido += (item.Lanche.Preco * item.Quantidade);
            }

            //atribui os valores obtidos ao pedido
            pedido.TotalItensPedido = totalItensPedido;
            pedido.PedidoTotal = precoTotalPedido;


            //valida os dados do pedido
            if (ModelState.IsValid) 
            {
                //cria o pedido e os detalhes
                _pedidoRepository.CriarPedido(pedido);

                //define mensagem ao cliente
                ViewBag.CheckoutCompletoMensagem = "Obrigado por realizar seu pedido conosco!";
                ViewBag.TotalPedido = _carrinhoCompra.GetCarrinhoCompraTotal();

                //Limpa o carrinho do cliente
                _carrinhoCompra.LimparCarrinho();

                //Exibe a view com dados do cliente e do pedido

                return View("~/Views/Pedido/CheckoutCompleto.cshtml", pedido);
            }
            return View(pedido);
        }

        public IActionResult GerarPdf(int pedidoId)
        {
            var pedido = _pedidoRepository.GetPedidoById(pedidoId);

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text($"Pedido #{pedido.PedidoId}")
                        .FontSize(24)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Cliente: {pedido.Nome} {pedido.Sobrenome}");
                        col.Item().Text($"Data: {pedido.PedidoEnviado:dd/MM/yyyy HH:mm}");

                        col.Item().PaddingVertical(10);

                        foreach (var item in pedido.PedidoItens)
                        {
                            col.Item().Text(
                                $"{item.Quantidade}x {item.Lanche.Nome} - {item.Lanche.Preco:C}"
                            );
                        }

                        col.Item().PaddingTop(20);

                        col.Item().Text($"Total: {pedido.PedidoTotal:C}")
                            .FontSize(18)
                            .Bold();
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
