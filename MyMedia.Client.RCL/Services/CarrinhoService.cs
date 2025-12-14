using MyMedia.Data.Entities;

namespace MyMedia.Client.RCL.Services
{
    public class CarrinhoItem
    {
        public Produto Produto { get; set; } = new();
        public int Quantidade { get; set; }
    }

    public class CarrinhoService
    {
        private List<CarrinhoItem> _items = new();
        public event Action? OnChange;

        public void Adicionar(Produto produto)
        {
            var itemExistente = _items.FirstOrDefault(x => x.Produto.Id == produto.Id);

            if (itemExistente != null)
            {
                // SÓ AUMENTA SE A QUANTIDADE ATUAL FOR MENOR QUE O STOCK
                if (itemExistente.Quantidade < produto.Stock)
                {
                    itemExistente.Quantidade++;
                }
            }
            else
            {
                // Só adiciona se houver pelo menos 1 em stock
                if (produto.Stock > 0)
                {
                    _items.Add(new CarrinhoItem { Produto = produto, Quantidade = 1 });
                }
            }
            NotificarMudanca();
        }

        public void Remover(Produto produto)
        {
            var item = _items.FirstOrDefault(x => x.Produto.Id == produto.Id);
            if (item != null)
            {
                _items.Remove(item);
                NotificarMudanca();
            }
        }

        public void AtualizarQuantidade(Produto produto, int novaQuantidade)
        {
            var item = _items.FirstOrDefault(x => x.Produto.Id == produto.Id);
            if (item != null)
            {
                if (novaQuantidade <= 0)
                {
                    _items.Remove(item);
                }
                // SE TENTAR POR MAIS QUE O STOCK, FIXAMOS NO MÁXIMO DO STOCK
                else if (novaQuantidade > produto.Stock)
                {
                    item.Quantidade = produto.Stock;
                }
                else
                {
                    item.Quantidade = novaQuantidade;
                }
                NotificarMudanca();
            }
        }

        public List<CarrinhoItem> GetItems() => _items;
        public decimal Total() => _items.Sum(x => x.Produto.PrecoFinal * x.Quantidade);
        public int ContarItens() => _items.Sum(x => x.Quantidade);

        private void NotificarMudanca() => OnChange?.Invoke();
    }
}                        