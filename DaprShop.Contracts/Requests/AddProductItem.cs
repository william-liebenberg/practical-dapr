using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaprShop.Contracts.Requests;

public record AddProductItemToCart(string? ProductId, string? ProductName, decimal Price, int Quantity);
public record RemoveProductItemFromCart(string? ProductId, int Quantity);
