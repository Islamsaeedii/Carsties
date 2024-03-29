﻿using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionController(AuctionDbContext context, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await context.Auctions
                                .Include(a => a.Item)
                                .OrderBy(x => x.Item.Make)
                                .ToListAsync();

            return mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await context.Auctions
                                .Include(a => a.Item)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();

            return mapper.Map<AuctionDto>(auction);
        }
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = mapper.Map<Auction>(createAuctionDto);
            //TODO: add current user as seller
            auction.Seller = "test";

            context.Auctions.Add(auction);

            var result = await context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, mapper.Map<AuctionDto>(auction));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await context.Auctions
                         .Include(x => x.Item)
                         .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();
            //TODO: check seller == username
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            var result = await context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Problem saving changes.");
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();
            // TODO: check seller == username
            context.Remove(auction);
            var result = await context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Could not update DB.");
            return Ok();
        }
    }
}
