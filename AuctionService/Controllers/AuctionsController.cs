using System;
using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;
[ApiController]
[Route("api/Auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>>GetAllAuctions()
    {
        var auctions = await _context.Auctions.Include(x=>x.Item)
        .OrderBy(x=>x.Item.Make)
        .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>>GetAutcionByID(Guid id)
    {
        var auction=await _context.Auctions.Include(x=>x.Item).FirstOrDefaultAsync(x=>x.Id==id);
        if(auction==null) return NotFound();
        return _mapper.Map<AuctionDto>(auction);
    }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>>CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction=_mapper.Map<Auction>(auctionDto);
        //TODO : Add Current user as seller
        auction.Seller="test";
        _context.Add(auction);
        var result=await _context.SaveChangesAsync()>0;
        if(!result) return BadRequest("Could not save changes to DB");
        else return CreatedAtAction(nameof(GetAutcionByID),new {auction.Id},_mapper.Map<AuctionDto>(auction));
        
    }
    [HttpPut("{id}")]
    public async Task<ActionResult>UpdateAuction(Guid id ,UpdateAuctionDto updateAuctionDto)
    {
        var auction=await _context.Auctions.Include(x=>x.Item).FirstOrDefaultAsync(x=>x.Id==id);
        if(auction==null) return NotFound();
        auction.Item.Make=updateAuctionDto.Make??auction.Item.Make;
        auction.Item.Model=updateAuctionDto.Model??auction.Item.Model;
        auction.Item.Color=updateAuctionDto.Color??auction.Item.Color;
        auction.Item.Mileage=updateAuctionDto.Mileage??auction.Item.Mileage;
        auction.Item.Year=updateAuctionDto.Year??auction.Item.Year;
        //_context.Update(auction);
        var res =await _context.SaveChangesAsync()>0;
        if(res) return Ok();
        return BadRequest("Problem saving changes");

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult>DeleteAuction(Guid id)
    {
        var auction=await _context.Auctions.FindAsync(id);
        if(auction==null) return NotFound();
        //TODO : Check Seller = username
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync()>0;
        if(!result) return BadRequest("Could not Delete Record");
        return Ok();
    }

}
