using LanchesMac.Models;

namespace LanchesMac.Repositories.Interfaces
{
    public interface IPedidoRepository
    {
        void CriarPedido(Pedido pedido);

        Pedido GetPedidoById(int pedidoId);
    }
}
