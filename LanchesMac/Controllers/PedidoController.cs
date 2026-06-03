using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
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



        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            return View();
        }

        [Authorize]
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

                foreach (var item in items)
                {
                    item.Lanche.QuantidadeEstoque -= item.Quantidade;

                    if (item.Lanche.QuantidadeEstoque <= 0)
                    {
                        item.Lanche.QuantidadeEstoque = 0;
                        item.Lanche.EmEstoque = false;
                    }
                }


                return View("~/Views/Pedido/CheckoutCompleto.cshtml", pedido);
            }
            return View(pedido);
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
