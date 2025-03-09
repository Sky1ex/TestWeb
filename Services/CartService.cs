﻿using Microsoft.EntityFrameworkCore;
using WebApplication1.DataBase;
using WebApplication1.DTO;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CartDto> GetCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartElement)
                .ThenInclude(ce => ce.Product)
                .FirstOrDefaultAsync(c => c.User.UserId == userId);

            if (cart == null)
            {
                _logger.LogWarning("Корзина для пользователя {UserId} не найдена.", userId);
                return null;
            }

            return new CartDto
            {
                CartId = cart.CartId,
                Products = cart.CartElement.Select(ce => new CartProductDto
                {
                    ProductId = ce.Product.ProductId,
                    Name = ce.Product.Name,
                    Price = ce.Product.Price,
                    Count = ce.Count
                }).ToList()
            };
        }

        public async Task AddToCartAsync(Guid userId, Guid productId, int count)
        {
            var cart = await _context.Carts
                .Include(c => c.CartElement)
                .ThenInclude(ce => ce.Product)
                .FirstOrDefaultAsync(c => c.User.UserId == userId);

            if (cart == null)
            {
                _logger.LogWarning("Корзина для пользователя {UserId} не найдена.", userId);
                throw new InvalidOperationException("Корзина не найдена.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Товар {ProductId} не найден.", productId);
                throw new InvalidOperationException("Товар не найден.");
            }

            var cartElement = cart.CartElement.FirstOrDefault(ce => ce.Product.ProductId == productId);
            if (cartElement != null)
            {
                cartElement.Count += count;
            }
            else
            {
                cart.CartElement.Add(new CartElement
                {
                    CartElementId = Guid.NewGuid(),
                    Product = product,
                    Count = count
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(Guid userId, Guid productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartElement)
                .FirstOrDefaultAsync(c => c.User.UserId == userId);

            if (cart == null)
            {
                _logger.LogWarning("Корзина для пользователя {UserId} не найдена.", userId);
                throw new InvalidOperationException("Корзина не найдена.");
            }

            var cartElement = cart.CartElement.FirstOrDefault(ce => ce.Product.ProductId == productId);
            if (cartElement != null)
            {
                cart.CartElement.Remove(cartElement);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartElement)
                .FirstOrDefaultAsync(c => c.User.UserId == userId);

            if (cart == null)
            {
                _logger.LogWarning("Корзина для пользователя {UserId} не найдена.", userId);
                throw new InvalidOperationException("Корзина не найдена.");
            }

            cart.CartElement.Clear();
            await _context.SaveChangesAsync();
        }

        public async Task<OrderDto> CheckoutAsync(Guid userId, Guid addressId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartElement)
                .ThenInclude(ce => ce.Product)
                .FirstOrDefaultAsync(c => c.User.UserId == userId);

            if (cart == null || !cart.CartElement.Any())
            {
                _logger.LogWarning("Корзина для пользователя {UserId} пуста.", userId);
                throw new InvalidOperationException("Корзина пуста.");
            }

            var address = await _context.Adresses.FindAsync(addressId);
            if (address == null)
            {
                _logger.LogWarning("Адрес {AddressId} не найден.", addressId);
                throw new InvalidOperationException("Адрес не найден.");
            }

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                dateTime = DateTimeOffset.UtcNow,
                Adress = address,
                OrderElement = cart.CartElement.Select(ce => new OrderElement
                {
                    OrderElementId = Guid.NewGuid(),
                    Product = ce.Product,
                    Count = ce.Count
                }).ToList()
            };

            _context.Orders.Add(order);
            cart.CartElement.Clear();
            await _context.SaveChangesAsync();

            return new OrderDto
            {
                OrderId = order.OrderId,
                DateTime = order.dateTime,
                Address = new AddressDto
                {
                    City = address.City,
                    Street = address.Street,
                    House = address.House
                },
                Products = order.OrderElement.Select(oe => new OrderProductDto
                {
                    ProductId = oe.Product.ProductId,
                    Name = oe.Product.Name,
                    Price = oe.Product.Price,
                    Count = oe.Count
                }).ToList()
            };
        }

    }
}
