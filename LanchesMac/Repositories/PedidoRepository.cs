using LanchesMac.Context;
using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LanchesMac.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly CarrinhoCompra _carrinhoCompra;

        public PedidoRepository(AppDbContext appDbContext, CarrinhoCompra carrinhoCompra)
        {
            _appDbContext = appDbContext;
            _carrinhoCompra = carrinhoCompra;
        }

        void IPedidoRepository.CriarPedido(Pedido pedido)
        {
            pedido.PedidoEnviado = DateTime.Now;
            _appDbContext.Pedidos.Add(pedido);
            _appDbContext.SaveChanges();

            var carrinhoCompraItens = _carrinhoCompra.CarrinhoCompraItems;

            foreach (var carrinhoItem in carrinhoCompraItens) 
            {
                var pedidoDetail = new PedidoDetalhe()
                {
                    Quantidade = carrinhoItem.Quantidade,
                    LancheId = carrinhoItem.Lanche.LancheId,
                    PedidoId = pedido.PedidoId,
                    Preco = carrinhoItem.Lanche.Preco
                };
                _appDbContext.PedidosDetalhe.Add(pedidoDetail);
            }

            foreach (var item in _carrinhoCompra.CarrinhoCompraItems)
            {
                item.Lanche.QuantidadeEstoque -= item.Quantidade;

                if (item.Lanche.QuantidadeEstoque <= 0)
                {
                    item.Lanche.QuantidadeEstoque = 0;
                    item.Lanche.EmEstoque = false;
                }
            }

            _appDbContext.SaveChanges();
        }

        public Pedido GetPedidoById(int pedidoId)
        {
            return _appDbContext.Pedidos
                .Include(p => p.PedidoItens)
                .ThenInclude(pi => pi.Lanche)
                .FirstOrDefault(p => p.PedidoId == pedidoId);
        }
    }
}
