﻿using WebApplication1.DTO;

namespace WebApplication1.Services
{
    public interface IAccountService
    {
        List<AddressDto> GetAddresses(Guid userId);

        List<OrderDto> GetOrders(Guid userId);
    }
}
