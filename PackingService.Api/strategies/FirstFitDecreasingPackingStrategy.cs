using PackingService.Api.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace PackingService.Api.Strategies
{
    public class FirstFitDecreasingPackingStrategy : IPackingStrategy
    {
        public List<PackedBoxDTO> Pack(
            IEnumerable<ProductDTO> products,
            IEnumerable<BoxDTO> availableBoxes
        )
        {
            var openBoxes = new List<(BoxDTO Box, List<ProductDTO> Items, decimal UsedVolume, string? Observacao)>();

            var boxTypes = availableBoxes
                .Select(b => (Box: b, Volume: GetVolume(b)))
                .OrderBy(x => x.Volume)
                .ToList();

            foreach (var prod in products.OrderByDescending(GetVolume))
            {
                var volProd = GetVolume(prod);
                var placed = false;

                for (int i = 0; i < openBoxes.Count; i++)
                {
                    var (box, items, usedVol, obs) = openBoxes[i];
                    if (usedVol + volProd <= GetVolume(box) && FitsInside(prod, box))
                    {
                        items.Add(prod);
                        usedVol += volProd;
                        openBoxes[i] = (box, items, usedVol, obs);
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    var candidate = boxTypes
                        .FirstOrDefault(bt => bt.Volume >= volProd && FitsInside(prod, bt.Box))
                        .Box;

                    if (candidate == null)
                    {

                        openBoxes.Add((
                            new BoxDTO
                            {
                                BoxType = "N/A",
                                Height = prod.Height,
                                Width = prod.Width,
                                Length = prod.Length
                            },
                            new List<ProductDTO> { prod },
                            volProd,
                            $"Produto '{prod.Name}' não cabe em nenhuma das caixas disponíveis."
                        ));
                    }
                    else
                    {
                        openBoxes.Add((candidate, new List<ProductDTO> { prod }, volProd, null));
                    }
                }
            }

            return openBoxes
                .Select(t => new PackedBoxDTO
                {
                    BoxType = t.Box.BoxType,
                    Products = t.Items.Select(p => p.Name).ToList(),
                    Observacao = t.Observacao
                })
                .ToList();
        }

        private static decimal GetVolume(ProductDTO p) => p.Height * p.Width * p.Length;
        private static decimal GetVolume(BoxDTO b) => b.Height * b.Width * b.Length;

        private static bool FitsInside(ProductDTO p, BoxDTO b) =>
            p.Height <= b.Height &&
            p.Width <= b.Width &&
            p.Length <= b.Length;
    }
}
