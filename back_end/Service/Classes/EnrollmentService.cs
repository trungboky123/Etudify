using AutoMapper;
using back_end.Database;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace back_end.Service.Classes;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _context;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly SlugHelper _slugHelper;
    
    public EnrollmentService(AppDbContext context,  IEnrollmentRepository enrollmentRepository, IMapper mapper,  SlugHelper slugHelper)
    {
        _context = context;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
        _slugHelper = slugHelper;
    }

    public async Task Enroll(string userId, int itemId)
    {
        Enrollment enrollment = new Enrollment
        {
            UserId = userId,
            ItemId = itemId,
            EnrolledAt = DateTime.UtcNow,
            Status = true
        };
        
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasEnrolled(string userId, int itemId)
    {
        return await _enrollmentRepository.ExistsByItemIdAndUserIdAsync(userId, itemId);
    }

    public async Task<List<EnrollmentResponse>> GetByUserId(string userId, string? keyword, int? categoryId, string? sortBy, string? sortDir)
    {
        var query = _enrollmentRepository.GetByUserIdAsync(userId);
        query = query.Where(e => e.Status == true);

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(e => e.Item.Name.ToLower().Contains(keyword.ToLower()));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(e => e.Item.Categories.Any(cat => cat.Id == categoryId));
        }

        if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortDir))
        {
            query = (sortBy, sortDir) switch
            {
                ("enrolledAt", "asc") => query.OrderBy(e => e.EnrolledAt),
                ("enrolledAt", "desc") => query.OrderByDescending(e => e.EnrolledAt)
            };
        }

        return await query
            .Select(e => new EnrollmentResponse(
                e.ItemId,
                e.Item.Name,
                e.Item.ThumbnailUrl,
                _slugHelper.GenerateSlug(e.Item.Name),
                e.EnrolledAt,
                e.Item.Categories
                    .Select(cat => new CategoryResponse(
                        cat.Id,
                        cat.Name
                    ))
                    .ToList()
            ))
            .ToListAsync();
    }
}