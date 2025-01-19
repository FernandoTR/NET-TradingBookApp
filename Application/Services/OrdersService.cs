using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    public OrdersService(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }
    public async Task<List<GetOrdersDataTableDto>> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _ordersRepository.GetOrdersDataTableAsync(parameters);
    }



}
