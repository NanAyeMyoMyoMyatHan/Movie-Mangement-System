using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Ratings;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.WebApi.Features.Ratings
{
    public class RatingServices:IRatingInterface
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RatingServices(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            _context = context;
        }
        public async Task<List<RatingModel>> GetRatingAsync(int userId)
        {
            var rating =await _context.Ratings
                .Where(x=>x.UserId == userId && !x.IsDeleted).
                Select(x=>new RatingModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    MovieID = x.MovieId,
                    Score = x.Score,
                    Review = x.Review,
                    CreatedAt = x.CreatedAt,
                }).ToListAsync();
            
            return rating;
        }
        public async Task<RatingCreateResponseModel> CreateRatingAsync(RatingCreateRequestModel model)
        {
            var rating = new Rating
            {
                UserId = model.UserId,
                MovieId = model.MovieID,
                Score = model.Score,
                Review= model.Review,
                CreatedAt = model.CreatedAt,
                IsDeleted = false
            };
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            return new RatingCreateResponseModel
            {
                IsSuccess = true,
                Message = "Create Successful"
            };
        }

        public async Task<RatingUpdateResponseModel> UpdateRatingAsync(RatingUpdateRequestModel model)
        {
            var rating = await _context.Ratings.FindAsync(model.Id);
            if(rating == null)
            {
                return new RatingUpdateResponseModel
                {
                    IsSuccess = false,
                    Message = "Rating Not Found"
                };
            }
            rating.Score = model.Score;
            rating.Review = model.Review;
            rating.UpdatedAt = model.UpdatedAt ?? DateTime.UtcNow;
            _context.Ratings.Update(rating);
            await _context.SaveChangesAsync();
            return new RatingUpdateResponseModel
            {
                IsSuccess = true,
                Message = "Update Success"
            };
        }

        public async Task<RatingDeleteResponseModel> DeleteRaingAsync(RatingDeleteRequestModel model)
        {
            var rating = await _context.Ratings.FindAsync(model.Id);
            if( rating == null)
            {
                return new RatingDeleteResponseModel
                {
                    IsSuccess = false,
                    Message = "Rating Not Found"
                };
            }
            rating.IsDeleted = true;
            rating.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new RatingDeleteResponseModel
            {
                IsSuccess = true,
                Message = "Delete Success"
            };
        }

    }
}
