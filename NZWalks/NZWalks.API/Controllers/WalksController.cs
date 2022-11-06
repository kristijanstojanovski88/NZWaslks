using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;

namespace NZWalks.API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WalksController : Controller
	{
		private readonly IWalkRepository walkRepository;
		private readonly IMapper mapper;
		private readonly IRegionRepository regionRepository;
		private readonly IWalkDifficultyRepository walkDifficultyRepository;

		public WalksController(IWalkRepository walkRepository, IMapper mapper, IRegionRepository regionRepository, IWalkDifficultyRepository walkDifficultyRepository)
		{
			this.walkRepository = walkRepository;
			this.mapper = mapper;
			this.regionRepository = regionRepository;
			this.walkDifficultyRepository = walkDifficultyRepository;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllWalksAsync()
		{
			// Fetch data from database - domain walks
			var walksDomain = await walkRepository.GetAllAsync();

			// Convert domain walks to DTO Walks
			var walksDTO = mapper.Map<List<Models.DTO.Walk>>(walksDomain);

			// Return response
			return Ok(walksDTO);
		}

		[HttpGet]
		[Route("{id:guid}")]
		[ActionName("GetWalksAsync")]
		public async Task<IActionResult> GetWalksAsync(Guid id)
		{
			//get walk domain from db
			var walkDomain = await walkRepository.GetAsync(id);

			//convert domain to dto
			var walkDTO = mapper.Map<Models.DTO.Walk>(walkDomain);

			//return response
			return Ok(walkDTO);
		}

		[HttpPost]
		public async Task<IActionResult> AddWalkAsync([FromBody] Models.DTO.AddWalkRequest addWalkRequest)
		{

			if((!await ValidateAddWalkAsync(addWalkRequest)))
			{
				return BadRequest(ModelState);
			}
			//request dto to convert to domain
			var walkDomain = new Models.Domain.Walk
			{
				Length = addWalkRequest.Length,
				Name = addWalkRequest.Name,
				RegionId = addWalkRequest.RegionId,
				WalkDifficultyId = addWalkRequest.WalkDifficultyId
			};

			//pass to repo
			walkDomain = await walkRepository.AddAsync(walkDomain);

			//convert back to dto
			var walkDTO = new Models.DTO.Walk
			{
				Id = walkDomain.Id,
				Length = walkDomain.Length,
				Name = walkDomain.Name,
				RegionId = walkDomain.RegionId,
				WalkDifficultyId = walkDomain.WalkDifficultyId
			};


			//send dto back to client
			return CreatedAtAction(nameof(GetWalksAsync), new { id = walkDTO.Id }, walkDTO);
		}

		[HttpPut]
		[Route("{id:guid}")]
		public async Task<IActionResult> UpdateWalkAsync(
			[FromRoute] Guid id, [FromBody] Models.DTO.UpdateWalkRequest updateWalkRequest)
		{
			if(!(await ValidateUpdateWalkAsync(updateWalkRequest)))
			{
				return BadRequest(ModelState);
			}

			//convert dto to domain
			var walkDomain = new Models.Domain.Walk
			{
				Length = updateWalkRequest.Length,
				Name = updateWalkRequest.Name,
				RegionId = updateWalkRequest.RegionId,
				WalkDifficultyId = updateWalkRequest.WalkDifficultyId
			};
			//pass details to repo -> get domain in response or null
			walkDomain = await walkRepository.UpdateAsync(id, walkDomain);
			//handle null
			if (walkDomain == null)
			{
				return NotFound("Walk with this ID was not found");
			}//convert back to dto
			var walkDTO = new Models.DTO.Walk
			{
				Id = walkDomain.Id,
				Length = walkDomain.Length,
				Name = walkDomain.Name,
				RegionId = walkDomain.RegionId,
				WalkDifficultyId = walkDomain.WalkDifficultyId
			};
			//return response
			return Ok(walkDTO);

		}

		[HttpDelete]
		[Route("{id:guid}")]
		public async Task<IActionResult> DeleteWalkAsync(Guid id)
		{
			//repo

			var walkDomain = await walkRepository.DeleteAsync(id);
			if(walkDomain == null)
			{
				return NotFound();
			}

			var walkDTO = mapper.Map<Models.DTO.Walk>(walkDomain);
			return Ok(walkDTO);

		}




		#region Private methods
		private async Task<bool> ValidateAddWalkAsync(AddWalkRequest addWalkRequest)
		{
			if (addWalkRequest == null)
			{
				ModelState.AddModelError(nameof(addWalkRequest),
					$"{nameof(addWalkRequest)} cannot be empty");
				return false;
			}

			if(string.IsNullOrEmpty(addWalkRequest.Name))
			{
				ModelState.AddModelError(nameof(addWalkRequest.Name),
					$"{nameof(addWalkRequest.Name)} cannot be empty");
			}

			if (addWalkRequest.Length <= 0)
			{
				ModelState.AddModelError(nameof(addWalkRequest.Length),
					$"{nameof(addWalkRequest.Length)} should be greated than zero");
			}

			var region = await regionRepository.GetAsync(addWalkRequest.RegionId);
			if(region == null)
			{
				ModelState.AddModelError(nameof(addWalkRequest.RegionId),
					$"{nameof(addWalkRequest.RegionId)} is an invalid");
			}

			var walkDifficulty = await walkDifficultyRepository.GetAsync(addWalkRequest.WalkDifficultyId);
			if (walkDifficulty == null)
			{
				ModelState.AddModelError(nameof(addWalkRequest.WalkDifficultyId),
					$"{nameof(addWalkRequest.WalkDifficultyId)} is an invalid");
			}

			if(ModelState.ErrorCount > 0)
			{
				return false;
			}

			return true;
		}

		private async Task<bool> ValidateUpdateWalkAsync(Models.DTO.UpdateWalkRequest updateWalkRequest)
		{
			if (updateWalkRequest == null)
			{
				ModelState.AddModelError(nameof(updateWalkRequest),
					$"{nameof(updateWalkRequest)} cannot be empty");
				return false;
			}

			if (string.IsNullOrEmpty(updateWalkRequest.Name))
			{
				ModelState.AddModelError(nameof(updateWalkRequest.Name),
					$"{nameof(updateWalkRequest.Name)} cannot be empty");
			}

			if (updateWalkRequest.Length <= 0)
			{
				ModelState.AddModelError(nameof(updateWalkRequest.Length),
					$"{nameof(updateWalkRequest.Length)} should be greated than zero");
			}

			var region = await regionRepository.GetAsync(updateWalkRequest.RegionId);
			if (region == null)
			{
				ModelState.AddModelError(nameof(updateWalkRequest.RegionId),
					$"{nameof(updateWalkRequest.RegionId)} is an invalid");
			}

			var walkDifficulty = await walkDifficultyRepository.GetAsync(updateWalkRequest.WalkDifficultyId);
			if (walkDifficulty == null)
			{
				ModelState.AddModelError(nameof(updateWalkRequest.WalkDifficultyId),
					$"{nameof(updateWalkRequest.WalkDifficultyId)} is an invalid");
			}

			if (ModelState.ErrorCount > 0)
			{
				return false;
			}

			return true;
		}

		#endregion



	}



}
